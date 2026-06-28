using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unitask.Application.DTOs.Contracts;

namespace Unitask.Application.Common.Interfaces;

/// <summary>
/// Quy trình giải quyết tranh chấp B1–B4.
///   B1 OpenAsync           — participant mở tranh chấp (NEGOTIATION)
///   B2 RequestMediationAsync— yêu cầu hòa giải (MEDIATION)
///   B3 ResolveAsync         — hòa giải viên (admin) ra quyết định + tác động Escrow (RESOLVED)
///   B4 AppealAsync          — kháng cáo trong 7 ngày (APPEAL)
/// Lỗi map: 404 / 403 / 409 (giống IMilestoneService).
/// </summary>
public interface IDisputeService
{
    Task<DisputeResponse> OpenAsync(OpenDisputeRequest request, Guid currentUserId, CancellationToken ct = default);
    Task<DisputeResponse> RequestMediationAsync(Guid disputeId, Guid currentUserId, CancellationToken ct = default);
    Task<DisputeResponse> ResolveAsync(Guid disputeId, Guid mediatorUserId, ResolveDisputeRequest request, CancellationToken ct = default);
    Task<DisputeResponse> AppealAsync(Guid disputeId, Guid currentUserId, AppealDisputeRequest request, CancellationToken ct = default);

    /// <summary>Tranh chấp của một hợp đồng (cho participant xem).</summary>
    Task<IReadOnlyList<DisputeResponse>> GetByContractAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default);

    /// <summary>
    /// Admin: liệt kê tất cả tranh chấp (kèm bối cảnh) để hòa giải. Lọc theo
    /// <paramref name="status"/> nếu có (NEGOTIATION/MEDIATION/RESOLVED/APPEAL/CLOSED).
    /// </summary>
    Task<IReadOnlyList<AdminDisputeResponse>> GetAllForAdminAsync(Guid adminUserId, string? status, CancellationToken ct = default);
}
