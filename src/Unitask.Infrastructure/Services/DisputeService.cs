using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs.Contracts;
using Unitask.Domain.Entities;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Infrastructure.Services;

/// <summary>Hiện thực quy trình tranh chấp B1–B4 (xem <see cref="IDisputeService"/>).</summary>
public class DisputeService : IDisputeService
{
    private readonly UnitaskDbContext _db;

    public DisputeService(UnitaskDbContext db)
    {
        _db = db;
    }

    private static class S { public const string Negotiation = "NEGOTIATION", Mediation = "MEDIATION", Resolved = "RESOLVED", Appeal = "APPEAL", Closed = "CLOSED"; }
    private static class D { public const string Release = "RELEASE", Refund = "REFUND", Split = "SPLIT"; }

    // B1 — mở tranh chấp
    public async Task<DisputeResponse> OpenAsync(OpenDisputeRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request?.Reason))
            throw new InvalidOperationException("Vui lòng nhập lý do tranh chấp.");

        var milestone = await _db.Milestones.Include(m => m.Contract)
            .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, ct)
            ?? throw new KeyNotFoundException("Milestone not found.");
        var contract = milestone.Contract;

        await EnsureParticipantAsync(contract, currentUserId, ct);

        // Chỉ tranh chấp khi task đang trong tiến trình (đã ký quỹ).
        if (milestone.Status is "PENDING" or "COMPLETED" or "CANCELED")
            throw new InvalidOperationException($"Không thể mở tranh chấp cho task ở trạng thái {milestone.Status}.");

        var existing = await _db.Disputes.AnyAsync(d => d.MilestoneId == milestone.Id
            && (d.Status == S.Negotiation || d.Status == S.Mediation || d.Status == S.Appeal), ct);
        if (existing)
            throw new InvalidOperationException("Đã có tranh chấp đang mở cho task này.");

        var dispute = new Dispute
        {
            Id = Guid.NewGuid(),
            MilestoneId = milestone.Id,
            ContractId = contract.Id,
            RaisedByUserId = currentUserId,
            Reason = request.Reason.Trim(),
            Status = S.Negotiation,
            CreatedAt = DateTime.UtcNow
        };
        _db.Disputes.Add(dispute);
        await NotifyBothAsync(contract, currentUserId, "⚠️ Tranh chấp mới",
            $"Có tranh chấp cho task \"{milestone.Title}\". Hai bên thương lượng qua chat (tối đa 48h) trước khi yêu cầu hòa giải.", ct);
        await _db.SaveChangesAsync(ct);
        return Map(dispute, milestone.Title);
    }

    // B2 — yêu cầu hòa giải
    public async Task<DisputeResponse> RequestMediationAsync(Guid disputeId, Guid currentUserId, CancellationToken ct = default)
    {
        var (dispute, milestone, contract) = await LoadAsync(disputeId, ct);
        await EnsureParticipantAsync(contract, currentUserId, ct);
        if (dispute.Status != S.Negotiation)
            throw new InvalidOperationException($"Chỉ yêu cầu hòa giải khi đang thương lượng (hiện tại: {dispute.Status}).");

        dispute.Status = S.Mediation;
        await NotifyBothAsync(contract, currentUserId, "🧑‍⚖️ Đã yêu cầu hòa giải",
            $"Tranh chấp task \"{milestone.Title}\" chuyển sang hòa giải. UniTask sẽ chỉ định hòa giải viên trong 24h.", ct);
        await _db.SaveChangesAsync(ct);
        return Map(dispute, milestone.Title);
    }

    // B3 — hòa giải viên (admin) ra quyết định + tác động Escrow
    public async Task<DisputeResponse> ResolveAsync(Guid disputeId, Guid mediatorUserId, ResolveDisputeRequest request, CancellationToken ct = default)
    {
        var isAdmin = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == mediatorUserId && u.UserType == "admin", ct);
        if (!isAdmin)
            throw new UnauthorizedAccessException("Chỉ hòa giải viên (admin) được ra quyết định.");

        var decision = (request?.Decision ?? string.Empty).ToUpperInvariant();
        if (decision is not (D.Release or D.Refund or D.Split))
            throw new InvalidOperationException("Decision phải là RELEASE, REFUND hoặc SPLIT.");

        var (dispute, milestone, contract) = await LoadAsync(disputeId, ct);
        if (dispute.Status is S.Resolved or S.Closed)
            throw new InvalidOperationException("Tranh chấp đã được giải quyết.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var amount = milestone.Amount;
        var percent = decision == D.Split ? Math.Clamp(request!.StudentPercent ?? 0, 0, 100)
                    : decision == D.Release ? 100 : 0;
        var studentAmount = Math.Round(amount * percent / 100m, 2);
        var businessRefund = amount - studentAmount;

        // Chỉ tác động tiền nếu milestone vẫn đang giữ escrow.
        if (milestone.Status is not ("COMPLETED" or "CANCELED"))
        {
            if (studentAmount > 0)
            {
                var wallet = await _db.StudentWallets.FirstOrDefaultAsync(w => w.StudentId == contract.StudentId, ct);
                if (wallet is null)
                {
                    wallet = new StudentWallet { Id = Guid.NewGuid(), StudentId = contract.StudentId, Balance = 0m, TotalEarned = 0m, TotalWithdrawn = 0m, UpdatedAt = DateTime.UtcNow };
                    _db.StudentWallets.Add(wallet);
                }
                wallet.Balance = (wallet.Balance ?? 0m) + studentAmount;
                wallet.TotalEarned = (wallet.TotalEarned ?? 0m) + studentAmount;
                wallet.UpdatedAt = DateTime.UtcNow;
            }

            var business = await _db.BusinessProfiles.FirstAsync(b => b.Id == contract.BusinessId, ct);
            if (businessRefund > 0) business.Balance = (business.Balance ?? 0m) + businessRefund;
            if (studentAmount > 0) business.TotalSpent = (business.TotalSpent ?? 0m) + studentAmount;
            business.UpdatedAt = DateTime.UtcNow;

            // Ghi lịch sử dòng tiền: phần trả cho người thực hiện theo quyết định.
            if (studentAmount > 0)
            {
                var appId = await _db.JobApplications.AsNoTracking()
                    .Where(a => a.JobId == contract.JobId && a.StudentId == contract.StudentId)
                    .Select(a => (Guid?)a.Id).FirstOrDefaultAsync(ct);
                if (appId is not null)
                {
                    _db.Payments.Add(new Payment
                    {
                        Id = Guid.NewGuid(), JobId = contract.JobId, JobApplicationId = appId.Value,
                        BusinessId = contract.BusinessId, StudentId = contract.StudentId, Amount = studentAmount,
                        Currency = "VND", Status = "released", PaymentMethod = "dispute",
                        Description = $"Tranh chấp {decision}: {milestone.Title}",
                        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, ReleasedAt = DateTime.UtcNow
                    });
                }
            }

            milestone.Status = decision == D.Release ? "COMPLETED" : "CANCELED";
        }

        dispute.Status = S.Resolved;
        dispute.Decision = decision;
        dispute.StudentPercent = percent;
        dispute.DecisionNote = request?.Note;
        dispute.MediatorId = mediatorUserId;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.AppealDeadline = DateTime.UtcNow.AddDays(7); // B4: 7 ngày kháng cáo

        await NotifyBothAsync(contract, mediatorUserId, "⚖️ Quyết định tranh chấp",
            $"Task \"{milestone.Title}\": {decision} (người làm nhận {percent}%). Có hiệu lực ngay; được kháng cáo trong 7 ngày.", ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Map(dispute, milestone.Title);
    }

    // B4 — kháng cáo
    public async Task<DisputeResponse> AppealAsync(Guid disputeId, Guid currentUserId, AppealDisputeRequest request, CancellationToken ct = default)
    {
        var (dispute, milestone, contract) = await LoadAsync(disputeId, ct);
        await EnsureParticipantAsync(contract, currentUserId, ct);
        if (dispute.Status != S.Resolved)
            throw new InvalidOperationException("Chỉ kháng cáo quyết định đã ban hành.");
        if (dispute.AppealDeadline is { } dl && DateTime.UtcNow > dl)
            throw new InvalidOperationException("Đã quá hạn kháng cáo (7 ngày).");

        dispute.Status = S.Appeal;
        if (!string.IsNullOrWhiteSpace(request?.Reason))
            dispute.DecisionNote = (dispute.DecisionNote ?? "") + $"\n[Kháng cáo] {request!.Reason!.Trim()}";
        await NotifyBothAsync(contract, currentUserId, "📨 Kháng cáo tranh chấp",
            $"Quyết định cho task \"{milestone.Title}\" đã bị kháng cáo. Ban phúc thẩm UniTask sẽ xem xét.", ct);
        await _db.SaveChangesAsync(ct);
        return Map(dispute, milestone.Title);
    }

    public async Task<IReadOnlyList<DisputeResponse>> GetByContractAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default)
    {
        var contract = await _db.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == contractId, ct)
            ?? throw new KeyNotFoundException("Contract not found.");
        await EnsureParticipantAsync(contract, currentUserId, ct);

        var rows = await _db.Disputes.AsNoTracking()
            .Where(d => d.ContractId == contractId)
            .Join(_db.Milestones.AsNoTracking(), d => d.MilestoneId, m => m.Id, (d, m) => new { d, m.Title })
            .OrderByDescending(x => x.d.CreatedAt)
            .ToListAsync(ct);
        return rows.Select(x => Map(x.d, x.Title)).ToList();
    }

    // ----- helpers -----
    private async Task<(Dispute dispute, Milestone milestone, Contract contract)> LoadAsync(Guid disputeId, CancellationToken ct)
    {
        var dispute = await _db.Disputes.FirstOrDefaultAsync(d => d.Id == disputeId, ct)
            ?? throw new KeyNotFoundException("Dispute not found.");
        var milestone = await _db.Milestones.Include(m => m.Contract).FirstAsync(m => m.Id == dispute.MilestoneId, ct);
        return (dispute, milestone, milestone.Contract);
    }

    private async Task EnsureParticipantAsync(Contract contract, Guid userId, CancellationToken ct)
    {
        var isBusiness = await _db.BusinessProfiles.AsNoTracking().AnyAsync(b => b.Id == contract.BusinessId && b.UserId == userId, ct);
        var isStudent = await _db.StudentProfiles.AsNoTracking().AnyAsync(s => s.Id == contract.StudentId && s.UserId == userId, ct);
        if (!isBusiness && !isStudent)
            throw new UnauthorizedAccessException("Bạn không thuộc hợp đồng này.");
    }

    private async Task NotifyBothAsync(Contract contract, Guid actorUserId, string title, string message, CancellationToken ct)
    {
        var studentUserId = await _db.StudentProfiles.AsNoTracking().Where(s => s.Id == contract.StudentId).Select(s => s.UserId).FirstAsync(ct);
        var businessUserId = await _db.BusinessProfiles.AsNoTracking().Where(b => b.Id == contract.BusinessId).Select(b => b.UserId).FirstAsync(ct);
        foreach (var uid in new[] { studentUserId, businessUserId })
        {
            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(), UserId = uid, Type = "dispute", Title = title, Message = message,
                RelatedJobId = contract.JobId, RelatedUserId = actorUserId, IsRead = false, CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static DisputeResponse Map(Dispute d, string? milestoneTitle) => new()
    {
        Id = d.Id,
        MilestoneId = d.MilestoneId,
        MilestoneTitle = milestoneTitle,
        ContractId = d.ContractId,
        RaisedByUserId = d.RaisedByUserId,
        Reason = d.Reason,
        Status = d.Status,
        Decision = d.Decision,
        StudentPercent = d.StudentPercent,
        DecisionNote = d.DecisionNote,
        CreatedAt = d.CreatedAt,
        ResolvedAt = d.ResolvedAt,
        AppealDeadline = d.AppealDeadline
    };
}
