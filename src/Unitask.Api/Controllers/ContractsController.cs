using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unitask.Api.Extensions;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs.Contracts;

namespace Unitask.Api.Controllers;

/// <summary>
/// Đọc dữ liệu hợp đồng để màn hình "Quản lý tiến độ dự án" (phía Business) render
/// danh sách Milestone. Cả Business chủ HĐ lẫn Student của HĐ đều xem được (guard trong service).
/// </summary>
[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IMilestoneService _milestoneService;

    public ContractsController(IMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    /// <summary>
    /// Business tạo hợp đồng + milestone từ một JobApplication đã 'accepted'.
    /// Trả 201 kèm ContractResponse; map lỗi nghiệp vụ sang 404/403/409.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContractResponse>> CreateContract([FromBody] CreateContractRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var created = await _milestoneService.CreateContractAsync(request, userId.Value, ct);
            return CreatedAtAction(nameof(GetContract), new { id = created.Id }, created);
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

    /// <summary>Danh sách hợp đồng của người dùng hiện tại (Business hoặc Student).</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<ContractResponse>>> GetMine(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await _milestoneService.GetMyContractsAsync(userId.Value, ct));
    }

    /// <summary>Lấy hợp đồng + toàn bộ milestone (kèm bản nộp mới nhất).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContractResponse>> GetContract(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            return Ok(await _milestoneService.GetContractAsync(id, userId.Value, ct));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>Tìm hợp đồng theo JobApplication (frontend kiểm tra ứng viên đã có HĐ chưa). 404 nếu chưa có.</summary>
    [HttpGet("by-application/{jobApplicationId:guid}")]
    public async Task<ActionResult<ContractResponse>> GetByApplication(Guid jobApplicationId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            return Ok(await _milestoneService.GetContractByApplicationAsync(jobApplicationId, userId.Value, ct));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>Business "giao task": thêm milestone vào hợp đồng. Trả về milestone vừa tạo.</summary>
    [HttpPost("{id:guid}/milestones")]
    public async Task<ActionResult<MilestoneResponse>> AddMilestone(Guid id, [FromBody] CreateMilestoneRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var created = await _milestoneService.AddMilestoneAsync(id, userId.Value, request, ct);
            return Ok(created);
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

    /// <summary>Chỉ lấy danh sách milestone của một hợp đồng.</summary>
    [HttpGet("{id:guid}/milestones")]
    public async Task<ActionResult<IReadOnlyList<MilestoneResponse>>> GetMilestones(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            return Ok(await _milestoneService.GetMilestonesAsync(id, userId.Value, ct));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }
}
