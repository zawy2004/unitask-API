using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.DTOs.Jobs;
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
    public async Task<ActionResult<IReadOnlyList<JobCategoryInfoDto>>> GetCategories()
    {
        var categories = await _dbContext.JobCategories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new JobCategoryInfoDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                JobCount = c.JobCount
            })
            .ToListAsync();

        return Ok(categories);
    }
}

