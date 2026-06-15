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

/// <summary>Quy trình tranh chấp B1–B4.</summary>
[ApiController]
[Route("api/disputes")]
[Authorize]
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _disputes;

    public DisputesController(IDisputeService disputes)
    {
        _disputes = disputes;
    }

    /// <summary>B1 — mở tranh chấp.</summary>
    [HttpPost]
    public Task<ActionResult<DisputeResponse>> Open([FromBody] OpenDisputeRequest request, CancellationToken ct)
        => Run(uid => _disputes.OpenAsync(request, uid, ct));

    /// <summary>B2 — yêu cầu hòa giải.</summary>
    [HttpPost("{id:guid}/request-mediation")]
    public Task<ActionResult<DisputeResponse>> RequestMediation(Guid id, CancellationToken ct)
        => Run(uid => _disputes.RequestMediationAsync(id, uid, ct));

    /// <summary>B3 — hòa giải viên (admin) ra quyết định.</summary>
    [HttpPost("{id:guid}/resolve")]
    public Task<ActionResult<DisputeResponse>> Resolve(Guid id, [FromBody] ResolveDisputeRequest request, CancellationToken ct)
        => Run(uid => _disputes.ResolveAsync(id, uid, request, ct));

    /// <summary>B4 — kháng cáo trong 7 ngày.</summary>
    [HttpPost("{id:guid}/appeal")]
    public Task<ActionResult<DisputeResponse>> Appeal(Guid id, [FromBody] AppealDisputeRequest request, CancellationToken ct)
        => Run(uid => _disputes.AppealAsync(id, uid, request, ct));

    /// <summary>Danh sách tranh chấp của một hợp đồng.</summary>
    [HttpGet("contract/{contractId:guid}")]
    public async Task<ActionResult<IReadOnlyList<DisputeResponse>>> ByContract(Guid contractId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        try { return Ok(await _disputes.GetByContractAsync(contractId, userId.Value, ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message }); }
    }

    private async Task<ActionResult<DisputeResponse>> Run(Func<Guid, Task<DisputeResponse>> action)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        try { return Ok(await action(userId.Value)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
