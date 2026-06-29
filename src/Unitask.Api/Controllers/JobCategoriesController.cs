using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.DTOs.Jobs;
using Unitask.Api.Services;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class JobCategoriesController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public JobCategoriesController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<IReadOnlyList<JobCategoryInfoDto>>> GetCategories()
    {
        try
        {
            // Đếm SỐNG số job đang mở theo từng ngành (cột JobCount tĩnh dễ bị lệch thực tế).
            var categories = await _dbContext.JobCategories.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new JobCategoryInfoDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    JobCount = _dbContext.Jobs.Count(j => j.CategoryId == c.Id && j.Status == "open")
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch
        {
            return Ok(FallbackData.GetCategories());
        }
    }
}

