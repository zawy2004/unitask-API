using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.DTOs.Portfolio;
using Unitask.Domain.Entities;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/portfolio")]
public class PortfolioController : ControllerBase
{
    private readonly UnitaskDbContext _db;

    public PortfolioController(UnitaskDbContext db) => _db = db;

    // ==================== PUBLIC VIEW ====================

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<PortfolioPublicResponse>> GetPublicPortfolio(Guid userId)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        var student = await _db.StudentProfiles.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (student is null) return NotFound(new { message = "Portfolio not found" });

        var skills = await _db.StudentSkills.AsNoTracking()
            .Where(ss => ss.StudentId == student.Id)
            .Include(ss => ss.Skill)
            .OrderByDescending(ss => ss.EndorsementCount)
            .Select(ss => new PortfolioSkillDto
            {
                Id = ss.Skill.Id,
                Name = ss.Skill.Name,
                Category = ss.Skill.Category,
                Proficiency = ss.Proficiency,
                EndorsementCount = ss.EndorsementCount ?? 0
            })
            .ToListAsync();

        var projects = await _db.PortfolioProjects.AsNoTracking()
            .Where(p => p.StudentId == student.Id)
            .OrderBy(p => p.SortOrder)
            .Select(p => new PortfolioProjectResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                ProjectUrl = p.ProjectUrl,
                GithubUrl = p.GithubUrl,
                Tags = p.Tags,
                Role = p.Role,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsHighlighted = p.IsHighlighted,
                SortOrder = p.SortOrder
            })
            .ToListAsync();

        var educations = await _db.Educations.AsNoTracking()
            .Where(e => e.StudentId == student.Id)
            .OrderBy(e => e.SortOrder)
            .Select(e => new EducationResponse
            {
                Id = e.Id,
                Institution = e.Institution,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                StartYear = e.StartYear,
                EndYear = e.EndYear,
                Gpa = e.Gpa,
                Description = e.Description,
                IsCurrent = e.IsCurrent,
                SortOrder = e.SortOrder
            })
            .ToListAsync();

        var certifications = await _db.Certifications.AsNoTracking()
            .Where(c => c.StudentId == student.Id)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CertificationResponse
            {
                Id = c.Id,
                Name = c.Name,
                IssuingOrganization = c.IssuingOrganization,
                IssueDate = c.IssueDate,
                ExpirationDate = c.ExpirationDate,
                CredentialUrl = c.CredentialUrl,
                CredentialId = c.CredentialId,
                ImageUrl = c.ImageUrl,
                SortOrder = c.SortOrder
            })
            .ToListAsync();

        var reviews = await _db.Reviews.AsNoTracking()
            .Where(r => r.ToUserId == userId && !r.IsAnonymous.GetValueOrDefault())
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Include(r => r.FromUser)
            .Include(r => r.Job)
            .Select(r => new PortfolioReviewDto
            {
                Id = r.Id,
                ReviewerName = r.FromUser.FullName,
                ReviewerAvatar = r.FromUser.AvatarUrl,
                Rating = r.Rating,
                Comment = r.Comment,
                JobTitle = r.Job.Title,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var avgRating = await _db.Reviews.AsNoTracking()
            .Where(r => r.ToUserId == userId)
            .Select(r => (decimal?)r.Rating)
            .AverageAsync() ?? 0m;

        var reviewCount = await _db.Reviews.AsNoTracking()
            .CountAsync(r => r.ToUserId == userId);

        return Ok(new PortfolioPublicResponse
        {
            UserId = userId,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Bio = student.Bio ?? user.Bio,
            Title = student.Major,
            University = student.University,
            Major = student.Major,
            GraduationYear = student.GraduationYear,
            PortfolioUrl = student.PortfolioUrl,
            CvUrl = student.CvUrl,
            Email = user.Email,
            Phone = user.Phone,
            IsVerified = student.IsVerified,
            CompletedJobs = student.CompletedJobs ?? 0,
            AverageRating = Math.Round(avgRating, 1),
            ReviewCount = reviewCount,
            Skills = skills,
            Projects = projects,
            Educations = educations,
            Certifications = certifications,
            Reviews = reviews
        });
    }

    // ==================== PROJECTS CRUD ====================

    [HttpGet("{userId:guid}/projects")]
    public async Task<IActionResult> GetProjects(Guid userId)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var items = await _db.PortfolioProjects.AsNoTracking()
            .Where(p => p.StudentId == studentId.Value)
            .OrderBy(p => p.SortOrder)
            .Select(p => new PortfolioProjectResponse
            {
                Id = p.Id, Title = p.Title, Description = p.Description,
                ImageUrl = p.ImageUrl, ProjectUrl = p.ProjectUrl, GithubUrl = p.GithubUrl,
                Tags = p.Tags, Role = p.Role, StartDate = p.StartDate, EndDate = p.EndDate,
                IsHighlighted = p.IsHighlighted, SortOrder = p.SortOrder
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("{userId:guid}/projects")]
    public async Task<IActionResult> CreateProject(Guid userId, [FromBody] PortfolioProjectRequest req)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var entity = new PortfolioProject
        {
            Id = Guid.NewGuid(),
            StudentId = studentId.Value,
            Title = req.Title,
            Description = req.Description,
            ImageUrl = req.ImageUrl,
            ProjectUrl = req.ProjectUrl,
            GithubUrl = req.GithubUrl,
            Tags = req.Tags,
            Role = req.Role,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            IsHighlighted = req.IsHighlighted,
            SortOrder = req.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.PortfolioProjects.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new PortfolioProjectResponse
        {
            Id = entity.Id, Title = entity.Title, Description = entity.Description,
            ImageUrl = entity.ImageUrl, ProjectUrl = entity.ProjectUrl, GithubUrl = entity.GithubUrl,
            Tags = entity.Tags, Role = entity.Role, StartDate = entity.StartDate, EndDate = entity.EndDate,
            IsHighlighted = entity.IsHighlighted, SortOrder = entity.SortOrder
        });
    }

    [HttpPut("{userId:guid}/projects/{id:guid}")]
    public async Task<IActionResult> UpdateProject(Guid userId, Guid id, [FromBody] PortfolioProjectRequest req)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var entity = await _db.PortfolioProjects.FirstOrDefaultAsync(p => p.Id == id && p.StudentId == studentId.Value);
        if (entity is null) return NotFound();

        entity.Title = req.Title;
        entity.Description = req.Description;
        entity.ImageUrl = req.ImageUrl;
        entity.ProjectUrl = req.ProjectUrl;
        entity.GithubUrl = req.GithubUrl;
        entity.Tags = req.Tags;
        entity.Role = req.Role;
        entity.StartDate = req.StartDate;
        entity.EndDate = req.EndDate;
        entity.IsHighlighted = req.IsHighlighted;
        entity.SortOrder = req.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{userId:guid}/projects/{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid userId, Guid id)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var entity = await _db.PortfolioProjects.FirstOrDefaultAsync(p => p.Id == id && p.StudentId == studentId.Value);
        if (entity is null) return NotFound();

        _db.PortfolioProjects.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ==================== EDUCATION CRUD ====================

    [HttpGet("{userId:guid}/educations")]
    public async Task<IActionResult> GetEducations(Guid userId)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var items = await _db.Educations.AsNoTracking()
            .Where(e => e.StudentId == studentId.Value)
            .OrderBy(e => e.SortOrder)
            .Select(e => new EducationResponse
            {
                Id = e.Id, Institution = e.Institution, Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy, StartYear = e.StartYear, EndYear = e.EndYear,
                Gpa = e.Gpa, Description = e.Description, IsCurrent = e.IsCurrent, SortOrder = e.SortOrder
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("{userId:guid}/educations")]
    public async Task<IActionResult> CreateEducation(Guid userId, [FromBody] EducationRequest req)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var entity = new Education
        {
            Id = Guid.NewGuid(),
            StudentId = studentId.Value,
            Institution = req.Institution,
            Degree = req.Degree,
            FieldOfStudy = req.FieldOfStudy,
            StartYear = req.StartYear,
            EndYear = req.EndYear,
            Gpa = req.Gpa,
            Description = req.Description,
            IsCurrent = req.IsCurrent,
            SortOrder = req.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Educations.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new EducationResponse
        {
            Id = entity.Id, Institution = entity.Institution, Degree = entity.Degree,
            FieldOfStudy = entity.FieldOfStudy, StartYear = entity.StartYear, EndYear = entity.EndYear,
            Gpa = entity.Gpa, Description = entity.Description, IsCurrent = entity.IsCurrent, SortOrder = entity.SortOrder
        });
    }

    [HttpPut("{userId:guid}/educations/{id:guid}")]
    public async Task<IActionResult> UpdateEducation(Guid userId, Guid id, [FromBody] EducationRequest req)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var entity = await _db.Educations.FirstOrDefaultAsync(e => e.Id == id && e.StudentId == studentId.Value);
        if (entity is null) return NotFound();

        entity.Institution = req.Institution;
        entity.Degree = req.Degree;
        entity.FieldOfStudy = req.FieldOfStudy;
        entity.StartYear = req.StartYear;
        entity.EndYear = req.EndYear;
        entity.Gpa = req.Gpa;
        entity.Description = req.Description;
        entity.IsCurrent = req.IsCurrent;
        entity.SortOrder = req.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{userId:guid}/educations/{id:guid}")]
    public async Task<IActionResult> DeleteEducation(Guid userId, Guid id)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var entity = await _db.Educations.FirstOrDefaultAsync(e => e.Id == id && e.StudentId == studentId.Value);
        if (entity is null) return NotFound();

        _db.Educations.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ==================== CERTIFICATION CRUD ====================

    [HttpGet("{userId:guid}/certifications")]
    public async Task<IActionResult> GetCertifications(Guid userId)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var items = await _db.Certifications.AsNoTracking()
            .Where(c => c.StudentId == studentId.Value)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CertificationResponse
            {
                Id = c.Id, Name = c.Name, IssuingOrganization = c.IssuingOrganization,
                IssueDate = c.IssueDate, ExpirationDate = c.ExpirationDate,
                CredentialUrl = c.CredentialUrl, CredentialId = c.CredentialId,
                ImageUrl = c.ImageUrl, SortOrder = c.SortOrder
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("{userId:guid}/certifications")]
    public async Task<IActionResult> CreateCertification(Guid userId, [FromBody] CertificationRequest req)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var entity = new Certification
        {
            Id = Guid.NewGuid(),
            StudentId = studentId.Value,
            Name = req.Name,
            IssuingOrganization = req.IssuingOrganization,
            IssueDate = req.IssueDate,
            ExpirationDate = req.ExpirationDate,
            CredentialUrl = req.CredentialUrl,
            CredentialId = req.CredentialId,
            ImageUrl = req.ImageUrl,
            SortOrder = req.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Certifications.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new CertificationResponse
        {
            Id = entity.Id, Name = entity.Name, IssuingOrganization = entity.IssuingOrganization,
            IssueDate = entity.IssueDate, ExpirationDate = entity.ExpirationDate,
            CredentialUrl = entity.CredentialUrl, CredentialId = entity.CredentialId,
            ImageUrl = entity.ImageUrl, SortOrder = entity.SortOrder
        });
    }

    [HttpPut("{userId:guid}/certifications/{id:guid}")]
    public async Task<IActionResult> UpdateCertification(Guid userId, Guid id, [FromBody] CertificationRequest req)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var entity = await _db.Certifications.FirstOrDefaultAsync(c => c.Id == id && c.StudentId == studentId.Value);
        if (entity is null) return NotFound();

        entity.Name = req.Name;
        entity.IssuingOrganization = req.IssuingOrganization;
        entity.IssueDate = req.IssueDate;
        entity.ExpirationDate = req.ExpirationDate;
        entity.CredentialUrl = req.CredentialUrl;
        entity.CredentialId = req.CredentialId;
        entity.ImageUrl = req.ImageUrl;
        entity.SortOrder = req.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{userId:guid}/certifications/{id:guid}")]
    public async Task<IActionResult> DeleteCertification(Guid userId, Guid id)
    {
        var studentId = await GetStudentId(userId);
        if (studentId is null) return NotFound();

        var entity = await _db.Certifications.FirstOrDefaultAsync(c => c.Id == id && c.StudentId == studentId.Value);
        if (entity is null) return NotFound();

        _db.Certifications.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ==================== HELPERS ====================

    private async Task<Guid?> GetStudentId(Guid userId)
    {
        var student = await _db.StudentProfiles.AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync();
        return student;
    }
}
