using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unitask.Api.Extensions;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs.Contracts;

namespace Unitask.Api.Controllers;

/// <summary>
/// Các hành động trên Milestone: escrow / submit / approve / request-changes.
///
/// LUỒNG DỮ LIỆU:
///   HTTP request → Controller (lấy userId từ JWT) → IMilestoneService (logic + guard quyền)
///   → DbContext (EF Core) → SQL Server. Controller chỉ điều phối + map lỗi sang HTTP status.
///
/// PHÂN QUYỀN: [Authorize] đảm bảo đã đăng nhập; việc kiểm tra "đúng Business/đúng Student
/// của hợp đồng" nằm trong service (resource-based authorization) vì cần tra cứu DB theo milestone.
/// </summary>
[ApiController]
[Route("api/milestones")]
[Authorize]
public class MilestonesController : ControllerBase
{
    private readonly IMilestoneService _milestoneService;

    public MilestonesController(IMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    /// <summary>Business nạp tiền ký quỹ: PENDING → ESCROWED.</summary>
    [HttpPost("{id:guid}/escrow")]
    public Task<ActionResult<MilestoneResponse>> Escrow(Guid id, [FromBody] EscrowMilestoneRequest? request, CancellationToken ct)
        => Run(userId => _milestoneService.EscrowAsync(id, userId, request, ct));

    /// <summary>Student nộp bài: ESCROWED|REVISION → UNDER_REVIEW (tạo Submission).</summary>
    [HttpPost("{id:guid}/submit")]
    public Task<ActionResult<MilestoneResponse>> Submit(Guid id, [FromBody] SubmitMilestoneRequest request, CancellationToken ct)
        => Run(userId => _milestoneService.SubmitAsync(id, userId, request, ct));

    /// <summary>Business nghiệm thu: UNDER_REVIEW → COMPLETED (+ giải ngân ví Student).</summary>
    [HttpPost("{id:guid}/approve")]
    public Task<ActionResult<MilestoneResponse>> Approve(Guid id, CancellationToken ct)
        => Run(userId => _milestoneService.ApproveAsync(id, userId, ct));

    /// <summary>Business yêu cầu sửa: UNDER_REVIEW → REVISION (+ lưu feedback).</summary>
    [HttpPost("{id:guid}/request-changes")]
    public Task<ActionResult<MilestoneResponse>> RequestChanges(Guid id, [FromBody] RequestChangesRequest request, CancellationToken ct)
        => Run(userId => _milestoneService.RequestChangesAsync(id, userId, request, ct));

    /// <summary>
    /// Bộ điều phối chung: lấy userId từ JWT, gọi service, và map exception nghiệp vụ → HTTP status.
    /// Gom về một chỗ để 4 endpoint trên ngắn gọn, nhất quán.
    /// </summary>
    private async Task<ActionResult<MilestoneResponse>> Run(Func<Guid, Task<MilestoneResponse>> action)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        try
        {
            var result = await action(userId.Value);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
