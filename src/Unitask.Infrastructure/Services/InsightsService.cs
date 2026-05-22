using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs.Insights;
using Unitask.Domain.Entities;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Infrastructure.Services;

public class InsightsService : IInsightsService
{
    private static readonly string[] Gradients =
    {
        "linear-gradient(135deg, #5b4fff, #00d4aa)",
        "linear-gradient(135deg, #ff7a59, #ffb347)",
        "linear-gradient(135deg, #7c3aed, #ec4899)",
        "linear-gradient(135deg, #0f766e, #22c55e)",
        "linear-gradient(135deg, #1d4ed8, #38bdf8)",
        "linear-gradient(135deg, #ea580c, #f97316)"
    };

    private readonly UnitaskDbContext _dbContext;

    public InsightsService(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<JobRecommendationResponse> GetRecommendationsAsync(JobRecommendationRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = Normalize(request.Query);
        var skillHints = NormalizeList(request.Skills);
        var jobs = await LoadOpenJobsAsync(cancellationToken);

        var scoredJobs = jobs
            .Select(job => ScoreJob(job, request, normalizedQuery, skillHints))
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Job.IsFeatured == true)
            .ThenByDescending(item => item.Job.CreatedAt)
            .Take(Math.Max(1, request.TopK))
            .ToList();

        return new JobRecommendationResponse
        {
            Query = request.Query?.Trim() ?? string.Empty,
            UsedSemanticSearch = HasSemanticSignals(request),
            Summary = BuildRecommendationSummary(scoredJobs, request),
            Matches = scoredJobs.Select(item => MapJobCard(item.Job, item.Score, item.Reasons)).ToList()
        };
    }

    public async Task<CareerChatResponse> ChatAsync(CareerChatRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedMessage = Normalize(request.Message);
        var context = request.User ?? new CareerUserContextDto();

        var recommendationRequest = new JobRecommendationRequest
        {
            Query = request.Message,
            Role = context.Role,
            Major = context.Major,
            Skills = context.Skills,
            Bio = context.Bio,
            CompanyName = context.CompanyName,
            University = context.University,
            Location = string.Equals(context.Role, "student", StringComparison.OrdinalIgnoreCase)
                ? context.University
                : context.CompanyName,
            TopK = Math.Max(1, request.TopK)
        };

        var recommendations = await GetRecommendationsAsync(recommendationRequest, cancellationToken);
        var jobs = recommendations.Matches.ToList();

        return new CareerChatResponse
        {
            Reply = BuildChatReply(request.Message, jobs, context, request.History),
            Jobs = jobs,
            FollowUpQuestions = BuildFollowUpQuestions(context, jobs, normalizedMessage),
            CareerPaths = BuildCareerPaths(context, jobs),
            Refused = false,
            Summary = recommendations.Summary
        };
    }

    public async Task<PersonalizationResponse> GetPersonalizationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .Include(u => u.StudentProfile)
            .Include(u => u.BusinessProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var skillNames = await GetUserSkillsAsync(userId, cancellationToken);
        var role = user.UserType?.Trim().ToLowerInvariant() ?? "student";
        var profile = user.StudentProfile;
        var businessProfile = user.BusinessProfile;

        var keywords = new List<string>();
        var categories = new List<string>();
        var locations = new List<string>();

        if (role == "student")
        {
            if (!string.IsNullOrWhiteSpace(profile?.Major))
            {
                keywords.Add(profile.Major);
            }

            keywords.AddRange(skillNames.Take(5));

            if (!string.IsNullOrWhiteSpace(profile?.University))
            {
                locations.Add(profile.University);
            }

            categories.AddRange(InferCategories(profile?.Major, skillNames));
        }
        else if (businessProfile is not null)
        {
            keywords.Add(businessProfile.CompanyName);
            keywords.AddRange(new[] { businessProfile.Industry, businessProfile.CompanySize }.Where(value => !string.IsNullOrWhiteSpace(value))!);
            locations.AddRange(new[] { businessProfile.Address, businessProfile.CompanyEmail }.Where(value => !string.IsNullOrWhiteSpace(value))!);
            categories.AddRange(InferCategories(businessProfile.Industry, skillNames));
        }

        var recommendationRequest = new JobRecommendationRequest
        {
            Role = role,
            Major = profile?.Major,
            Skills = skillNames,
            Bio = profile?.Bio ?? user.Bio,
            CompanyName = businessProfile?.CompanyName,
            University = profile?.University,
            Location = locations.FirstOrDefault(),
            TopK = 6
        };

        var recommendations = await GetRecommendationsAsync(recommendationRequest, cancellationToken);

        var nextActions = new List<string>();
        if (role == "student")
        {
            nextActions.Add(profile is null ? "Hoàn thiện hồ sơ học tập" : "Cập nhật ngành học và kỹ năng nổi bật");
            nextActions.Add(skillNames.Count == 0 ? "Thêm kỹ năng chính" : "Tìm job theo kỹ năng đang có");
            nextActions.Add("Lưu job phù hợp để nhận thông báo");
        }
        else
        {
            nextActions.Add(businessProfile is null ? "Tạo hồ sơ doanh nghiệp" : "Cập nhật thông tin doanh nghiệp");
            nextActions.Add("Tối ưu bài đăng job bằng gợi ý AI");
            nextActions.Add("Bật tự động hóa duyệt hồ sơ và thông báo");
        }

        return new PersonalizationResponse
        {
            UserId = user.Id,
            Role = role,
            Summary = role == "student"
                ? $"Cá nhân hóa theo ngành {profile?.Major ?? "chưa cập nhật"} với {skillNames.Count} kỹ năng và {recommendations.Matches.Count} job phù hợp."
                : $"Cá nhân hóa cho doanh nghiệp {businessProfile?.CompanyName ?? user.FullName} với {recommendations.Matches.Count} cơ hội phù hợp.",
            RecommendedKeywords = keywords.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).Take(8).ToList(),
            RecommendedCategories = categories.Distinct(StringComparer.OrdinalIgnoreCase).Take(6).ToList(),
            RecommendedLocations = locations.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).Take(6).ToList(),
            SuggestedJobs = recommendations.Matches,
            NextActions = nextActions
        };
    }

    public async Task<AutomationSuggestionResponse> GetAutomationSuggestionsAsync(Guid? businessId, Guid? userId, CancellationToken cancellationToken = default)
    {
        var resolvedBusinessId = businessId;

        if (!resolvedBusinessId.HasValue && userId.HasValue)
        {
            resolvedBusinessId = await _dbContext.BusinessProfiles.AsNoTracking()
                .Where(b => b.UserId == userId.Value)
                .Select(b => (Guid?)b.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (!resolvedBusinessId.HasValue)
        {
            throw new KeyNotFoundException("Business profile not found.");
        }

        var jobs = await _dbContext.Jobs.AsNoTracking()
            .Where(job => job.BusinessId == resolvedBusinessId.Value)
            .Include(job => job.JobApplications)
            .ThenInclude(application => application.Student)
            .ThenInclude(student => student.User)
            .ToListAsync(cancellationToken);

        var applications = jobs.SelectMany(job => job.JobApplications).ToList();
        var pending = applications.Count(application => string.Equals(application.Status, "pending", StringComparison.OrdinalIgnoreCase));

        var studentIds = applications.Select(application => application.StudentId).Distinct().ToList();
        var ratings = await _dbContext.Reviews.AsNoTracking()
            .Where(review => studentIds.Contains(review.ToUserId))
            .GroupBy(review => review.ToUserId)
            .Select(group => new { UserId = group.Key, Rating = group.Average(review => (decimal?)review.Rating) ?? 0m })
            .ToDictionaryAsync(item => item.UserId, item => item.Rating, cancellationToken);

        var applicantRatings = applications
            .Select(application => ratings.TryGetValue(application.Student.UserId, out var rating) ? rating : 0m)
            .ToList();

        var highRating = applicantRatings.Count(rating => rating >= 4.5m);
        var lowRating = applicantRatings.Count(rating => rating > 0 && rating < 3.5m);
        var averageRating = applicantRatings.Count == 0 ? 0m : applicantRatings.Average();

        var suggestions = new List<AutomationSuggestionDto>();

        if (highRating > 0 && pending > 0)
        {
            suggestions.Add(new AutomationSuggestionDto
            {
                Id = "auto-accept-high",
                Icon = "✅",
                Title = "Tự động chấp nhận ứng viên rating cao",
                Description = $"Có {highRating} hồ sơ nổi bật đang chờ xử lý.",
                Benefit = "Rút ngắn thời gian duyệt các hồ sơ mạnh.",
                Action = "Thiết lập auto-accept",
                ActionLink = "/automation-rules",
                Priority = "high"
            });
        }

        if (lowRating > 0)
        {
            suggestions.Add(new AutomationSuggestionDto
            {
                Id = "auto-reject-low",
                Icon = "❌",
                Title = "Tự động từ chối hồ sơ yếu",
                Description = $"Có {lowRating} ứng viên rating thấp trong danh sách chờ.",
                Benefit = "Loại bớt các hồ sơ ít phù hợp trước khi review thủ công.",
                Action = "Thiết lập auto-reject",
                ActionLink = "/automation-rules",
                Priority = "normal"
            });
        }

        if (pending > 3)
        {
            suggestions.Add(new AutomationSuggestionDto
            {
                Id = "bulk-pending",
                Icon = "📋",
                Title = "Duyệt hàng loạt hồ sơ",
                Description = $"Hiện có {pending} hồ sơ đang chờ xử lý.",
                Benefit = "Dùng bulk actions để xử lý nhanh hơn.",
                Action = "Mở bulk actions",
                ActionLink = "/manage-jobs",
                Priority = "high"
            });
        }

        if (jobs.Count > 0)
        {
            suggestions.Add(new AutomationSuggestionDto
            {
                Id = "auto-notify",
                Icon = "🔔",
                Title = "Tự động gửi thông báo",
                Description = "Gửi tin nhắn tự động cho ứng viên mới để giữ nhịp phản hồi.",
                Benefit = "Cải thiện trải nghiệm ứng viên và giảm độ trễ phản hồi.",
                Action = "Thiết lập auto-notify",
                ActionLink = "/automation-rules",
                Priority = "normal"
            });
        }

        if (averageRating >= 4.5m && jobs.Count > 0)
        {
            suggestions.Add(new AutomationSuggestionDto
            {
                Id = "quality-high",
                Icon = "⭐",
                Title = "Chất lượng hồ sơ đang rất tốt",
                Description = $"Điểm trung bình ứng viên là {averageRating:F1}.",
                Benefit = "Bạn có thể tăng tốc quy trình tuyển hoặc mở rộng quy mô đăng tuyển.",
                Action = "Xem business automation",
                ActionLink = "/business-automation",
                Priority = "low"
            });
        }

        return new AutomationSuggestionResponse
        {
            BusinessId = resolvedBusinessId,
            PendingApplications = pending,
            Summary = $"Phân tích {jobs.Count} job và {applications.Count} hồ sơ, phát hiện {suggestions.Count} gợi ý tự động hóa.",
            Suggestions = suggestions
        };
    }

    private async Task<List<Job>> LoadOpenJobsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Jobs.AsNoTracking()
            .Include(job => job.Category)
            .Include(job => job.Business)
            .Where(job => job.Status == "open" || job.Status == "published")
            .ToListAsync(cancellationToken);
    }

    private static bool HasSemanticSignals(JobRecommendationRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Query)
            || !string.IsNullOrWhiteSpace(request.Role)
            || !string.IsNullOrWhiteSpace(request.Major)
            || !string.IsNullOrWhiteSpace(request.Bio)
            || !string.IsNullOrWhiteSpace(request.CompanyName)
            || !string.IsNullOrWhiteSpace(request.University)
            || !string.IsNullOrWhiteSpace(request.Location)
            || (request.Skills?.Count ?? 0) > 0;
    }

    private static (Job Job, decimal Score, IReadOnlyList<string> Reasons) ScoreJob(
        Job job,
        JobRecommendationRequest request,
        string normalizedQuery,
        IReadOnlyList<string> skillHints)
    {
        var score = 0m;
        var reasons = new List<string>();

        if (job.IsFeatured == true)
        {
            score += 8;
        }

        if (string.Equals(job.Status, "open", StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
        }

        var jobText = Normalize(string.Join(' ', new[]
        {
            job.Title,
            job.Description,
            job.Location,
            job.Category?.Name,
            job.Business.CompanyName,
            job.TagsJson,
            job.RequiredSkillsJson
        }.Where(value => !string.IsNullOrWhiteSpace(value))));

        if (skillHints.Count > 0)
        {
            var matchedSkills = skillHints
                .Where(skill => Normalize(job.RequiredSkillsJson).Contains(skill) || jobText.Contains(skill))
                .Take(3)
                .ToList();

            if (matchedSkills.Count > 0)
            {
                score += Math.Min(42, matchedSkills.Count * 12);
                reasons.Add($"Khớp kỹ năng: {string.Join(", ", matchedSkills)}");
            }
        }

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            var queryTokens = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(token => token.Length > 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var queryHits = queryTokens.Count(token => jobText.Contains(token));
            if (queryHits > 0)
            {
                score += Math.Min(24, queryHits * 5);
                reasons.Add("Khớp truy vấn tìm kiếm");
            }
        }

        var inferredCategory = InferCategory(request.Major, skillHints);
        if (!string.IsNullOrWhiteSpace(inferredCategory) && MatchesCategory(job, inferredCategory))
        {
            score += 18;
            reasons.Add($"Phù hợp ngành nghề: {inferredCategory}");
        }

        if (!string.IsNullOrWhiteSpace(request.Location)
            && !string.IsNullOrWhiteSpace(job.Location)
            && Normalize(job.Location).Contains(Normalize(request.Location)))
        {
            score += 10;
            reasons.Add($"Khớp địa điểm: {job.Location}");
        }

        if (!string.IsNullOrWhiteSpace(request.CompanyName)
            && Normalize(job.Business.CompanyName).Contains(Normalize(request.CompanyName)))
        {
            score += 14;
            reasons.Add($"Khớp doanh nghiệp: {job.Business.CompanyName}");
        }

        if (!string.IsNullOrWhiteSpace(request.University)
            && !string.IsNullOrWhiteSpace(job.Location)
            && Normalize(job.Location).Contains(Normalize(request.University)))
        {
            score += 8;
            reasons.Add("Khớp khu vực học tập");
        }

        if (job.SpotsTotal.HasValue && job.SpotsFilled.HasValue && job.SpotsTotal > job.SpotsFilled)
        {
            var fillRatio = (decimal)job.SpotsFilled.Value / Math.Max(1, job.SpotsTotal.Value);
            score += fillRatio < 0.5m ? 6 : 2;
        }

        if (job.Deadline.HasValue && job.Deadline.Value.Date <= DateTime.UtcNow.AddDays(7).Date)
        {
            score += 4;
            reasons.Add("Sắp hết hạn");
        }

        if (job.Business.Rating is not null && job.Business.Rating >= 4m)
        {
            score += 4;
        }

        if (reasons.Count == 0)
        {
            reasons.Add("Phù hợp hồ sơ và xu hướng tìm kiếm");
        }

        return (job, Math.Min(100m, score), reasons.Take(3).ToList());
    }

    private static bool MatchesCategory(Job job, string category)
    {
        var normalizedCategory = Normalize(category);
        var jobCategory = Normalize(job.Category?.Slug ?? job.Category?.Name);

        return !string.IsNullOrWhiteSpace(jobCategory) && jobCategory.Contains(normalizedCategory)
            || Normalize(job.Title).Contains(normalizedCategory)
            || Normalize(job.RequiredSkillsJson).Contains(normalizedCategory);
    }

    private static InsightJobCardDto MapJobCard(Job job, decimal score, IReadOnlyList<string> reasons)
    {
        var requiredSkills = ParseJsonList(job.RequiredSkillsJson);
        var tags = BuildTags(ParseJsonList(job.TagsJson), job.Category?.Name, requiredSkills);
        var spotsTotal = Math.Max(1, job.SpotsTotal ?? 1);
        var spotsFilled = Math.Max(0, job.SpotsFilled ?? 0);
        var spotsLeft = Math.Max(0, spotsTotal - spotsFilled);

        return new InsightJobCardDto
        {
            Id = job.Id,
            CompanyId = job.BusinessId,
            Title = job.Title,
            Company = job.Business.CompanyName,
            LogoText = BuildLogoText(job.Business.CompanyName),
            LogoGradient = BuildGradient(job.Business.CompanyName),
            Verified = job.Business.IsVerified == true || job.IsFeatured == true,
            Location = job.Location ?? "Remote",
            Tags = tags,
            SpotsLeft = spotsLeft,
            SpotsTotal = spotsTotal,
            Pay = BuildPay(job),
            PayMin = job.SalaryMin,
            PayMax = job.SalaryMax,
            Deadline = job.Deadline?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "N/A",
            Category = job.Category?.Name ?? job.ExperienceLevel ?? "General",
            Featured = job.IsFeatured,
            Description = job.Description,
            Requirements = requiredSkills,
            Duration = BuildDuration(job),
            PostedAt = job.CreatedAt?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "N/A",
            MatchScore = Math.Round(score, 0),
            MatchReasons = reasons
        };
    }

    private static string BuildRecommendationSummary(IReadOnlyList<(Job Job, decimal Score, IReadOnlyList<string> Reasons)> scoredJobs, JobRecommendationRequest request)
    {
        if (scoredJobs.Count == 0)
        {
            return string.IsNullOrWhiteSpace(request.Query)
                ? "Chưa tìm được job phù hợp từ hồ sơ hiện tại."
                : $"Chưa có job phù hợp mạnh với truy vấn '{request.Query}'.";
        }

        var top = scoredJobs[0];
        return string.IsNullOrWhiteSpace(request.Query)
            ? $"Đã xếp hạng {scoredJobs.Count} job phù hợp, nổi bật nhất là {top.Job.Title} tại {top.Job.Business.CompanyName}."
            : $"Đã tìm {scoredJobs.Count} job cho truy vấn '{request.Query}', nổi bật nhất là {top.Job.Title} tại {top.Job.Business.CompanyName}.";
    }

    private static string BuildChatReply(string message, IReadOnlyList<InsightJobCardDto> jobs, CareerUserContextDto context, IReadOnlyList<CareerChatTurnDto> history)
    {
        if (jobs.Count == 0)
        {
            return string.IsNullOrWhiteSpace(context.Major)
                ? "Mình chưa có đủ dữ liệu để gợi ý thật sát. Hãy thêm ngành học, kỹ năng hoặc mô tả mục tiêu nghề nghiệp rõ hơn."
                : $"Mình chưa tìm được job thật phù hợp với {context.Major}. Hãy thử thêm kỹ năng, mức lương hoặc khu vực làm việc.";
        }

        var topJobs = jobs.Take(3).Select(job => job.Title).ToList();
        var topic = string.IsNullOrWhiteSpace(message) ? "mục tiêu nghề nghiệp của bạn" : message.Trim();
        var historyHint = history.Count > 0 ? " Mình đã cân nhắc cả mạch hội thoại gần nhất để xếp hạng." : string.Empty;

        return $"Mình đã lọc được {jobs.Count} job phù hợp cho '{topic}'. Gợi ý nổi bật: {string.Join(", ", topJobs)}.{historyHint}";
    }

    private static IReadOnlyList<string> BuildFollowUpQuestions(CareerUserContextDto context, IReadOnlyList<InsightJobCardDto> jobs, string normalizedMessage)
    {
        var suggestions = new List<string>();

        if (string.Equals(context.Role, "student", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(context.Major))
            {
                suggestions.Add($"Job liên quan đến {context.Major}");
            }

            suggestions.Add("Job part-time remote");
            suggestions.Add("Thực tập đúng kỹ năng");
        }
        else
        {
            suggestions.Add("Ứng viên rating cao");
            suggestions.Add("Tự động lọc hồ sơ");
            suggestions.Add("Gợi ý bài đăng job mới");
        }

        if (!string.IsNullOrWhiteSpace(normalizedMessage) && normalizedMessage.Contains("remote", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.Add("Job remote full-time");
        }

        if (jobs.Count > 0)
        {
            suggestions.Add($"Xem thêm job tương tự {jobs[0].Title}");
        }

        return suggestions.Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList();
    }

    private static IReadOnlyList<string> BuildCareerPaths(CareerUserContextDto context, IReadOnlyList<InsightJobCardDto> jobs)
    {
        var paths = new List<string>();
        var major = Normalize(context.Major);
        var skills = NormalizeList(context.Skills);

        if (major.Contains("cong nghe thong tin") || major.Contains("cntt") || skills.Any(skill => skill.Contains("react") || skill.Contains("node") || skill.Contains("typescript")))
        {
            paths.Add("Front-end / Back-end / Full-stack");
            paths.Add("Software Engineer Intern");
        }

        if (major.Contains("marketing") || skills.Any(skill => skill.Contains("seo") || skill.Contains("content")))
        {
            paths.Add("Digital Marketing / SEO Content");
            paths.Add("Growth Marketing Intern");
        }

        if (major.Contains("design") || skills.Any(skill => skill.Contains("figma") || skill.Contains("ui")))
        {
            paths.Add("UI/UX Designer");
            paths.Add("Product Designer Intern");
        }

        if (paths.Count == 0 && jobs.Count > 0)
        {
            paths.Add($"Lộ trình theo job tương tự {jobs[0].Title}");
        }

        return paths.Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList();
    }

    private async Task<IReadOnlyList<string>> GetUserSkillsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.StudentSkills.AsNoTracking()
            .Where(skill => skill.Student.UserId == userId)
            .Select(skill => skill.Skill.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyList<string> InferCategories(string? major, IReadOnlyList<string> skills)
    {
        var normalizedMajor = Normalize(major);
        var categories = new List<string>();

        if (normalizedMajor.Contains("cong nghe thong tin") || normalizedMajor.Contains("cntt") || skills.Any(skill => skill is "react" or "node" or "typescript" or "javascript" or "python"))
        {
            categories.Add("IT");
        }

        if (normalizedMajor.Contains("marketing") || normalizedMajor.Contains("seo") || skills.Any(skill => skill.Contains("seo") || skill.Contains("content")))
        {
            categories.Add("Marketing");
        }

        if (normalizedMajor.Contains("design") || skills.Any(skill => skill.Contains("figma") || skill.Contains("ui")))
        {
            categories.Add("Design");
        }

        if (normalizedMajor.Contains("business") || normalizedMajor.Contains("finance"))
        {
            categories.Add("Business");
        }

        return categories;
    }

    private static string InferCategory(string? major, IReadOnlyList<string> skills)
    {
        return InferCategories(major, skills).FirstOrDefault() ?? string.Empty;
    }

    private static IReadOnlyList<InsightTagDto> BuildTags(IReadOnlyList<string> rawTags, string? categoryName, IReadOnlyList<string> requirements)
    {
        var labels = rawTags.Count > 0
            ? rawTags
            : requirements.Take(3).ToList();

        if (labels.Count == 0 && !string.IsNullOrWhiteSpace(categoryName))
        {
            labels = new List<string> { categoryName };
        }

        var variants = new[] { "p", "t", "a", "g" };

        return labels
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select((label, index) => new InsightTagDto
            {
                Label = label,
                Variant = variants[index % variants.Length]
            })
            .ToList();
    }

    private static string BuildLogoText(string companyName)
    {
        var words = companyName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length == 0)
        {
            return "U";
        }

        return string.Concat(words.Take(2).Select(word => char.ToUpperInvariant(word[0])));
    }

    private static string BuildGradient(string companyName)
    {
        var index = GetStableIndex(companyName) % Gradients.Length;
        return Gradients[index];
    }

    private static string BuildPay(Job job)
    {
        if (job.SalaryMin.HasValue && job.SalaryMax.HasValue)
        {
            return $"{FormatMoney(job.SalaryMin.Value)} - {FormatMoney(job.SalaryMax.Value)} {job.Currency ?? "VND"}";
        }

        if (job.SalaryMin.HasValue)
        {
            return $"Từ {FormatMoney(job.SalaryMin.Value)} {job.Currency ?? "VND"}";
        }

        if (job.SalaryMax.HasValue)
        {
            return $"Đến {FormatMoney(job.SalaryMax.Value)} {job.Currency ?? "VND"}";
        }

        return "Thỏa thuận";
    }

    private static string BuildDuration(Job job)
    {
        if (!string.IsNullOrWhiteSpace(job.DurationType) && job.DurationDays.HasValue)
        {
            return $"{job.DurationDays} ngày · {job.DurationType}";
        }

        if (!string.IsNullOrWhiteSpace(job.DurationType))
        {
            return job.DurationType;
        }

        return "N/A";
    }

    private static string FormatMoney(decimal value)
    {
        return value >= 1000000m
            ? $"{value / 1000000m:0.#} triệu"
            : value.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static IReadOnlyList<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static IReadOnlyList<string> NormalizeList(IReadOnlyList<string>? values)
    {
        if (values is null)
        {
            return Array.Empty<string>();
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }

    private static int GetStableIndex(string value)
    {
        var normalized = Normalize(value);
        var hash = 17;

        foreach (var character in normalized)
        {
            hash = hash * 31 + character;
        }

        return Math.Abs(hash);
    }
}