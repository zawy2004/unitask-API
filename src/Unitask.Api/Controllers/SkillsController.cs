using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Application.DTOs.Skills;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api")]
public class SkillsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public SkillsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("skills")]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<IReadOnlyList<SkillResponse>>> GetSkills()
    {
        try
        {
            var skills = await _dbContext.Skills.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SkillResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    IconUrl = s.IconUrl
                })
                .ToListAsync();

            return Ok(skills);
        }
        catch
        {
            return Ok(Array.Empty<SkillResponse>());
        }
    }

    [HttpGet("students/{userId:guid}/skills")]
    public async Task<ActionResult<IReadOnlyList<StudentSkillResponse>>> GetStudentSkills(Guid userId)
    {
        var student = await _dbContext.StudentProfiles.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (student is null)
        {
            return NotFound();
        }

        var skills = await _dbContext.StudentSkills.AsNoTracking()
            .Where(ss => ss.StudentId == student.Id)
            .Include(ss => ss.Skill)
            .Select(ss => new StudentSkillResponse
            {
                Id = ss.Id,
                SkillId = ss.SkillId,
                SkillName = ss.Skill.Name,
                Proficiency = ss.Proficiency,
                EndorsementCount = ss.EndorsementCount,
                AddedAt = ss.AddedAt
            })
            .ToListAsync();

        return Ok(skills);
    }

    [Authorize]
    [HttpPost("students/me/skills")]
    public async Task<IActionResult> AddStudentSkill([FromBody] AddStudentSkillRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var student = await _dbContext.StudentProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        if (student is null)
        {
            var user = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.Value && u.UserType == "student");
            if (user is null)
                return BadRequest(new { message = "Student profile not found." });

            student = new Unitask.Domain.Entities.StudentProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                StudentEmail = user.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsVerified = false,
                CompletedJobs = 0,
                TotalEarnings = 0m
            };
            _dbContext.StudentProfiles.Add(student);
            await _dbContext.SaveChangesAsync();
        }

        var exists = await _dbContext.StudentSkills
            .AnyAsync(ss => ss.StudentId == student.Id && ss.SkillId == request.SkillId);
        if (exists)
        {
            return Conflict(new { message = "Skill already added." });
        }

        var studentSkill = new Unitask.Domain.Entities.StudentSkill
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            SkillId = request.SkillId,
            Proficiency = request.Proficiency,
            EndorsementCount = 0,
            AddedAt = DateTime.UtcNow
        };

        _dbContext.StudentSkills.Add(studentSkill);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpDelete("students/me/skills/{skillId:guid}")]
    public async Task<IActionResult> RemoveStudentSkill(Guid skillId)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var student = await _dbContext.StudentProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        if (student is null)
        {
            return BadRequest(new { message = "Student profile not found." });
        }

        var studentSkill = await _dbContext.StudentSkills
            .FirstOrDefaultAsync(ss => ss.StudentId == student.Id && ss.SkillId == skillId);
        if (studentSkill is null)
        {
            return NotFound();
        }

        _dbContext.StudentSkills.Remove(studentSkill);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpPost("skills/{skillId:guid}/endorse")]
    public async Task<IActionResult> EndorseSkill(Guid skillId, [FromBody] EndorseSkillRequest request)
    {
        var studentSkill = await _dbContext.StudentSkills
            .FirstOrDefaultAsync(ss => ss.SkillId == skillId && ss.StudentId == request.StudentId);
        if (studentSkill is null)
        {
            return NotFound();
        }

        studentSkill.EndorsementCount = (studentSkill.EndorsementCount ?? 0) + 1;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}

