using System.Text.Json;
using Unitask.Api.Models;
using Unitask.Infrastructure.Persistence;
using Unitask.Domain.Entities;

namespace Unitask.Api.Services;

public interface IDataNormalizationService
{
    RagDocument NormalizeJob(Job job);
    RagDocument NormalizeUser(User user);
    RagDocument NormalizeApplication(JobApplication application, Job? job);
    List<RagDocument> GetTaxonomyDocuments();
    string NormalizeText(string text);
}

public class DataNormalizationService : IDataNormalizationService
{
    private readonly UnitaskDbContext _dbContext;
    private readonly ILogger<DataNormalizationService> _logger;

    public DataNormalizationService(UnitaskDbContext dbContext, ILogger<DataNormalizationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public RagDocument NormalizeJob(Job job)
    {
        var content = string.Join(" ", new[] { job.Title, job.Description, job.Location, job.TagsJson, job.RequiredSkillsJson }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return new RagDocument
        {
            Id = job.Id.ToString(),
            Type = "job",
            Title = job.Title ?? "",
            Content = NormalizeText(content),
            Metadata = new Dictionary<string, string>
            {
                { "business", job.Business?.CompanyName ?? string.Empty },
                { "category", job.Category?.Name ?? string.Empty }
            },
            CreatedAt = job.CreatedAt ?? DateTime.UtcNow
        };
    }

    public RagDocument NormalizeUser(User user)
    {
        var content = string.Join(" ", new[] { user.FullName, user.Bio, user.StudentProfile?.Major, user.BusinessProfile?.CompanyName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return new RagDocument
        {
            Id = user.Id.ToString(),
            Type = "user",
            Title = user.FullName ?? string.Empty,
            Content = NormalizeText(content),
            Metadata = new Dictionary<string, string>
            {
                { "email", user.Email ?? string.Empty }
            },
            CreatedAt = user.CreatedAt ?? DateTime.UtcNow
        };
    }

    public RagDocument NormalizeApplication(JobApplication application, Job? job)
    {
        var content = string.Join(" ", new[] { application.CoverLetter, job?.Title, job?.Description }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return new RagDocument
        {
            Id = application.Id.ToString(),
            Type = "application",
            Title = job?.Title ?? "Application",
            Content = NormalizeText(content),
            Metadata = new Dictionary<string, string>
            {
                { "jobId", application.JobId.ToString() },
                { "studentId", application.StudentId.ToString() }
            },
            CreatedAt = application.AppliedAt ?? DateTime.UtcNow
        };
    }

    public List<RagDocument> GetTaxonomyDocuments()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", "skill_taxonomy.json");
            if (!File.Exists(path))
            {
                _logger.LogWarning("Taxonomy file not found at {Path}", path);
                return new List<RagDocument>();
            }

            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

            return items.Select((item, idx) => new RagDocument
            {
                Id = $"taxonomy-{idx}",
                Type = "taxonomy",
                Title = item,
                Content = NormalizeText(item),
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading taxonomy documents");
            return new List<RagDocument>();
        }
    }

    public string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.Trim().Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
