-- UniTask Seed Data for SQL Server
-- Sample data for development and testing
-- Bcrypt hashed password: "password123"
-- Hash: $2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm
--
-- LƯU Ý TIẾNG VIỆT (Unicode):
--   1) Mọi chuỗi có dấu PHẢI dùng tiền tố N'...'  (NVARCHAR), ví dụ N'Nguyễn Văn A'.
--   2) Chạy file bằng UTF-8 để không bị lỗi "????":
--        sqlcmd -S localhost -d unitask -E -C -f 65001 -i seed-sqlserver.sql
--      (hoặc lưu file dạng "UTF-8 with BOM" rồi mở bằng SSMS để chạy)

BEGIN TRANSACTION;

-- ==========================================
-- USERS DATA
-- ==========================================

INSERT INTO [dbo].[Users] ([Id], [Email], [PasswordHash], [FullName], [Phone], [Bio], [UserType], [IsVerified])
VALUES
-- Students
('550e8400-e29b-41d4-a716-446655440001', 'student1@edu.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Nguyễn Văn A', '0987654321', N'Sinh viên năm 4, chuyên ngành Frontend', 'student', 1),
('550e8400-e29b-41d4-a716-446655440002', 'student2@edu.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Trần Thị B', '0987654322', N'Thiết kế đồ họa, yêu thích UI/UX', 'student', 1),
('550e8400-e29b-41d4-a716-446655440003', 'student3@edu.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Lê Hoàng C', '0987654323', N'Marketing & Content Creator', 'student', 1),
('550e8400-e29b-41d4-a716-446655440004', 'student4@edu.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Phạm Minh D', '0987654324', N'Backend Developer, Python & Node.js', 'student', 0),
-- Businesses
('550e8400-e29b-41d4-a716-446655440010', 'technova@company.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Đội ngũ TechNova VN', '0243456789', N'Công ty công nghệ hàng đầu Việt Nam', 'business', 1),
('550e8400-e29b-41d4-a716-446655440011', 'creative@company.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'CreativeBox Studio', '0243456790', N'Agency sáng tạo & thiết kế', 'business', 1),
('550e8400-e29b-41d4-a716-446655440012', 'market@company.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'MarketHub VN', '0243456791', N'Giải pháp Marketing số', 'business', 1),
('550e8400-e29b-41d4-a716-446655440013', 'devstack@company.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'DevStack JSC', '0243456792', N'Phát triển phần mềm & tư vấn công nghệ', 'business', 1);

PRINT 'Inserted 8 users (4 students + 4 businesses)';
GO

-- ==========================================
-- STUDENT PROFILES DATA
-- ==========================================

INSERT INTO [dbo].[StudentProfiles] ([Id], [UserId], [StudentEmail], [University], [Major], [GraduationYear], [IsVerified], [VerifiedAt], [CompletedJobs], [TotalEarnings])
VALUES
('650e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440001', 'student1@hust.edu.vn', N'Đại học Bách Khoa Hà Nội', N'Công Nghệ Thông Tin', 2025, 1, GETUTCDATE(), 5, 12500000),
('650e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440002', 'student2@fpt.edu.vn', N'Đại học FPT', N'Thiết Kế Đồ Họa', 2024, 1, GETUTCDATE(), 8, 18000000),
('650e8400-e29b-41d4-a716-446655440003', '550e8400-e29b-41d4-a716-446655440003', 'student3@hcmut.edu.vn', N'Đại học Bách Khoa TP.HCM', N'Marketing', 2025, 1, GETUTCDATE(), 3, 6200000),
('650e8400-e29b-41d4-a716-446655440004', '550e8400-e29b-41d4-a716-446655440004', 'student4@vnu.edu.vn', N'Đại học Quốc Gia Hà Nội', N'Công Nghệ Thông Tin', 2026, 0, NULL, 1, 3000000);

PRINT 'Inserted 4 student profiles';
GO

-- ==========================================
-- BUSINESS PROFILES DATA
-- ==========================================

INSERT INTO [dbo].[BusinessProfiles] ([Id], [UserId], [CompanyName], [CompanyEmail], [CompanyWebsite], [CompanySize], [Industry], [IsVerified], [VerifiedAt], [CompletedProjects], [TotalSpent], [Rating])
VALUES
('750e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440010', N'TechNova VN', 'info@technova.vn', 'https://technova.vn', 'sme', N'Phát triển phần mềm', 1, GETUTCDATE(), 24, 245000000, 4.8),
('750e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440011', N'CreativeBox Studio', 'hello@creativebox.vn', 'https://creativebox.vn', 'startup', N'Agency thiết kế', 1, GETUTCDATE(), 18, 156000000, 4.6),
('750e8400-e29b-41d4-a716-446655440003', '550e8400-e29b-41d4-a716-446655440012', N'MarketHub VN', 'contact@markethub.vn', 'https://markethub.vn', 'sme', N'Marketing số', 1, GETUTCDATE(), 32, 320000000, 4.7),
('750e8400-e29b-41d4-a716-446655440004', '550e8400-e29b-41d4-a716-446655440013', N'DevStack JSC', 'hr@devstack.vn', 'https://devstack.vn', 'sme', N'Phát triển phần mềm', 1, GETUTCDATE(), 28, 280000000, 4.9);

PRINT 'Inserted 4 business profiles';
GO

-- ==========================================
-- JOB CATEGORIES DATA
-- ==========================================

INSERT INTO [dbo].[JobCategories] ([Id], [Name], [Slug], [Description])
VALUES
('850e8400-e29b-41d4-a716-446655440001', N'IT & Lập Trình', 'it-lap-trinh', N'Công việc liên quan đến phát triển phần mềm, web'),
('850e8400-e29b-41d4-a716-446655440002', N'Thiết Kế Đồ Họa', 'thiet-ke', N'Thiết kế UI/UX, branding, đồ họa'),
('850e8400-e29b-41d4-a716-446655440003', N'Nội Dung & Viết Lách', 'noi-dung', N'Viết bài, content marketing, copywriting'),
('850e8400-e29b-41d4-a716-446655440004', N'Marketing & Quảng Cáo', 'marketing', N'Marketing digital, SEO, quảng cáo, SMM'),
('850e8400-e29b-41d4-a716-446655440005', N'Dịch & Biên Dịch', 'dich-thuat', N'Dịch tài liệu, dịch website'),
('850e8400-e29b-41d4-a716-446655440006', N'Video & Multimedia', 'video', N'Làm video, editing, animation');

PRINT 'Inserted 6 job categories';
GO

-- ==========================================
-- JOBS DATA
-- ==========================================

INSERT INTO [dbo].[Jobs] ([Id], [BusinessId], [Title], [Description], [CategoryId], [TagsJson], [Status], [SalaryMin], [SalaryMax], [DurationType], [DurationDays], [RequiredSkillsJson], [ExperienceLevel], [SpotsTotal], [SpotsFilled], [Location], [IsRemote], [IsFeatured], [Deadline], [PublishedAt])
VALUES
-- IT Jobs
('950e8400-e29b-41d4-a716-446655440001', '750e8400-e29b-41d4-a716-446655440001', N'Frontend Developer (React + Tailwind)', 
N'Cần nhà phát triển Frontend để xây dựng giao diện website e-commerce. Yêu cầu: React, TypeScript, Tailwind CSS. Dự án kéo dài 2 tuần, 20 giờ/tuần.',
'850e8400-e29b-41d4-a716-446655440001', N'["React", "Frontend", "E-commerce"]', 'open', 2500000, 4000000, 'short-term', 14, N'["React", "TypeScript", "Tailwind CSS"]', 'intermediate', 2, 1, N'Hồ Chí Minh', 1, 1, DATEADD(day, 5, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),

('950e8400-e29b-41d4-a716-446655440002', '750e8400-e29b-41d4-a716-446655440004', N'Fix Bug Python Flask API - E-commerce',
N'Ứng dụng Flask gặp lỗi trong xử lý thanh toán. Cần debug và khắc phục trong 3 ngày. API kết nối với database PostgreSQL.',
'850e8400-e29b-41d4-a716-446655440001', N'["Python", "Backend", "Debug"]', 'open', 2000000, 3500000, 'micro', 3, N'["Python", "Flask", "PostgreSQL"]', 'advanced', 1, 0, N'Remote', 1, 0, DATEADD(day, 3, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),

('950e8400-e29b-41d4-a716-446655440003', '750e8400-e29b-41d4-a716-446655440001', N'Xây dựng REST API với Node.js',
N'Phát triển REST API cho ứng dụng quản lý dự án. Stack: Node.js, Express, MongoDB. Dự kiến 1 tháng.',
'850e8400-e29b-41d4-a716-446655440001', N'["Node.js", "Backend", "API"]', 'open', 4000000, 6000000, 'project', 30, N'["Node.js", "Express", "MongoDB"]', 'intermediate', 1, 0, N'Remote', 1, 0, DATEADD(day, 10, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),

-- Design Jobs
('950e8400-e29b-41d4-a716-446655440004', '750e8400-e29b-41d4-a716-446655440002', N'Thiết kế Bộ nhận diện thương hiệu',
N'Tạo logo, color palette, typography cho startup fintech. Cần 5-7 khái niệm logo khác nhau và brand guideline hoàn chỉnh.',
'850e8400-e29b-41d4-a716-446655440002', N'["Branding", "Logo Design", "Adobe"]', 'open', 1800000, 3000000, 'micro', 5, N'["Adobe XD", "Figma", "Illustrator"]', 'intermediate', 2, 0, N'Hà Nội', 1, 0, DATEADD(day, 8, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),

('950e8400-e29b-41d4-a716-446655440005', '750e8400-e29b-41d4-a716-446655440002', N'Thiết kế UI/UX cho Mobile App',
N'Thiết kế giao diện cho ứng dụng fitness tracking. Bao gồm wireframe, mockup, prototype. Sử dụng Figma.',
'850e8400-e29b-41d4-a716-446655440002', N'["UI/UX", "Mobile", "Figma"]', 'open', 3000000, 5000000, 'project', 21, N'["Figma", "UI Design", "UX Design"]', 'intermediate', 1, 0, N'Remote', 1, 0, DATEADD(day, 12, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),

-- Content Jobs
('950e8400-e29b-41d4-a716-446655440006', '750e8400-e29b-41d4-a716-446655440003', N'Viết 10 bài SEO Blog (chuẩn EEAT)',
N'Viết bài blog về marketing digital, SEO. Mỗi bài 2000+ từ, chuẩn E-E-A-T (Google). Topic từ client cung cấp.',
'850e8400-e29b-41d4-a716-446655440003', N'["SEO", "Content Writing", "Marketing"]', 'open', 1200000, 2000000, 'micro', 7, N'["SEO Writing", "Content Marketing"]', 'beginner', 5, 2, N'Remote', 1, 1, DATEADD(day, 12, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),

('950e8400-e29b-41d4-a716-446655440007', '750e8400-e29b-41d4-a716-446655440003', N'Copywriting cho Email Marketing Campaign',
N'Viết 15 email marketing sequences cho sản phẩm SaaS. Email conversion-focused, A/B testing ready.',
'850e8400-e29b-41d4-a716-446655440003', N'["Copywriting", "Email Marketing"]', 'open', 800000, 1500000, 'micro', 5, N'["Copywriting", "Marketing"]', 'beginner', 3, 1, N'Remote', 1, 0, DATEADD(day, 10, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),

-- Marketing Jobs
('950e8400-e29b-41d4-a716-446655440008', '750e8400-e29b-41d4-a716-446655440003', N'Chạy quảng cáo Facebook Ads - F&B',
N'Quản lý campaign Facebook/Instagram Ads cho nhà hàng F&B. Budget: 5-10M/tháng. KPI: bán hàng/booking.',
'850e8400-e29b-41d4-a716-446655440004', N'["Facebook Ads", "Social Media", "Marketing"]', 'open', 2000000, 3500000, 'short-term', 30, N'["Facebook Ads", "Google Analytics"]', 'intermediate', 2, 0, N'Đà Nẵng', 0, 1, DATEADD(day, 3, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),

('950e8400-e29b-41d4-a716-446655440009', '750e8400-e29b-41d4-a716-446655440003', N'Quản lý Social Media - 30 ngày',
N'Quản lý TikTok/Instagram cho startup startup. Viết nội dung, lên schedule, theo dõi analytics hàng ngày.',
'850e8400-e29b-41d4-a716-446655440004', N'["Social Media", "TikTok", "Instagram"]', 'open', 3000000, 5000000, 'short-term', 30, N'["Social Media Management", "Content Creation"]', 'intermediate', 2, 0, N'Remote', 1, 0, DATEADD(day, 7, CAST(GETUTCDATE() AS DATE)), GETUTCDATE());

PRINT 'Inserted 9 jobs';
GO

-- ==========================================
-- JOB APPLICATIONS DATA
-- ==========================================

INSERT INTO [dbo].[JobApplications] ([Id], [JobId], [StudentId], [Status], [AppliedAt], [CoverLetter], [AcceptedAt], [StartedAt])
VALUES
('a50e8400-e29b-41d4-a716-446655440001', '950e8400-e29b-41d4-a716-446655440001', '650e8400-e29b-41d4-a716-446655440001', 'accepted', DATEADD(day, -2, GETUTCDATE()), N'Mình có kinh nghiệm 2 năm với React. Rất hứng thú với dự án này!', DATEADD(day, -1, GETUTCDATE()), GETUTCDATE()),
('a50e8400-e29b-41d4-a716-446655440002', '950e8400-e29b-41d4-a716-446655440004', '650e8400-e29b-41d4-a716-446655440002', 'pending', GETUTCDATE(), N'Đã làm 8 dự án branding trước. Portfolio: [link]', NULL, NULL),
('a50e8400-e29b-41d4-a716-446655440003', '950e8400-e29b-41d4-a716-446655440006', '650e8400-e29b-41d4-a716-446655440003', 'accepted', DATEADD(day, -1, GETUTCDATE()), N'Chuyên SEO, có 15+ bài 2000+ từ. Hiểu rõ thuật toán Google.', GETUTCDATE(), GETUTCDATE()),
('a50e8400-e29b-41d4-a716-446655440004', '950e8400-e29b-41d4-a716-446655440002', '650e8400-e29b-41d4-a716-446655440004', 'pending', DATEADD(hour, -3, GETUTCDATE()), N'Làm Python/Flask 3 năm, có thể bắt đầu ngay.', NULL, NULL);

PRINT 'Inserted 4 job applications';
GO

-- ==========================================
-- STUDENT WALLETS DATA
-- ==========================================

INSERT INTO [dbo].[StudentWallets] ([Id], [StudentId], [Balance], [TotalEarned], [TotalWithdrawn])
VALUES
('b50e8400-e29b-41d4-a716-446655440001', '650e8400-e29b-41d4-a716-446655440001', 2500000, 12500000, 10000000),
('b50e8400-e29b-41d4-a716-446655440002', '650e8400-e29b-41d4-a716-446655440002', 5000000, 18000000, 13000000),
('b50e8400-e29b-41d4-a716-446655440003', '650e8400-e29b-41d4-a716-446655440003', 1000000, 6200000, 5200000),
('b50e8400-e29b-41d4-a716-446655440004', '650e8400-e29b-41d4-a716-446655440004', 3000000, 3000000, 0);

PRINT 'Inserted 4 student wallets';
GO

-- ==========================================
-- PAYMENTS DATA
-- ==========================================

INSERT INTO [dbo].[Payments] ([Id], [JobId], [JobApplicationId], [BusinessId], [StudentId], [Amount], [Status], [PaymentMethod], [CreatedAt], [ReleasedAt])
VALUES
('c50e8400-e29b-41d4-a716-446655440001', '950e8400-e29b-41d4-a716-446655440001', 'a50e8400-e29b-41d4-a716-446655440001', '750e8400-e29b-41d4-a716-446655440001', '650e8400-e29b-41d4-a716-446655440001', 3000000, 'released', 'bank_transfer', DATEADD(day, -1, GETUTCDATE()), GETUTCDATE()),
('c50e8400-e29b-41d4-a716-446655440002', '950e8400-e29b-41d4-a716-446655440006', 'a50e8400-e29b-41d4-a716-446655440003', '750e8400-e29b-41d4-a716-446655440003', '650e8400-e29b-41d4-a716-446655440003', 1500000, 'escrow', 'bank_transfer', GETUTCDATE(), NULL);

PRINT 'Inserted 2 payments';
GO

-- ==========================================
-- REVIEWS DATA
-- ==========================================

INSERT INTO [dbo].[Reviews] ([Id], [JobId], [JobApplicationId], [FromUserId], [ToUserId], [Rating], [Comment], [SkillEndorsementsJson])
VALUES
('d50e8400-e29b-41d4-a716-446655440001', '950e8400-e29b-41d4-a716-446655440001', 'a50e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440001', 5, N'Code rất sạch, commit messages rõ ràng. Giao tiếp tốt, giao hàng đúng hạn. Sẽ hợp tác lại!', N'["React", "TypeScript", "Professional"]');

PRINT 'Inserted 1 review';
GO

-- ==========================================
-- SKILLS DATA
-- ==========================================

INSERT INTO [dbo].[Skills] ([Id], [Name], [Category])
VALUES
('e50e8400-e29b-41d4-a716-446655440001', 'React', 'Frontend'),
('e50e8400-e29b-41d4-a716-446655440002', 'TypeScript', 'Frontend'),
('e50e8400-e29b-41d4-a716-446655440003', 'Node.js', 'Backend'),
('e50e8400-e29b-41d4-a716-446655440004', 'Python', 'Backend'),
('e50e8400-e29b-41d4-a716-446655440005', 'Figma', 'Design'),
('e50e8400-e29b-41d4-a716-446655440006', 'Adobe XD', 'Design'),
('e50e8400-e29b-41d4-a716-446655440007', 'SEO Writing', 'Content'),
('e50e8400-e29b-41d4-a716-446655440008', 'Facebook Ads', 'Marketing'),
('e50e8400-e29b-41d4-a716-446655440009', 'Content Marketing', 'Marketing'),
('e50e8400-e29b-41d4-a716-446655440010', 'English Translation', 'Language');

PRINT 'Inserted 10 skills';
GO

-- ==========================================
-- STUDENT SKILLS DATA
-- ==========================================

INSERT INTO [dbo].[StudentSkills] ([Id], [StudentId], [SkillId], [Proficiency], [EndorsementCount])
VALUES
('f50e8400-e29b-41d4-a716-446655440001', '650e8400-e29b-41d4-a716-446655440001', 'e50e8400-e29b-41d4-a716-446655440001', 'advanced', 5),
('f50e8400-e29b-41d4-a716-446655440002', '650e8400-e29b-41d4-a716-446655440001', 'e50e8400-e29b-41d4-a716-446655440002', 'advanced', 3),
('f50e8400-e29b-41d4-a716-446655440003', '650e8400-e29b-41d4-a716-446655440002', 'e50e8400-e29b-41d4-a716-446655440005', 'expert', 8),
('f50e8400-e29b-41d4-a716-446655440004', '650e8400-e29b-41d4-a716-446655440002', 'e50e8400-e29b-41d4-a716-446655440006', 'advanced', 4),
('f50e8400-e29b-41d4-a716-446655440005', '650e8400-e29b-41d4-a716-446655440003', 'e50e8400-e29b-41d4-a716-446655440007', 'advanced', 6),
('f50e8400-e29b-41d4-a716-446655440006', '650e8400-e29b-41d4-a716-446655440003', 'e50e8400-e29b-41d4-a716-446655440008', 'intermediate', 3);

PRINT 'Inserted 6 student skills';
GO

-- ==========================================
-- NOTIFICATIONS DATA
-- ==========================================

INSERT INTO [dbo].[Notifications] ([Id], [UserId], [Type], [Title], [Message], [RelatedJobId], [IsRead])
VALUES
('950e8400-e29b-41d4-a716-000000000001', '550e8400-e29b-41d4-a716-446655440001', 'application_accepted', N'Ứng dụng được chấp nhận', N'Ứng dụng của bạn cho vị trí Frontend Developer đã được chấp nhận!', '950e8400-e29b-41d4-a716-446655440001', 1),
('950e8400-e29b-41d4-a716-000000000002', '550e8400-e29b-41d4-a716-446655440002', 'new_job_matched', N'Công việc phù hợp', N'Có công việc thiết kế mới phù hợp với kỹ năng của bạn!', '950e8400-e29b-41d4-a716-446655440004', 0),
('950e8400-e29b-41d4-a716-000000000003', '550e8400-e29b-41d4-a716-446655440010', 'job_applied', N'Ứng viên mới', N'Có ứng viên mới ứng tuyển cho vị trí Frontend Developer', '950e8400-e29b-41d4-a716-446655440001', 0);

PRINT 'Inserted 3 notifications';
GO

-- ==========================================
-- FAQ DATA
-- ==========================================

INSERT INTO [dbo].[FAQs] ([Id], [Question], [Answer], [Category], [OrderIndex])
VALUES
('a60e8400-e29b-41d4-a716-446655440001', N'Làm thế nào để xác thực tài khoản sinh viên?', N'Bạn cần cung cấp email đuôi .edu hoặc ảnh thẻ sinh viên. Chúng tôi sẽ xác thực trong vòng 24-48 giờ.', 'verification', 1),
('a60e8400-e29b-41d4-a716-446655440002', N'Tiền từ công việc hoàn thành sẽ về khi nào?', N'Tiền được giữ trong Escrow. Khi doanh nghiệp duyệt hoàn thành, tiền sẽ chuyển vào ví của bạn trong 1-2 ngày.', 'payment', 2),
('a60e8400-e29b-41d4-a716-446655440003', N'Tôi rút tiền ra ngân hàng như thế nào?', N'Vào Ví của bạn > Rút tiền > Nhập thông tin ngân hàng. Yêu cầu rút tiền sẽ được xử lý trong 3-5 ngày.', 'payment', 3),
('a60e8400-e29b-41d4-a716-446655440004', N'Có phí giao dịch không?', N'Không, UniTask không tính phí. Tất cả tiền kiếm được đều về ví của bạn.', 'payment', 4);

PRINT 'Inserted 4 FAQs';
GO

COMMIT TRANSACTION;

PRINT '========================================';
PRINT 'Seed Data Inserted Successfully!';
PRINT '========================================';
PRINT 'Summary:';
PRINT '- Users: 8 (4 students, 4 businesses)';
PRINT '- Student Profiles: 4';
PRINT '- Business Profiles: 4';
PRINT '- Job Categories: 6';
PRINT '- Jobs: 9';
PRINT '- Job Applications: 4';
PRINT '- Student Wallets: 4';
PRINT '- Payments: 2';
PRINT '- Reviews: 1';
PRINT '- Skills: 10';
PRINT '- Student Skills: 6';
PRINT '- Notifications: 3';
PRINT '- FAQs: 4';
PRINT '========================================';
