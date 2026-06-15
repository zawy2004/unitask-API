using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs.Contracts;
using Unitask.Domain.Entities;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Infrastructure.Services;

/// <summary>
/// Hiện thực nghiệp vụ Hợp đồng & Milestone.
///
/// LUỒNG TỔNG QUÁT của một milestone:
///   1) Business gọi /escrow   : PENDING        → ESCROWED      (nạp tiền ký quỹ, giả lập cổng TT)
///   2) Student  gọi /submit   : ESCROWED|REVISION → UNDER_REVIEW (tạo bản ghi Submission)
///   3a) Business gọi /approve : UNDER_REVIEW    → COMPLETED     (giải ngân vào ví Student)
///   3b) Business gọi /request-changes : UNDER_REVIEW → REVISION (ghi ClientFeedback) → quay lại (2)
/// </summary>
public class MilestoneService : IMilestoneService
{
    private readonly UnitaskDbContext _db;

    public MilestoneService(UnitaskDbContext db)
    {
        _db = db;
    }

    // ----- Hằng số trạng thái: tránh "magic string" rải rác -----
    private static class MilestoneStatus
    {
        public const string Pending = "PENDING";
        public const string Escrowed = "ESCROWED";
        public const string UnderReview = "UNDER_REVIEW";
        public const string Revision = "REVISION";
        public const string Completed = "COMPLETED";
        public const string Canceled = "CANCELED";
    }

    private static class ContractStatus
    {
        public const string Active = "ACTIVE";
        public const string Completed = "COMPLETED";
        public const string Canceled = "CANCELED";
    }

    // ============================================================
    // TẠO HỢP ĐỒNG — Business duyệt ứng tuyển → sinh Contract + Milestones
    // ============================================================

    public async Task<ContractResponse> CreateContractAsync(CreateContractRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        if (request is null || request.JobApplicationId == Guid.Empty)
            throw new InvalidOperationException("Thiếu JobApplicationId.");
        if (request.Milestones is null || request.Milestones.Count == 0)
            throw new InvalidOperationException("Hợp đồng phải có ít nhất 1 milestone.");

        // Nạp đơn ứng tuyển + Job (để biết BusinessId / StudentId).
        var application = await _db.JobApplications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == request.JobApplicationId, ct)
            ?? throw new KeyNotFoundException("JobApplication not found.");

        // GUARD trạng thái: chỉ tạo hợp đồng khi đơn đã được duyệt.
        if (!string.Equals(application.Status, "accepted", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Chỉ tạo hợp đồng từ đơn đã 'accepted' (hiện tại: {application.Status}).");

        // GUARD quyền: currentUser phải là Business sở hữu Job của đơn này.
        var businessId = application.Job.BusinessId;
        await EnsureBusinessOwnerAsync(businessId, currentUserId, ct);

        // Chống tạo trùng: 1 (Job, Student) chỉ nên có 1 hợp đồng.
        var duplicated = await _db.Contracts
            .AnyAsync(c => c.JobId == application.JobId && c.StudentId == application.StudentId, ct);
        if (duplicated)
            throw new InvalidOperationException("Hợp đồng cho ứng viên này trên job đã tồn tại.");

        // FinalPrice = giá truyền vào, hoặc mặc định = tổng các milestone.
        var milestonesTotal = request.Milestones.Sum(m => m.Amount);
        var finalPrice = request.FinalPrice ?? milestonesTotal;

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            JobId = application.JobId,
            StudentId = application.StudentId,
            BusinessId = businessId,
            FinalPrice = finalPrice,
            Status = ContractStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Milestones = request.Milestones.Select(m => new Milestone
            {
                Id = Guid.NewGuid(),
                Title = m.Title,
                Amount = m.Amount,
                Status = MilestoneStatus.Pending,
                DueDate = m.DueDate,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };

        _db.Contracts.Add(contract);

        // Thông báo cho sinh viên: có hợp đồng mới.
        var (studentUserId, _) = await ResolveUserIdsAsync(contract, ct);
        QueueNotification(studentUserId, currentUserId, "contract",
            "🤝 Hợp đồng mới",
            $"Bạn nhận hợp đồng mới cho \"{application.Job.Title}\" gồm {contract.Milestones.Count} task.",
            contract.JobId);

        await _db.SaveChangesAsync(ct);

        // Trả về đầy đủ (tái sử dụng hàm đọc + map).
        return await GetContractAsync(contract.Id, currentUserId, ct);
    }

    public async Task<MilestoneResponse> AddMilestoneAsync(Guid contractId, Guid currentUserId, CreateMilestoneRequest request, CancellationToken ct = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Title))
            throw new InvalidOperationException("Tên task (milestone) không được để trống.");
        if (request.Amount <= 0)
            throw new InvalidOperationException("Số tiền milestone phải lớn hơn 0.");

        var contract = await _db.Contracts.FirstOrDefaultAsync(c => c.Id == contractId, ct)
            ?? throw new KeyNotFoundException("Contract not found.");

        // GUARD: chỉ Business chủ hợp đồng được giao task.
        await EnsureBusinessOwnerAsync(contract.BusinessId, currentUserId, ct);

        // Chỉ cấm khi hợp đồng đã HỦY. Hợp đồng đã hoàn thành hết task vẫn được giao thêm
        // (mở lại trạng thái ACTIVE) — doanh nghiệp có thể giao task tiếp trong thời hạn hợp đồng.
        if (contract.Status == ContractStatus.Canceled)
            throw new InvalidOperationException("Hợp đồng đã hủy, không thể thêm task.");
        if (contract.Status == ContractStatus.Completed)
            contract.Status = ContractStatus.Active;

        var milestone = new Milestone
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            Title = request.Title,
            Amount = request.Amount,
            Status = MilestoneStatus.Pending,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow
        };
        _db.Milestones.Add(milestone);

        // Cập nhật lại giá trị hợp đồng = tổng các milestone.
        contract.FinalPrice += request.Amount;

        // Thông báo cho sinh viên: được giao task mới.
        var (studentUserId, _) = await ResolveUserIdsAsync(contract, ct);
        QueueNotification(studentUserId, currentUserId, "milestone",
            "📋 Task mới được giao",
            $"Bạn được giao task \"{milestone.Title}\" ({Vnd(milestone.Amount)}).",
            contract.JobId);

        await _db.SaveChangesAsync(ct);
        return await ReloadMilestoneAsync(milestone.Id, ct);
    }

    // ============================================================
    // ĐỌC DỮ LIỆU
    // ============================================================

    public async Task<ContractResponse> GetContractAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default)
    {
        var contract = await _db.Contracts.AsNoTracking()
            .Include(c => c.Job)
            .Include(c => c.Student).ThenInclude(s => s.User)
            .Include(c => c.Milestones).ThenInclude(m => m.Submissions)
            .FirstOrDefaultAsync(c => c.Id == contractId, ct)
            ?? throw new KeyNotFoundException("Contract not found.");

        // Cả Business chủ HĐ lẫn Student của HĐ đều được xem.
        await EnsureContractParticipantAsync(contract.StudentId, contract.BusinessId, currentUserId, ct);

        // Người xem là doanh nghiệp chủ HĐ? Nếu không (sinh viên) -> ẩn task chưa ký quỹ (1.1).
        var isBusinessViewer = await _db.BusinessProfiles.AsNoTracking()
            .AnyAsync(b => b.Id == contract.BusinessId && b.UserId == currentUserId, ct);
        return MapContract(contract, hidePending: !isBusinessViewer);
    }

    public async Task<IReadOnlyList<ContractResponse>> GetMyContractsAsync(Guid currentUserId, CancellationToken ct = default)
    {
        // Xác định profile của người dùng (có thể là Business hoặc Student).
        var businessId = await _db.BusinessProfiles.AsNoTracking()
            .Where(b => b.UserId == currentUserId).Select(b => (Guid?)b.Id).FirstOrDefaultAsync(ct);
        var studentId = await _db.StudentProfiles.AsNoTracking()
            .Where(s => s.UserId == currentUserId).Select(s => (Guid?)s.Id).FirstOrDefaultAsync(ct);

        if (businessId is null && studentId is null)
            return Array.Empty<ContractResponse>();

        var contracts = await _db.Contracts.AsNoTracking()
            .Include(c => c.Job)
            .Include(c => c.Student).ThenInclude(s => s.User)
            .Include(c => c.Milestones).ThenInclude(m => m.Submissions)
            .Where(c => (businessId != null && c.BusinessId == businessId) || (studentId != null && c.StudentId == studentId))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        // Sinh viên (không phải doanh nghiệp) -> ẩn task chưa ký quỹ (1.1).
        var hidePending = businessId is null;
        return contracts.Select(c => MapContract(c, hidePending)).ToList();
    }

    public async Task<ContractResponse> GetContractByApplicationAsync(Guid jobApplicationId, Guid currentUserId, CancellationToken ct = default)
    {
        var application = await _db.JobApplications.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == jobApplicationId, ct)
            ?? throw new KeyNotFoundException("JobApplication not found.");

        var contract = await _db.Contracts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.JobId == application.JobId && c.StudentId == application.StudentId, ct)
            ?? throw new KeyNotFoundException("Contract not found for this application.");

        // Tái dùng GetContractAsync để có guard quyền + map đầy đủ.
        return await GetContractAsync(contract.Id, currentUserId, ct);
    }

    public async Task<IReadOnlyList<MilestoneResponse>> GetMilestonesAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default)
    {
        var contract = await GetContractAsync(contractId, currentUserId, ct);
        return contract.Milestones;
    }

    // ============================================================
    // (1) ESCROW — Business nạp tiền ký quỹ: PENDING → ESCROWED
    // ============================================================

    public async Task<MilestoneResponse> EscrowAsync(Guid milestoneId, Guid currentUserId, EscrowMilestoneRequest? request, CancellationToken ct = default)
    {
        var (milestone, contract) = await LoadMilestoneWithContractAsync(milestoneId, ct);

        // GUARD: đúng Business chủ hợp đồng.
        await EnsureBusinessOwnerAsync(contract.BusinessId, currentUserId, ct);

        // GUARD trạng thái: chỉ ký quỹ khi đang PENDING.
        if (milestone.Status != MilestoneStatus.Pending)
            throw new InvalidOperationException($"Chỉ có thể ký quỹ milestone ở trạng thái PENDING (hiện tại: {milestone.Status}).");

        // GUARD số dư: doanh nghiệp phải có đủ tiền trong ví để ký quỹ.
        var business = await _db.BusinessProfiles.FirstAsync(b => b.Id == contract.BusinessId, ct);
        var balance = business.Balance ?? 0m;
        if (balance < milestone.Amount)
            throw new InvalidOperationException(
                $"Số dư ví doanh nghiệp không đủ để ký quỹ (cần {Vnd(milestone.Amount)}, hiện có {Vnd(balance)}). Vui lòng nạp thêm tiền.");

        // --- Giả lập gọi cổng thanh toán / webhook ---
        // Thực tế: gọi MomoService.CreatePayment(...) rồi chờ IPN xác nhận mới chuyển ESCROWED.
        var paymentRef = request?.PaymentReference ?? $"ESCROW-{milestone.Id:N}";
        _ = paymentRef; // (đã có thể lưu xuống bảng Payments nếu cần)

        // Giữ tiền ký quỹ: trừ khỏi số dư khả dụng của doanh nghiệp.
        business.Balance = balance - milestone.Amount;
        business.UpdatedAt = DateTime.UtcNow;

        milestone.Status = MilestoneStatus.Escrowed;
        milestone.EscrowedAt = DateTime.UtcNow; // mốc tính 48h cho chính sách hủy 1.3
        await AddPaymentRecordAsync(contract, milestone.Amount, "escrow", "escrow", $"Ký quỹ: {milestone.Title}", ct);

        // Thông báo cho sinh viên: đã có tiền ký quỹ, bắt đầu làm.
        var (studentUserId, _) = await ResolveUserIdsAsync(contract, ct);
        QueueNotification(studentUserId, currentUserId, "milestone",
            "🔒 Đã ký quỹ task",
            $"Doanh nghiệp đã ký quỹ \"{milestone.Title}\" ({Vnd(milestone.Amount)}). Bạn có thể bắt đầu thực hiện.",
            contract.JobId);

        await _db.SaveChangesAsync(ct);

        return await ReloadMilestoneAsync(milestone.Id, ct);
    }

    // ============================================================
    // (2) SUBMIT — Student nộp bài: ESCROWED|REVISION → UNDER_REVIEW
    // ============================================================

    public async Task<MilestoneResponse> SubmitAsync(Guid milestoneId, Guid currentUserId, SubmitMilestoneRequest request, CancellationToken ct = default)
    {
        var (milestone, contract) = await LoadMilestoneWithContractAsync(milestoneId, ct);

        // GUARD: đúng Student của hợp đồng.
        await EnsureStudentOwnerAsync(contract.StudentId, currentUserId, ct);

        // GUARD trạng thái: chỉ nộp khi đã ký quỹ (ESCROWED) hoặc đang phải sửa (REVISION).
        if (milestone.Status != MilestoneStatus.Escrowed && milestone.Status != MilestoneStatus.Revision)
            throw new InvalidOperationException($"Chỉ có thể nộp bài khi milestone ở trạng thái ESCROWED hoặc REVISION (hiện tại: {milestone.Status}).");

        // Thêm bản ghi Submission mới (giữ lịch sử các lần nộp).
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            MilestoneId = milestone.Id,
            StudentId = contract.StudentId,
            FileUrl = request?.FileUrl,
            CoverLetter = request?.CoverLetter,
            CreatedAt = DateTime.UtcNow
        };
        _db.Submissions.Add(submission);

        milestone.Status = MilestoneStatus.UnderReview;

        // Thông báo cho doanh nghiệp: có bài nộp mới cần nghiệm thu.
        var (_, businessUserId) = await ResolveUserIdsAsync(contract, ct);
        QueueNotification(businessUserId, currentUserId, "milestone",
            "📤 Sinh viên đã nộp bài",
            $"Sinh viên đã nộp bài cho task \"{milestone.Title}\". Vui lòng nghiệm thu.",
            contract.JobId);

        await _db.SaveChangesAsync(ct);

        return await ReloadMilestoneAsync(milestone.Id, ct);
    }

    // ============================================================
    // (3a) APPROVE — Business nghiệm thu: UNDER_REVIEW → COMPLETED + giải ngân ví
    // ============================================================

    public async Task<MilestoneResponse> ApproveAsync(Guid milestoneId, Guid currentUserId, CancellationToken ct = default)
    {
        var (milestone, contract) = await LoadMilestoneWithContractAsync(milestoneId, ct);

        // GUARD: đúng Business chủ hợp đồng.
        await EnsureBusinessOwnerAsync(contract.BusinessId, currentUserId, ct);

        // GUARD trạng thái: chỉ nghiệm thu khi đang chờ duyệt.
        if (milestone.Status != MilestoneStatus.UnderReview)
            throw new InvalidOperationException($"Chỉ có thể nghiệm thu milestone ở trạng thái UNDER_REVIEW (hiện tại: {milestone.Status}).");

        // Bọc trong transaction: đổi trạng thái milestone + cộng ví phải toàn-vẹn (cùng thành công/thất bại).
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        await ApplyApprovalAsync(milestone, contract, currentUserId, auto: false, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await ReloadMilestoneAsync(milestone.Id, ct);
    }

    /// <summary>
    /// Lõi nghiệm thu (dùng chung cho approve thủ công và auto-release 72h):
    /// COMPLETED + giải ngân vào ví Sinh viên + ghi TotalSpent doanh nghiệp + đóng HĐ nếu xong hết
    /// + tạo thông báo. KHÔNG SaveChanges (caller tự lưu/commit).
    /// </summary>
    /// <param name="actorUserId">User thực hiện; null nếu hệ thống tự động giải ngân.</param>
    private async Task ApplyApprovalAsync(Milestone milestone, Contract contract, Guid? actorUserId, bool auto, CancellationToken ct)
    {
        milestone.Status = MilestoneStatus.Completed;

        // --- Giải ngân tiền ký quỹ vào ví Sinh viên ---
        var wallet = await _db.StudentWallets.FirstOrDefaultAsync(w => w.StudentId == contract.StudentId, ct);
        if (wallet is null)
        {
            wallet = new StudentWallet
            {
                Id = Guid.NewGuid(),
                StudentId = contract.StudentId,
                Balance = 0m,
                TotalEarned = 0m,
                TotalWithdrawn = 0m,
                UpdatedAt = DateTime.UtcNow
            };
            _db.StudentWallets.Add(wallet);
        }

        wallet.Balance = (wallet.Balance ?? 0m) + milestone.Amount;
        wallet.TotalEarned = (wallet.TotalEarned ?? 0m) + milestone.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        // Tiền đã được giữ (trừ Balance) lúc ký quỹ -> nay ghi nhận là đã chi.
        var business = await _db.BusinessProfiles.FirstAsync(b => b.Id == contract.BusinessId, ct);
        business.TotalSpent = (business.TotalSpent ?? 0m) + milestone.Amount;
        business.UpdatedAt = DateTime.UtcNow;

        // Ghi lịch sử dòng tiền: giải ngân cho người thực hiện.
        await AddPaymentRecordAsync(contract, milestone.Amount, "released", "escrow", $"Giải ngân: {milestone.Title}", ct);

        // Nếu mọi milestone đã COMPLETED -> đóng hợp đồng.
        var allCompleted = await _db.Milestones
            .Where(m => m.ContractId == contract.Id && m.Id != milestone.Id)
            .AllAsync(m => m.Status == MilestoneStatus.Completed, ct);
        if (allCompleted)
        {
            var trackedContract = await _db.Contracts.FirstAsync(c => c.Id == contract.Id, ct);
            trackedContract.Status = ContractStatus.Completed;
        }

        // Thông báo cho sinh viên (khác nhau giữa nghiệm thu thủ công và tự động 72h).
        var (studentUserId, _) = await ResolveUserIdsAsync(contract, ct);
        var (title, message) = auto
            ? ("⏱️ Tự động nghiệm thu (72h)",
               $"Doanh nghiệp không phản hồi trong 72 giờ. Task \"{milestone.Title}\" được tự động nghiệm thu, {Vnd(milestone.Amount)} đã vào ví của bạn.")
            : ("✅ Đã nghiệm thu & giải ngân",
               $"Task \"{milestone.Title}\" đã được nghiệm thu. {Vnd(milestone.Amount)} đã vào ví của bạn.");
        QueueNotification(studentUserId, actorUserId ?? studentUserId, "payment", title, message, contract.JobId);
    }

    /// <summary>
    /// CHÍNH SÁCH 1.2 — "im lặng = chấp thuận": tự động nghiệm thu & giải ngân các milestone
    /// đang UNDER_REVIEW mà bản nộp mới nhất đã quá <paramref name="timeoutHours"/> giờ.
    /// Gọi định kỳ bởi background service. Trả về số milestone đã giải ngân.
    /// </summary>
    public async Task<int> AutoReleaseExpiredAsync(int timeoutHours, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-timeoutHours);

        var expired = await _db.Milestones
            .Include(m => m.Contract)
            .Include(m => m.Submissions)
            .Where(m => m.Status == MilestoneStatus.UnderReview
                        && m.Submissions.Any()
                        && m.Submissions.Max(s => s.CreatedAt) < cutoff)
            .ToListAsync(ct);

        if (expired.Count == 0)
            return 0;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        foreach (var milestone in expired)
        {
            // actorUserId = null -> hành động của hệ thống.
            await ApplyApprovalAsync(milestone, milestone.Contract, actorUserId: null, auto: true, ct);
        }
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return expired.Count;
    }

    // ============================================================
    // (3b) REQUEST CHANGES — Business yêu cầu sửa: UNDER_REVIEW → REVISION
    // ============================================================

    public async Task<MilestoneResponse> RequestChangesAsync(Guid milestoneId, Guid currentUserId, RequestChangesRequest request, CancellationToken ct = default)
    {
        // Chính sách 1.4: từ chối nghiệm thu phải có LÝ DO cụ thể + BẰNG CHỨNG kèm theo.
        if (string.IsNullOrWhiteSpace(request?.Feedback))
            throw new InvalidOperationException("Vui lòng nhập lý do cần chỉnh sửa (feedback).");
        if (string.IsNullOrWhiteSpace(request?.EvidenceUrl))
            throw new InvalidOperationException("Vui lòng đính kèm bằng chứng (link) khi từ chối nghiệm thu.");

        var (milestone, contract) = await LoadMilestoneWithContractAsync(milestoneId, ct);

        // GUARD: đúng Business chủ hợp đồng.
        await EnsureBusinessOwnerAsync(contract.BusinessId, currentUserId, ct);

        // GUARD trạng thái: chỉ yêu cầu sửa khi đang chờ duyệt.
        if (milestone.Status != MilestoneStatus.UnderReview)
            throw new InvalidOperationException($"Chỉ có thể yêu cầu sửa milestone ở trạng thái UNDER_REVIEW (hiện tại: {milestone.Status}).");

        // Ghi feedback vào bản nộp MỚI NHẤT để Student biết cần sửa gì.
        var latest = await _db.Submissions
            .Where(s => s.MilestoneId == milestone.Id)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Không tìm thấy bản nộp để gửi yêu cầu sửa.");

        latest.ClientFeedback = request.Feedback;
        latest.ClientEvidenceUrl = request.EvidenceUrl;
        milestone.Status = MilestoneStatus.Revision;

        // Thông báo cho sinh viên: cần chỉnh sửa, kèm lý do.
        var (studentUserId, _) = await ResolveUserIdsAsync(contract, ct);
        QueueNotification(studentUserId, currentUserId, "milestone",
            "✏️ Yêu cầu chỉnh sửa",
            $"Task \"{milestone.Title}\" cần chỉnh sửa: {request.Feedback}",
            contract.JobId);

        await _db.SaveChangesAsync(ct);

        return await ReloadMilestoneAsync(milestone.Id, ct);
    }

    // ============================================================
    // (4) CANCEL — Business hủy task: hoàn tiền theo % tiến độ (chính sách 1.3)
    // ============================================================

    public async Task<MilestoneResponse> CancelMilestoneAsync(Guid milestoneId, Guid currentUserId, CancelMilestoneRequest request, CancellationToken ct = default)
    {
        var (milestone, contract) = await LoadMilestoneWithContractAsync(milestoneId, ct);

        // GUARD: đúng Business chủ hợp đồng.
        await EnsureBusinessOwnerAsync(contract.BusinessId, currentUserId, ct);

        if (milestone.Status == MilestoneStatus.Completed || milestone.Status == MilestoneStatus.Canceled)
            throw new InvalidOperationException($"Không thể hủy task ở trạng thái {milestone.Status}.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var (studentUserId, businessUserId) = await ResolveUserIdsAsync(contract, ct);

        // Task CHƯA ký quỹ (PENDING): không có tiền giữ -> chỉ hủy.
        if (milestone.Status == MilestoneStatus.Pending)
        {
            milestone.Status = MilestoneStatus.Canceled;
            QueueNotification(studentUserId, currentUserId, "milestone",
                "🚫 Task đã bị hủy", $"Task \"{milestone.Title}\" đã bị doanh nghiệp hủy.", contract.JobId);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return await ReloadMilestoneAsync(milestone.Id, ct);
        }

        // Đã ký quỹ (ESCROWED/UNDER_REVIEW/REVISION): chia tiền theo % tiến độ.
        var percent = Math.Clamp(request?.ProgressPercent ?? 0, 0, 100);

        // Chính sách 1.3: tối thiểu 30% cho người thực hiện nếu đã qua 48h kể từ lúc ký quỹ.
        if (milestone.EscrowedAt is { } escrowedAt && DateTime.UtcNow - escrowedAt > TimeSpan.FromHours(48))
            percent = Math.Max(percent, 30);

        var studentAmount = Math.Round(milestone.Amount * percent / 100m, 2);
        var businessRefund = milestone.Amount - studentAmount;

        // Trả phần tiến độ cho người thực hiện.
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

        // Hoàn phần còn lại + ghi nhận phần đã chi cho doanh nghiệp.
        var business = await _db.BusinessProfiles.FirstAsync(b => b.Id == contract.BusinessId, ct);
        business.Balance = (business.Balance ?? 0m) + businessRefund;
        if (studentAmount > 0)
            business.TotalSpent = (business.TotalSpent ?? 0m) + studentAmount;
        business.UpdatedAt = DateTime.UtcNow;

        // Ghi lịch sử: phần trả cho người thực hiện theo tiến độ (nếu có).
        await AddPaymentRecordAsync(contract, studentAmount, "released", "escrow", $"Hủy task (trả {percent}% tiến độ): {milestone.Title}", ct);

        milestone.Status = MilestoneStatus.Canceled;

        QueueNotification(studentUserId, currentUserId, "payment",
            "🚫 Task bị hủy — thanh toán theo tiến độ",
            $"Task \"{milestone.Title}\" bị hủy. Bạn nhận {Vnd(studentAmount)} ({percent}% tiến độ).", contract.JobId);
        QueueNotification(businessUserId, currentUserId, "payment",
            "🚫 Đã hủy task — hoàn quỹ",
            $"Task \"{milestone.Title}\" đã hủy. Hoàn {Vnd(businessRefund)} về ví, trả {Vnd(studentAmount)} cho người thực hiện.", contract.JobId);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await ReloadMilestoneAsync(milestone.Id, ct);
    }

    // ============================================================
    // HÀM DÙNG CHUNG
    // ============================================================

    /// <summary>Nạp milestone (tracked) cùng contract của nó; ném 404 nếu không có.</summary>
    private async Task<(Milestone milestone, Contract contract)> LoadMilestoneWithContractAsync(Guid milestoneId, CancellationToken ct)
    {
        var milestone = await _db.Milestones
            .Include(m => m.Contract)
            .FirstOrDefaultAsync(m => m.Id == milestoneId, ct)
            ?? throw new KeyNotFoundException("Milestone not found.");

        return (milestone, milestone.Contract);
    }

    /// <summary>Đọc lại milestone (no-tracking) + bản nộp mới nhất để trả về client.</summary>
    private async Task<MilestoneResponse> ReloadMilestoneAsync(Guid milestoneId, CancellationToken ct)
    {
        var milestone = await _db.Milestones.AsNoTracking()
            .Include(m => m.Submissions)
            .FirstAsync(m => m.Id == milestoneId, ct);
        return MapMilestone(milestone);
    }

    // ----- THÔNG BÁO (dbo.Notifications) -----

    /// <summary>Lấy User.Id của Student và Business đứng sau hợp đồng (recipient của thông báo).</summary>
    private async Task<(Guid studentUserId, Guid businessUserId)> ResolveUserIdsAsync(Contract contract, CancellationToken ct)
    {
        var studentUserId = await _db.StudentProfiles.AsNoTracking()
            .Where(s => s.Id == contract.StudentId).Select(s => s.UserId).FirstAsync(ct);
        var businessUserId = await _db.BusinessProfiles.AsNoTracking()
            .Where(b => b.Id == contract.BusinessId).Select(b => b.UserId).FirstAsync(ct);
        return (studentUserId, businessUserId);
    }

    /// <summary>
    /// Xếp 1 thông báo vào dbo.Notifications (chưa SaveChanges — sẽ lưu cùng hành động đổi
    /// trạng thái để đảm bảo thông báo và state thay đổi đồng bộ trong cùng 1 transaction).
    /// </summary>
    private void QueueNotification(Guid recipientUserId, Guid actorUserId, string type, string title, string message, Guid? relatedJobId)
    {
        _db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            RelatedJobId = relatedJobId,
            RelatedUserId = actorUserId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static string Vnd(decimal v) => v.ToString("#,##0", new CultureInfo("vi-VN")) + "₫";

    // ----- LỊCH SỬ DÒNG TIỀN (dbo.Payments) -----

    /// <summary>Tìm JobApplication ứng với hợp đồng (theo Job + Student) để gắn vào Payment.</summary>
    private Task<Guid?> ResolveApplicationIdAsync(Contract c, CancellationToken ct) =>
        _db.JobApplications.AsNoTracking()
            .Where(a => a.JobId == c.JobId && a.StudentId == c.StudentId)
            .Select(a => (Guid?)a.Id)
            .FirstOrDefaultAsync(ct);

    /// <summary>
    /// Ghi 1 bản ghi vào dbo.Payments để hiển thị trong "lịch sử dòng tiền" của ví.
    /// status: escrow | released | refunded. Bỏ qua nếu không tìm thấy JobApplication
    /// (cột JobApplicationId là NOT NULL).
    /// </summary>
    private async Task AddPaymentRecordAsync(Contract c, decimal amount, string status, string method, string? description, CancellationToken ct)
    {
        if (amount <= 0) return;
        var appId = await ResolveApplicationIdAsync(c, ct);
        if (appId is null) return;

        _db.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            JobId = c.JobId,
            JobApplicationId = appId.Value,
            BusinessId = c.BusinessId,
            StudentId = c.StudentId,
            Amount = amount,
            Currency = "VND",
            Status = status,
            PaymentMethod = method,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ReleasedAt = status == "released" ? DateTime.UtcNow : null
        });
    }

    /// <summary>GUARD: currentUserId phải là chủ của BusinessProfile = businessId.</summary>
    private async Task EnsureBusinessOwnerAsync(Guid businessId, Guid currentUserId, CancellationToken ct)
    {
        var isOwner = await _db.BusinessProfiles.AsNoTracking()
            .AnyAsync(b => b.Id == businessId && b.UserId == currentUserId, ct);
        if (!isOwner)
            throw new UnauthorizedAccessException("Bạn không phải doanh nghiệp sở hữu hợp đồng này.");
    }

    /// <summary>GUARD: currentUserId phải là chủ của StudentProfile = studentId.</summary>
    private async Task EnsureStudentOwnerAsync(Guid studentId, Guid currentUserId, CancellationToken ct)
    {
        var isOwner = await _db.StudentProfiles.AsNoTracking()
            .AnyAsync(s => s.Id == studentId && s.UserId == currentUserId, ct);
        if (!isOwner)
            throw new UnauthorizedAccessException("Bạn không phải sinh viên của hợp đồng này.");
    }

    /// <summary>GUARD đọc: currentUserId là Business HOẶC Student của hợp đồng.</summary>
    private async Task EnsureContractParticipantAsync(Guid studentId, Guid businessId, Guid currentUserId, CancellationToken ct)
    {
        var isBusiness = await _db.BusinessProfiles.AsNoTracking()
            .AnyAsync(b => b.Id == businessId && b.UserId == currentUserId, ct);
        var isStudent = await _db.StudentProfiles.AsNoTracking()
            .AnyAsync(s => s.Id == studentId && s.UserId == currentUserId, ct);
        if (!isBusiness && !isStudent)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập hợp đồng này.");
    }

    /// <summary>
    /// Map entity Contract -> DTO. Chính sách 1.1: khi <paramref name="hidePending"/> = true
    /// (người xem là Sinh viên), ẩn các task CHƯA ký quỹ (PENDING) — task chỉ hiển thị cho
    /// người nhận sau khi doanh nghiệp nạp đủ 100% vào Escrow.
    /// </summary>
    private static ContractResponse MapContract(Contract contract, bool hidePending = false) => new()
    {
        Id = contract.Id,
        JobId = contract.JobId,
        JobTitle = contract.Job?.Title,
        StudentId = contract.StudentId,
        StudentName = contract.Student?.User?.FullName,
        BusinessId = contract.BusinessId,
        FinalPrice = contract.FinalPrice,
        Status = contract.Status,
        CreatedAt = contract.CreatedAt,
        Milestones = contract.Milestones
            .Where(m => !hidePending || m.Status != "PENDING")
            .OrderBy(m => m.CreatedAt)
            .Select(MapMilestone)
            .ToList()
    };

    /// <summary>Map entity Milestone -> DTO (kèm bản nộp mới nhất).</summary>
    private static MilestoneResponse MapMilestone(Milestone m)
    {
        var latest = m.Submissions
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefault();

        return new MilestoneResponse
        {
            Id = m.Id,
            ContractId = m.ContractId,
            Title = m.Title,
            Amount = m.Amount,
            Status = m.Status,
            DueDate = m.DueDate,
            CreatedAt = m.CreatedAt,
            LatestSubmission = latest is null ? null : new SubmissionResponse
            {
                Id = latest.Id,
                MilestoneId = latest.MilestoneId,
                FileUrl = latest.FileUrl,
                CoverLetter = latest.CoverLetter,
                ClientFeedback = latest.ClientFeedback,
                ClientEvidenceUrl = latest.ClientEvidenceUrl,
                CreatedAt = latest.CreatedAt
            }
        };
    }
}
