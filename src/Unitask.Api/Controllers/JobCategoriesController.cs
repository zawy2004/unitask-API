using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Unitask.Application.DTOs.Jobs;
using Unitask.Api.Services;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class JobCategoriesController : ControllerBase
{
    private const string CategoriesCacheKey = "categories:list";

    private readonly UnitaskDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public JobCategoriesController(UnitaskDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    [HttpGet]
    [ResponseCache(Duration = 120)]
    public async Task<ActionResult<IReadOnlyList<JobCategoryInfoDto>>> GetCategories()
    {
        try
        {
            if (_cache.TryGetValue(CategoriesCacheKey, out IReadOnlyList<JobCategoryInfoDto>? cached) && cached is not null)
            {
                return Ok(cached);
            }

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

            // JobCount thay đổi thường xuyên hơn Skills/FAQs nên TTL ngắn hơn (2 phút thay vì 5 phút).
            _cache.Set(CategoriesCacheKey, categories, TimeSpan.FromMinutes(2));

            return Ok(categories);
        }
        catch
        {
            return Ok(FallbackData.GetCategories());
        }
    }
}

