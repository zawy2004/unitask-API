using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.DTOs.Faqs;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/faqs")]
public class FaqsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public FaqsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FaqResponse>>> GetFaqs([FromQuery] string? category)
    {
        var query = _dbContext.FAQs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(f => f.Category == category);
        }

        var items = await query
            .OrderBy(f => f.OrderIndex)
            .Select(f => new FaqResponse
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer,
                Category = f.Category,
                ViewCount = f.ViewCount
            })
            .ToListAsync();

        return Ok(items);
    }
}

