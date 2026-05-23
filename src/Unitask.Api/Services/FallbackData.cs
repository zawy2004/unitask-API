using Unitask.Application.Common.Interfaces;
using Unitask.Application.Common.Models;
using Unitask.Application.DTOs.Blog;
using Unitask.Application.DTOs.Businesses;
using Unitask.Application.DTOs.Common;
using Unitask.Application.DTOs.Jobs;
using Unitask.Application.DTOs.Students;
using Unitask.Application.DTOs.Users;
using Unitask.Domain.Entities;

namespace Unitask.Api.Services;

public static class FallbackData
{
    public static readonly Guid StudentUserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
    public static readonly Guid BusinessUserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440010");
    public static readonly Guid AdminUserId = Guid.Parse("550e8400-e29b-41d4-a716-4466554400aa");
    public static readonly Guid StudentProfileId = Guid.Parse("650e8400-e29b-41d4-a716-446655440001");
    public static readonly Guid BusinessProfileId = Guid.Parse("750e8400-e29b-41d4-a716-446655440001");

    public static User? TryGetDemoUser(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedPassword = password.Trim();

        if ((normalizedEmail == "student1@edu.vn" || normalizedEmail == "student@demo.com") && (normalizedPassword == "password123" || normalizedPassword == "demo123"))
        {
            return new User
            {
                Id = StudentUserId,
                Email = "student1@edu.vn",
                FullName = "Nguyễn Văn A",
                UserType = "student",
                Phone = "0987654321",
                Bio = "Sinh viên năm 4, chuyên ngành Frontend",
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
            };
        }

        if ((normalizedEmail == "technova@company.vn" || normalizedEmail == "business@demo.com") && (normalizedPassword == "password123" || normalizedPassword == "demo123"))
        {
            return new User
            {
                Id = BusinessUserId,
                Email = "technova@company.vn",
                FullName = "TechNova VN Team",
                UserType = "business",
                Phone = "0243456789",
                Bio = "Leading tech company in Vietnam",
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
            };
        }

        if (normalizedEmail == "admin@demo.com" && (normalizedPassword == "demo123" || normalizedPassword == "password123"))
        {
            return new User
            {
                Id = AdminUserId,
                Email = "admin@demo.com",
                FullName = "Admin UniTask",
                UserType = "admin",
                Phone = "0900000000",
                Bio = "Quản trị hệ thống UniTask",
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
            };
        }

        return null;
    }

    public static AuthResult CreateAuthResult(User user, IJwtTokenGenerator jwtTokenGenerator)
    {
        return new AuthResult
        {
            User = user,
            Token = jwtTokenGenerator.GenerateAccessToken(user),
            RefreshToken = jwtTokenGenerator.GenerateRefreshToken(user)
        };
    }

    public static UserProfileResponse? GetUserProfile(Guid id)
    {
        if (id == StudentUserId)
        {
            return new UserProfileResponse
            {
                Id = StudentUserId,
                Email = "student1@edu.vn",
                FullName = "Nguyễn Văn A",
                Phone = "0987654321",
                Bio = "Sinh viên năm 4, chuyên ngành Frontend",
                UserType = "student",
                IsVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
            };
        }

        if (id == BusinessUserId)
        {
            return new UserProfileResponse
            {
                Id = BusinessUserId,
                Email = "technova@company.vn",
                FullName = "TechNova VN Team",
                Phone = "0243456789",
                Bio = "Leading tech company in Vietnam",
                UserType = "business",
                IsVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
            };
        }

        if (id == AdminUserId)
        {
            return new UserProfileResponse
            {
                Id = AdminUserId,
                Email = "admin@demo.com",
                FullName = "Admin UniTask",
                Phone = "0900000000",
                Bio = "Quản trị hệ thống UniTask",
                UserType = "admin",
                IsVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
            };
        }

        return null;
    }

    public static StudentProfileResponse? GetStudentProfile(Guid userId)
    {
        if (userId != StudentUserId)
        {
            return null;
        }

        return new StudentProfileResponse
        {
            Id = StudentProfileId,
            UserId = StudentUserId,
            StudentEmail = "student1@hust.edu.vn",
            University = "Đại học Bách Khoa Hà Nội",
            Major = "Công Nghệ Thông Tin",
            GraduationYear = 2025,
            IsVerified = true,
            VerifiedAt = DateTime.UtcNow.AddDays(-20),
            CompletedJobs = 5,
            TotalEarnings = 12500000,
            Bio = "Sinh viên năm 4, chuyên ngành Frontend",
            PortfolioUrl = "https://portfolio.example.com/student1",
            CvUrl = "https://cv.example.com/student1.pdf",
        };
    }

    public static BusinessProfileResponse? GetBusinessProfile(Guid userId)
    {
        if (userId != BusinessUserId)
        {
            return null;
        }

        return new BusinessProfileResponse
        {
            Id = BusinessProfileId,
            UserId = BusinessUserId,
            CompanyName = "TechNova VN",
            CompanyEmail = "info@technova.vn",
            CompanyWebsite = "https://technova.vn",
            CompanySize = "sme",
            Industry = "Software Development",
            IsVerified = true,
            VerifiedAt = DateTime.UtcNow.AddDays(-20),
            CompletedProjects = 24,
            TotalSpent = 245000000,
            Rating = 4.8m,
            Description = "Leading tech company in Vietnam",
            Address = "Hồ Chí Minh",
        };
    }

    public static StudentDashboardResponse? GetStudentDashboard(Guid userId)
    {
        if (userId != StudentUserId)
        {
            return null;
        }

        return new StudentDashboardResponse
        {
            Student = GetStudentProfile(userId) ?? new StudentProfileResponse(),
            Wallet = new StudentWalletSummaryDto
            {
                Id = Guid.Parse("850e8400-e29b-41d4-a716-446655440101"),
                StudentId = StudentProfileId,
                Balance = 5200000,
                TotalEarned = 12500000,
                TotalWithdrawn = 7000000,
                UpdatedAt = DateTime.UtcNow,
            },
            Stats = new StudentStatsResponse
            {
                CompletedJobs = 5,
                ActiveApplications = 2,
                PendingApplications = 1,
                TotalEarnings = 12500000,
                AverageRating = 4.9m,
            },
            RecentJobs = new[]
            {
                new StudentDashboardJobDto
                {
                    Id = Jobs[0].Id,
                    Title = Jobs[0].Title,
                    CompanyName = Jobs[0].CompanyName,
                    Status = Jobs[0].Status,
                    CreatedAt = Jobs[0].CreatedAt,
                },
                new StudentDashboardJobDto
                {
                    Id = Jobs[1].Id,
                    Title = Jobs[1].Title,
                    CompanyName = Jobs[1].CompanyName,
                    Status = Jobs[1].Status,
                    CreatedAt = Jobs[1].CreatedAt,
                },
            },
            Notifications = new[]
            {
                new Unitask.Application.DTOs.Students.StudentDashboardNotificationDto
                {
                    Id = Guid.Parse("860e8400-e29b-41d4-a716-446655440101"),
                    Type = "application_status",
                    Title = "✅ Hồ sơ đã được chấp nhận",
                    Message = "TechNova VN đã chấp nhận hồ sơ của bạn.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-5),
                },
            },
        };
    }

    public static BusinessDashboardResponse? GetBusinessDashboard(Guid userId)
    {
        if (userId != BusinessUserId)
        {
            return null;
        }

        return new BusinessDashboardResponse
        {
            Business = GetBusinessProfile(userId) ?? new BusinessProfileResponse(),
            Stats = new BusinessStatsResponse
            {
                OpenJobs = 3,
                TotalApplications = 7,
                PendingApplications = 2,
                TotalSpent = 245000000,
                CompletedProjects = 24,
                AverageRating = 4.8m,
            },
            RecentApplications = new[]
            {
                new BusinessDashboardApplicationDto
                {
                    Id = Guid.Parse("870e8400-e29b-41d4-a716-446655440101"),
                    Status = "pending",
                    JobId = Jobs[0].Id,
                    JobTitle = Jobs[0].Title,
                    StudentName = "Nguyễn Văn A",
                    AppliedAt = DateTime.UtcNow.AddHours(-8),
                },
            },
            OpenJobs = new[]
            {
                new BusinessDashboardJobDto
                {
                    Id = Jobs[0].Id,
                    Title = Jobs[0].Title,
                    Status = Jobs[0].Status,
                    SpotsFilled = Jobs[0].SpotsFilled,
                    SpotsTotal = Jobs[0].SpotsTotal,
                },
            },
            Notifications = new[]
            {
                new Unitask.Application.DTOs.Businesses.StudentDashboardNotificationDto
                {
                    Id = Guid.Parse("860e8400-e29b-41d4-a716-446655440102"),
                    Type = "payment",
                    Title = "💰 Có ứng viên mới",
                    Message = "Bạn có 1 ứng viên mới đang chờ duyệt.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                },
            },
        };
    }

    private static readonly IReadOnlyList<JobCategoryInfoDto> Categories = new[]
    {
        new JobCategoryInfoDto
        {
            Id = Guid.Parse("850e8400-e29b-41d4-a716-446655440001"),
            Name = "IT & Lập Trình",
            Slug = "it-lap-trinh",
            Description = "Công việc liên quan đến phát triển phần mềm, web",
            JobCount = 3,
        },
        new JobCategoryInfoDto
        {
            Id = Guid.Parse("850e8400-e29b-41d4-a716-446655440002"),
            Name = "Thiết Kế Đồ Họa",
            Slug = "thiet-ke",
            Description = "Thiết kế UI/UX, branding, đồ họa",
            JobCount = 2,
        },
        new JobCategoryInfoDto
        {
            Id = Guid.Parse("850e8400-e29b-41d4-a716-446655440003"),
            Name = "Nội Dung & Viết Lách",
            Slug = "noi-dung",
            Description = "Viết bài, content marketing, copywriting",
            JobCount = 2,
        },
        new JobCategoryInfoDto
        {
            Id = Guid.Parse("850e8400-e29b-41d4-a716-446655440004"),
            Name = "Marketing & Quảng Cáo",
            Slug = "marketing",
            Description = "Marketing digital, SEO, quảng cáo, SMM",
            JobCount = 2,
        },
        new JobCategoryInfoDto
        {
            Id = Guid.Parse("850e8400-e29b-41d4-a716-446655440005"),
            Name = "Dịch & Biên Dịch",
            Slug = "dich-thuat",
            Description = "Dịch tài liệu, dịch website",
            JobCount = 1,
        },
        new JobCategoryInfoDto
        {
            Id = Guid.Parse("850e8400-e29b-41d4-a716-446655440006"),
            Name = "Video & Multimedia",
            Slug = "video",
            Description = "Làm video, editing, animation",
            JobCount = 1,
        },
    };

    private static readonly IReadOnlyList<JobListItemResponse> Jobs = new[]
    {
        new JobListItemResponse
        {
            Id = Guid.Parse("950e8400-e29b-41d4-a716-446655440001"),
            Title = "Frontend Developer (React + Tailwind)",
            Description = "Cần nhà phát triển Frontend để xây dựng giao diện website e-commerce.",
            CategoryId = Categories[0].Id,
            CategoryName = Categories[0].Name,
            BusinessId = Guid.Parse("750e8400-e29b-41d4-a716-446655440001"),
            CompanyName = "TechNova VN",
            Tags = new[] { "React", "Frontend", "E-commerce" },
            Status = "open",
            SalaryMin = 2500000,
            SalaryMax = 4000000,
            DurationType = "short-term",
            DurationDays = 14,
            RequiredSkills = new[] { "React", "TypeScript", "Tailwind CSS" },
            ExperienceLevel = "intermediate",
            SpotsTotal = 2,
            SpotsFilled = 1,
            Location = "Hồ Chí Minh",
            IsRemote = true,
            IsFeatured = true,
            Deadline = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            PublishedAt = DateTime.UtcNow.AddDays(-2),
        },
        new JobListItemResponse
        {
            Id = Guid.Parse("950e8400-e29b-41d4-a716-446655440002"),
            Title = "Fix Bug Python Flask API - E-commerce",
            Description = "Ứng dụng Flask gặp lỗi trong xử lý thanh toán. Cần debug và khắc phục trong 3 ngày.",
            CategoryId = Categories[0].Id,
            CategoryName = Categories[0].Name,
            BusinessId = Guid.Parse("750e8400-e29b-41d4-a716-446655440004"),
            CompanyName = "DevStack JSC",
            Tags = new[] { "Python", "Backend", "Debug" },
            Status = "open",
            SalaryMin = 2000000,
            SalaryMax = 3500000,
            DurationType = "micro",
            DurationDays = 3,
            RequiredSkills = new[] { "Python", "Flask", "PostgreSQL" },
            ExperienceLevel = "advanced",
            SpotsTotal = 1,
            SpotsFilled = 0,
            Location = "Remote",
            IsRemote = true,
            IsFeatured = false,
            Deadline = DateTime.UtcNow.AddDays(3),
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            PublishedAt = DateTime.UtcNow.AddDays(-3),
        },
        new JobListItemResponse
        {
            Id = Guid.Parse("950e8400-e29b-41d4-a716-446655440003"),
            Title = "Xây dựng REST API với Node.js",
            Description = "Phát triển REST API cho ứng dụng quản lý dự án.",
            CategoryId = Categories[0].Id,
            CategoryName = Categories[0].Name,
            BusinessId = Guid.Parse("750e8400-e29b-41d4-a716-446655440001"),
            CompanyName = "TechNova VN",
            Tags = new[] { "Node.js", "Backend", "API" },
            Status = "open",
            SalaryMin = 4000000,
            SalaryMax = 6000000,
            DurationType = "project",
            DurationDays = 30,
            RequiredSkills = new[] { "Node.js", "Express", "MongoDB" },
            ExperienceLevel = "intermediate",
            SpotsTotal = 1,
            SpotsFilled = 0,
            Location = "Remote",
            IsRemote = true,
            IsFeatured = false,
            Deadline = DateTime.UtcNow.AddDays(10),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            PublishedAt = DateTime.UtcNow.AddDays(-1),
        },
    };

    private static readonly IReadOnlyList<BlogPostListItemResponse> BlogPosts = new[]
    {
        new BlogPostListItemResponse
        {
            Id = Guid.Parse("a50e8400-e29b-41d4-a716-446655440001"),
            Title = "Cách viết profile sinh viên để tăng tỷ lệ được nhận job",
            Slug = "cach-viet-profile-sinh-vien",
            Excerpt = "Tối ưu bio, kỹ năng, và portfolio để nổi bật hơn trong mắt doanh nghiệp.",
            Category = "Career",
            Status = "published",
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            PublishedAt = DateTime.UtcNow.AddDays(-7),
        },
        new BlogPostListItemResponse
        {
            Id = Guid.Parse("a50e8400-e29b-41d4-a716-446655440002"),
            Title = "5 mẹo đăng job để tìm được ứng viên phù hợp nhanh hơn",
            Slug = "meo-dang-job-tim-ung-vien",
            Excerpt = "Viết mô tả rõ ràng, đặt deadline đúng, và dùng tag hiệu quả.",
            Category = "Business",
            Status = "published",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            PublishedAt = DateTime.UtcNow.AddDays(-5),
        },
    };

    public static PagedResult<JobListItemResponse> GetJobs(int page, int limit)
    {
        var data = Jobs.Skip((page - 1) * limit).Take(limit).ToList();
        return new PagedResult<JobListItemResponse>
        {
            Total = Jobs.Count,
            Page = page,
            Limit = limit,
            Data = data,
        };
    }

    public static JobDetailsResponse? GetJobById(Guid id)
    {
        var job = Jobs.FirstOrDefault(item => item.Id == id);
        if (job is null)
        {
            return null;
        }

        return new JobDetailsResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Category = Categories.FirstOrDefault(category => category.Id == job.CategoryId),
            Business = new JobBusinessInfoDto
            {
                Id = job.BusinessId,
                CompanyName = job.CompanyName ?? "UniTask",
                Rating = 4.8m,
            },
            Tags = job.Tags,
            Status = job.Status,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            RequiredSkills = job.RequiredSkills,
            SpotsTotal = job.SpotsTotal,
            SpotsFilled = job.SpotsFilled,
            Location = job.Location,
            IsRemote = job.IsRemote,
            DurationType = job.DurationType,
            DurationDays = job.DurationDays,
            Deadline = job.Deadline,
            Applications = Array.Empty<JobApplicationSummaryDto>(),
            CreatedAt = job.CreatedAt,
            PublishedAt = job.PublishedAt,
        };
    }

    public static IReadOnlyList<JobCategoryInfoDto> GetCategories() => Categories;

    public static PagedResult<BlogPostListItemResponse> GetBlogPosts(int page, int limit)
    {
        var data = BlogPosts.Skip((page - 1) * limit).Take(limit).ToList();
        return new PagedResult<BlogPostListItemResponse>
        {
            Total = BlogPosts.Count,
            Page = page,
            Limit = limit,
            Data = data,
        };
    }

    public static BlogPostResponse? GetBlogPostBySlug(string slug)
    {
        var post = BlogPosts.FirstOrDefault(item => string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (post is null)
        {
            return null;
        }

        return new BlogPostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = $"{post.Excerpt}\n\nNội dung demo được trả về khi database AWS chưa sẵn sàng.",
            Excerpt = post.Excerpt,
            Category = post.Category,
            Tags = new[] { "Demo", "Fallback" },
            Status = post.Status,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.CreatedAt,
            PublishedAt = post.PublishedAt,
            AuthorName = "UniTask Team",
        };
    }
}