using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unitask.Application.DTOs.Contracts;

namespace Unitask.Application.Common.Interfaces;

/// <summary>
/// Nghiệp vụ Hợp đồng & Milestone: ký quỹ (escrow), nộp bài, nghiệm thu, yêu cầu sửa.
///
/// Quy ước bảo mật: mọi method nhận <paramref name="currentUserId"/> (lấy từ JWT) và TỰ
/// kiểm tra quyền sở hữu bên trong service:
///   - Escrow / Approve / RequestChanges: chỉ Business chủ hợp đồng được gọi.
///   - Submit: chỉ Student của hợp đồng được gọi.
///
/// Quy ước lỗi (controller map sang HTTP status):
///   - KeyNotFoundException        -> 404 Not Found
///   - UnauthorizedAccessException -> 403 Forbidden
///   - InvalidOperationException   -> 409 Conflict (sai trạng thái / vi phạm state machine)
/// </summary>
public interface IMilestoneService
{
    /// <summary>
    /// Business tạo hợp đồng + danh sách milestone từ một JobApplication đã được "accepted".
    /// Guard: chỉ Business sở hữu Job của đơn ứng tuyển mới được tạo.
    /// </summary>
    Task<ContractResponse> CreateContractAsync(CreateContractRequest request, Guid currentUserId, CancellationToken cancellationToken = default);

    /// <summary>Danh sách hợp đồng của người dùng hiện tại (theo vai trò Business hoặc Student).</summary>
    Task<IReadOnlyList<ContractResponse>> GetMyContractsAsync(Guid currentUserId, CancellationToken cancellationToken = default);

    /// <summary>Business "giao task": thêm 1 milestone (PENDING) vào hợp đồng đang ACTIVE. Guard: chủ HĐ.</summary>
    Task<MilestoneResponse> AddMilestoneAsync(Guid contractId, Guid currentUserId, CreateMilestoneRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lấy hợp đồng + toàn bộ milestone (kèm bản nộp mới nhất).</summary>
    Task<ContractResponse> GetContractAsync(Guid contractId, Guid currentUserId, CancellationToken cancellationToken = default);

    /// <summary>Tìm hợp đồng theo một JobApplication (để biết ứng viên đã có HĐ chưa). Ném 404 nếu chưa có.</summary>
    Task<ContractResponse> GetContractByApplicationAsync(Guid jobApplicationId, Guid currentUserId, CancellationToken cancellationToken = default);

    /// <summary>Danh sách milestone của một hợp đồng.</summary>
    Task<IReadOnlyList<MilestoneResponse>> GetMilestonesAsync(Guid contractId, Guid currentUserId, CancellationToken cancellationToken = default);

    /// <summary>Business nạp tiền ký quỹ: PENDING → ESCROWED.</summary>
    Task<MilestoneResponse> EscrowAsync(Guid milestoneId, Guid currentUserId, EscrowMilestoneRequest? request, CancellationToken cancellationToken = default);

    /// <summary>Student nộp bài: ESCROWED | REVISION → UNDER_REVIEW (+ tạo Submission).</summary>
    Task<MilestoneResponse> SubmitAsync(Guid milestoneId, Guid currentUserId, SubmitMilestoneRequest request, CancellationToken cancellationToken = default);

    /// <summary>Business nghiệm thu: UNDER_REVIEW → COMPLETED (+ cộng tiền vào ví Student).</summary>
    Task<MilestoneResponse> ApproveAsync(Guid milestoneId, Guid currentUserId, CancellationToken cancellationToken = default);

    /// <summary>Business yêu cầu sửa: UNDER_REVIEW → REVISION (+ lưu feedback).</summary>
    Task<MilestoneResponse> RequestChangesAsync(Guid milestoneId, Guid currentUserId, RequestChangesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Chính sách 1.3 — Business hủy task: trả % tiến độ cho người thực hiện, hoàn phần còn lại
    /// về ví doanh nghiệp; tối thiểu 30% cho người thực hiện nếu đã qua 48h kể từ lúc ký quỹ.
    /// </summary>
    Task<MilestoneResponse> CancelMilestoneAsync(Guid milestoneId, Guid currentUserId, CancelMilestoneRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Chính sách 1.2 ("im lặng = chấp thuận"): tự động nghiệm thu &amp; giải ngân các milestone
    /// UNDER_REVIEW quá <paramref name="timeoutHours"/> giờ. Dùng cho background service. Trả về số đã giải ngân.
    /// </summary>
    Task<int> AutoReleaseExpiredAsync(int timeoutHours, CancellationToken cancellationToken = default);
}
