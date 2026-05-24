-- ============================================================
-- UniTask Extended Seed Data
-- Adds realistic Vietnamese data on top of seed-sqlserver.sql
-- Idempotent: uses fixed GUID prefixes so re-running is safe.
-- ============================================================

BEGIN TRANSACTION;

DELETE FROM [dbo].[Reviews]           WHERE [Id] LIKE 'd60e8400-%';
DELETE FROM [dbo].[StudentSkills]     WHERE [Id] LIKE 'f60e8400-%';
DELETE FROM [dbo].[Payments]          WHERE [Id] LIKE 'c60e8400-%';
DELETE FROM [dbo].[JobApplications]   WHERE [Id] LIKE 'a60e8400-%';
DELETE FROM [dbo].[Jobs]              WHERE [Id] LIKE '960e8400-%';
DELETE FROM [dbo].[Skills]            WHERE [Id] LIKE 'e60e8400-%';
DELETE FROM [dbo].[FAQs]              WHERE [Id] LIKE 'a70e8400-%';
DELETE FROM [dbo].[BlogPosts]         WHERE [Id] LIKE 'b70e8400-%';
DELETE FROM [dbo].[StudentWallets]    WHERE [Id] LIKE 'b60e8400-%';
DELETE FROM [dbo].[StudentProfiles]   WHERE [Id] LIKE '660e8400-%';
DELETE FROM [dbo].[BusinessProfiles]  WHERE [Id] LIKE '760e8400-%';
DELETE FROM [dbo].[Users]             WHERE [Id] LIKE '560e8400-%';
PRINT 'Cleared previous extended seed (if any)';
GO

INSERT INTO [dbo].[Users] ([Id], [Email], [PasswordHash], [FullName], [Phone], [Bio], [UserType], [IsVerified])
VALUES
('560e8400-e29b-41d4-a716-446655440001', 'mai.nguyen@edu.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Nguyễn Hoài Mai',  '0901234567', N'Sinh viên năm 3 chuyên ngành Khoa học Máy tính, đam mê Machine Learning và Python.', 'student', 1),
('560e8400-e29b-41d4-a716-446655440002', 'duc.tran@edu.vn',  '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Trần Anh Đức',     '0902345678', N'Full-stack Developer thiên về React + Node.js. Đã làm 4 dự án freelance.',          'student', 1),
('560e8400-e29b-41d4-a716-446655440003', 'linh.pham@edu.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Phạm Khánh Linh',  '0903456789', N'UI/UX Designer, thành thạo Figma và có portfolio mạnh về mobile app.',              'student', 1),
('560e8400-e29b-41d4-a716-446655440004', 'hoang.le@edu.vn',  '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Lê Quốc Hoàng',    '0904567890', N'Mobile Developer, làm React Native và Flutter.',                                    'student', 1),
('560e8400-e29b-41d4-a716-446655440005', 'thao.bui@edu.vn',  '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Bùi Thu Thảo',     '0905678901', N'Content Creator & Copywriter, viết SEO blog cho 5 doanh nghiệp.',                  'student', 1),
('560e8400-e29b-41d4-a716-446655440006', 'minh.do@edu.vn',   '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Đỗ Tuấn Minh',     '0906789012', N'Backend Developer, chuyên Java Spring Boot và microservices.',                      'student', 1),
('560e8400-e29b-41d4-a716-446655440007', 'an.vu@edu.vn',     '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Vũ Bình An',       '0907890123', N'Data Analyst tập sự, mạnh về SQL, Power BI, Python pandas.',                        'student', 0),
('560e8400-e29b-41d4-a716-446655440008', 'huy.dang@edu.vn',  '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Đặng Gia Huy',     '0908901234', N'Marketing Digital, chuyên Facebook Ads và Google Ads.',                             'student', 1),
('560e8400-e29b-41d4-a716-446655440009', 'ha.ngo@edu.vn',    '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Ngô Thu Hà',       '0909012345', N'Graphic Designer, có kinh nghiệm thiết kế bộ nhận diện thương hiệu.',              'student', 1),
('560e8400-e29b-41d4-a716-446655440010', 'kien.ho@edu.vn',   '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Hồ Trung Kiên',    '0910123456', N'DevOps tập sự, biết Docker, Kubernetes, CI/CD GitHub Actions.',                    'student', 1),
('560e8400-e29b-41d4-a716-446655440011', 'oanh.duong@edu.vn','$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Dương Kim Oanh',   '0911234567', N'Video Editor, thành thạo Adobe Premiere và DaVinci Resolve.',                       'student', 1),
('560e8400-e29b-41d4-a716-446655440012', 'phong.ly@edu.vn',  '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'Lý Hoàng Phong',   '0912345678', N'Dịch thuật Anh-Việt, chuyên ngành kỹ thuật và IT.',                                  'student', 0),
('560e8400-e29b-41d4-a716-446655440020', 'cto@viettech.vn',  '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'VietTech Solutions','0241234567', N'Công ty phát triển phần mềm chuyên về Fintech.',                                    'business', 1),
('560e8400-e29b-41d4-a716-446655440021', 'hr@greenfarm.vn',  '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'GreenFarm Vietnam','0242345678', N'Nông nghiệp công nghệ cao, app kết nối nông dân và khách hàng.',                    'business', 1),
('560e8400-e29b-41d4-a716-446655440022', 'team@studibox.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'StudiBox EdTech',   '0243456789', N'Nền tảng học online cho sinh viên Việt Nam.',                                       'business', 1),
('560e8400-e29b-41d4-a716-446655440023', 'pm@cloudkit.vn',   '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'CloudKit JSC',     '0244567890', N'Cloud infrastructure & DevOps services.',                                           'business', 1),
('560e8400-e29b-41d4-a716-446655440024', 'hello@brandlab.vn','$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'BrandLab Agency',  '0245678901', N'Branding studio & creative agency Hà Nội.',                                          'business', 1),
('560e8400-e29b-41d4-a716-446655440025', 'contact@mediawave.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'MediaWave',     '0246789012', N'Production house: video commercial, motion graphic.',                              'business', 1),
('560e8400-e29b-41d4-a716-446655440026', 'jobs@finhub.vn',   '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'FinHub Vietnam',   '0247890123', N'Fintech startup, ví điện tử cho sinh viên.',                                        'business', 1),
('560e8400-e29b-41d4-a716-446655440027', 'careers@nutribite.vn', '$2b$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86E36gZvWFm', N'NutriBite',     '0248901234', N'Thương hiệu thực phẩm healthy, kênh D2C qua TikTok Shop.',                          'business', 1);
PRINT 'Inserted 20 users';
GO

INSERT INTO [dbo].[StudentProfiles] ([Id], [UserId], [StudentEmail], [University], [Major], [GraduationYear], [IsVerified], [VerifiedAt], [CompletedJobs], [TotalEarnings], [Bio], [PortfolioUrl])
VALUES
('660e8400-e29b-41d4-a716-446655440001', '560e8400-e29b-41d4-a716-446655440001', 'mai.nguyen@hust.edu.vn',   N'Đại học Bách Khoa Hà Nội',      N'Khoa học Máy tính',         2026, 1, GETUTCDATE(), 4,  9500000,  N'ML enthusiast, đã làm 2 dự án nhận dạng ảnh.',           'https://github.com/mainguyen'),
('660e8400-e29b-41d4-a716-446655440002', '560e8400-e29b-41d4-a716-446655440002', 'duc.tran@fpt.edu.vn',      N'Đại học FPT',                    N'Kỹ thuật phần mềm',         2025, 1, GETUTCDATE(), 6,  16800000, N'Full-stack web với React + Node.js.',                    'https://anhduc.dev'),
('660e8400-e29b-41d4-a716-446655440003', '560e8400-e29b-41d4-a716-446655440003', 'linh.pham@rmit.edu.vn',    N'Đại học RMIT Việt Nam',          N'Thiết kế Đa phương tiện',   2025, 1, GETUTCDATE(), 8,  22000000, N'UI/UX, đoạt giải nhì Vietnam Web Awards 2024.',          'https://dribbble.com/linhpham'),
('660e8400-e29b-41d4-a716-446655440004', '560e8400-e29b-41d4-a716-446655440004', 'hoang.le@hcmut.edu.vn',    N'Đại học Bách Khoa TP.HCM',       N'Hệ thống Thông tin',        2026, 1, GETUTCDATE(), 3,  7500000,  N'Mobile app dev, đã release 2 app trên Play Store.',      'https://github.com/lehoang'),
('660e8400-e29b-41d4-a716-446655440005', '560e8400-e29b-41d4-a716-446655440005', 'thao.bui@neu.edu.vn',      N'Đại học Kinh tế Quốc dân',       N'Marketing',                 2025, 1, GETUTCDATE(), 9,  18500000, N'Content writing cho fintech, edtech, F&B.',              'https://thaobui.notion.site'),
('660e8400-e29b-41d4-a716-446655440006', '560e8400-e29b-41d4-a716-446655440006', 'minh.do@uet.vnu.edu.vn',   N'Trường ĐH Công nghệ - ĐHQGHN',    N'Công nghệ Thông tin',       2024, 1, GETUTCDATE(), 5,  14200000, N'Java Spring Boot, microservices.',                       'https://github.com/minhdo'),
('660e8400-e29b-41d4-a716-446655440007', '560e8400-e29b-41d4-a716-446655440007', 'an.vu@ftu.edu.vn',         N'Đại học Ngoại Thương',           N'Phân tích Dữ liệu',         2026, 0, NULL,         1,  1800000,  N'Data analyst tập sự, mạnh Excel + SQL + Power BI.',      NULL),
('660e8400-e29b-41d4-a716-446655440008', '560e8400-e29b-41d4-a716-446655440008', 'huy.dang@ueh.edu.vn',      N'Đại học Kinh tế TP.HCM',         N'Marketing',                 2025, 1, GETUTCDATE(), 7,  17000000, N'Digital marketing, chạy ads tổng ngân sách > 500tr.',    'https://huydang.com'),
('660e8400-e29b-41d4-a716-446655440009', '560e8400-e29b-41d4-a716-446655440009', 'ha.ngo@hcmuaf.edu.vn',     N'Đại học Mỹ thuật TP.HCM',         N'Đồ họa',                    2025, 1, GETUTCDATE(), 6,  13500000, N'Brand identity, đã làm 12 bộ nhận diện.',                'https://behance.net/hango'),
('660e8400-e29b-41d4-a716-446655440010', '560e8400-e29b-41d4-a716-446655440010', 'kien.ho@hust.edu.vn',      N'Đại học Bách Khoa Hà Nội',      N'Mạng máy tính',             2025, 1, GETUTCDATE(), 4,  11000000, N'DevOps, container & CI/CD.',                              'https://github.com/kienho'),
('660e8400-e29b-41d4-a716-446655440011', '560e8400-e29b-41d4-a716-446655440011', 'oanh.duong@uah.edu.vn',    N'Đại học Sân khấu Điện ảnh',       N'Điện ảnh - Truyền hình',     2024, 1, GETUTCDATE(), 10, 22500000, N'Video editor cho TVC, MV ngắn.',                          'https://vimeo.com/oanhduong'),
('660e8400-e29b-41d4-a716-446655440012', '560e8400-e29b-41d4-a716-446655440012', 'phong.ly@ulis.vnu.edu.vn', N'Trường ĐH Ngoại ngữ - ĐHQGHN',   N'Ngôn ngữ Anh',              2025, 0, NULL,         2,  3400000,  N'Dịch tài liệu IT, đã dịch 4 tài liệu kỹ thuật dày.',     NULL);
PRINT 'Inserted 12 student profiles';
GO

INSERT INTO [dbo].[BusinessProfiles] ([Id], [UserId], [CompanyName], [CompanyEmail], [CompanyWebsite], [CompanySize], [Industry], [IsVerified], [VerifiedAt], [CompletedProjects], [TotalSpent], [Rating], [Description], [Address])
VALUES
('760e8400-e29b-41d4-a716-446655440001', '560e8400-e29b-41d4-a716-446655440020', N'VietTech Solutions',  'cto@viettech.vn',     'https://viettech.vn',  'sme',      N'Fintech Software',       1, GETUTCDATE(), 35, 450000000, 4.7, N'Phát triển sản phẩm tài chính cho doanh nghiệp vừa và nhỏ.',  N'Cầu Giấy, Hà Nội'),
('760e8400-e29b-41d4-a716-446655440002', '560e8400-e29b-41d4-a716-446655440021', N'GreenFarm Vietnam',   'hr@greenfarm.vn',     'https://greenfarm.vn', 'startup',  N'AgriTech',               1, GETUTCDATE(), 12, 95000000,  4.5, N'Nền tảng kết nối nông dân và người tiêu dùng.',                N'Thủ Đức, TP.HCM'),
('760e8400-e29b-41d4-a716-446655440003', '560e8400-e29b-41d4-a716-446655440022', N'StudiBox EdTech',     'team@studibox.vn',    'https://studibox.vn',  'startup',  N'EdTech',                 1, GETUTCDATE(), 18, 165000000, 4.8, N'Học online cho sinh viên: video, quiz, mentor.',                N'Đống Đa, Hà Nội'),
('760e8400-e29b-41d4-a716-446655440004', '560e8400-e29b-41d4-a716-446655440023', N'CloudKit JSC',        'pm@cloudkit.vn',      'https://cloudkit.vn',  'sme',      N'Cloud Infrastructure',   1, GETUTCDATE(), 22, 285000000, 4.9, N'AWS & Azure consulting, DevOps.',                                N'Quận 1, TP.HCM'),
('760e8400-e29b-41d4-a716-446655440005', '560e8400-e29b-41d4-a716-446655440024', N'BrandLab Agency',     'hello@brandlab.vn',   'https://brandlab.vn',  'sme',      N'Branding & Design',      1, GETUTCDATE(), 28, 235000000, 4.6, N'Branding & creative agency cho startup VN.',                     N'Hoàn Kiếm, Hà Nội'),
('760e8400-e29b-41d4-a716-446655440006', '560e8400-e29b-41d4-a716-446655440025', N'MediaWave',           'contact@mediawave.vn','https://mediawave.vn', 'startup',  N'Video Production',       1, GETUTCDATE(), 15, 125000000, 4.6, N'TVC, MV, motion graphic.',                                        N'Bình Thạnh, TP.HCM'),
('760e8400-e29b-41d4-a716-446655440007', '560e8400-e29b-41d4-a716-446655440026', N'FinHub Vietnam',      'jobs@finhub.vn',      'https://finhub.vn',    'startup',  N'Fintech',                1, GETUTCDATE(), 9,  72000000,  4.4, N'Ví điện tử & ưu đãi cho sinh viên.',                              N'Quận 7, TP.HCM'),
('760e8400-e29b-41d4-a716-446655440008', '560e8400-e29b-41d4-a716-446655440027', N'NutriBite',           'careers@nutribite.vn','https://nutribite.vn', 'startup',  N'F&B / D2C',              1, GETUTCDATE(), 6,  48000000,  4.3, N'Thực phẩm healthy, kênh D2C TikTok Shop.',                       N'Cầu Giấy, Hà Nội');
PRINT 'Inserted 8 business profiles';
GO

INSERT INTO [dbo].[StudentWallets] ([Id], [StudentId], [Balance], [TotalEarned], [TotalWithdrawn])
VALUES
('b60e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 2500000, 9500000,  7000000),
('b60e8400-e29b-41d4-a716-446655440002', '660e8400-e29b-41d4-a716-446655440002', 1800000, 16800000, 15000000),
('b60e8400-e29b-41d4-a716-446655440003', '660e8400-e29b-41d4-a716-446655440003', 4500000, 22000000, 17500000),
('b60e8400-e29b-41d4-a716-446655440004', '660e8400-e29b-41d4-a716-446655440004', 2200000, 7500000,  5300000),
('b60e8400-e29b-41d4-a716-446655440005', '660e8400-e29b-41d4-a716-446655440005', 3500000, 18500000, 15000000),
('b60e8400-e29b-41d4-a716-446655440006', '660e8400-e29b-41d4-a716-446655440006', 2700000, 14200000, 11500000),
('b60e8400-e29b-41d4-a716-446655440007', '660e8400-e29b-41d4-a716-446655440007', 1800000, 1800000,  0),
('b60e8400-e29b-41d4-a716-446655440008', '660e8400-e29b-41d4-a716-446655440008', 5000000, 17000000, 12000000),
('b60e8400-e29b-41d4-a716-446655440009', '660e8400-e29b-41d4-a716-446655440009', 3500000, 13500000, 10000000),
('b60e8400-e29b-41d4-a716-446655440010', '660e8400-e29b-41d4-a716-446655440010', 2000000, 11000000, 9000000),
('b60e8400-e29b-41d4-a716-446655440011', '660e8400-e29b-41d4-a716-446655440011', 4500000, 22500000, 18000000),
('b60e8400-e29b-41d4-a716-446655440012', '660e8400-e29b-41d4-a716-446655440012', 1400000, 3400000,  2000000);
PRINT 'Inserted 12 wallets';
GO

INSERT INTO [dbo].[Skills] ([Id], [Name], [Category]) VALUES
('e60e8400-e29b-41d4-a716-446655440001', N'Vue.js',              N'Frontend'),
('e60e8400-e29b-41d4-a716-446655440002', N'Angular',             N'Frontend'),
('e60e8400-e29b-41d4-a716-446655440003', N'Next.js',             N'Frontend'),
('e60e8400-e29b-41d4-a716-446655440004', N'Tailwind CSS',        N'Frontend'),
('e60e8400-e29b-41d4-a716-446655440005', N'Java Spring Boot',    N'Backend'),
('e60e8400-e29b-41d4-a716-446655440006', N'Go (Golang)',         N'Backend'),
('e60e8400-e29b-41d4-a716-446655440007', N'PostgreSQL',          N'Database'),
('e60e8400-e29b-41d4-a716-446655440008', N'MongoDB',             N'Database'),
('e60e8400-e29b-41d4-a716-446655440009', N'Redis',               N'Database'),
('e60e8400-e29b-41d4-a716-446655440010', N'Docker',              N'DevOps'),
('e60e8400-e29b-41d4-a716-446655440011', N'Kubernetes',          N'DevOps'),
('e60e8400-e29b-41d4-a716-446655440012', N'AWS',                 N'Cloud'),
('e60e8400-e29b-41d4-a716-446655440013', N'Azure',               N'Cloud'),
('e60e8400-e29b-41d4-a716-446655440014', N'React Native',        N'Mobile'),
('e60e8400-e29b-41d4-a716-446655440015', N'Flutter',             N'Mobile'),
('e60e8400-e29b-41d4-a716-446655440016', N'Adobe Photoshop',     N'Design'),
('e60e8400-e29b-41d4-a716-446655440017', N'Adobe Illustrator',   N'Design'),
('e60e8400-e29b-41d4-a716-446655440018', N'Adobe Premiere',      N'Video'),
('e60e8400-e29b-41d4-a716-446655440019', N'DaVinci Resolve',     N'Video'),
('e60e8400-e29b-41d4-a716-446655440021', N'Google Ads',          N'Marketing'),
('e60e8400-e29b-41d4-a716-446655440022', N'SEO',                 N'Marketing');
PRINT 'Inserted 21 skills';
GO

INSERT INTO [dbo].[StudentSkills] ([Id], [StudentId], [SkillId], [Proficiency], [EndorsementCount]) VALUES
('f60e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'e60e8400-e29b-41d4-a716-446655440007', 'advanced',    3),
('f60e8400-e29b-41d4-a716-446655440002', '660e8400-e29b-41d4-a716-446655440001', 'e60e8400-e29b-41d4-a716-446655440008', 'intermediate', 1),
('f60e8400-e29b-41d4-a716-446655440003', '660e8400-e29b-41d4-a716-446655440002', 'e60e8400-e29b-41d4-a716-446655440003', 'advanced',    5),
('f60e8400-e29b-41d4-a716-446655440004', '660e8400-e29b-41d4-a716-446655440002', 'e60e8400-e29b-41d4-a716-446655440004', 'advanced',    4),
('f60e8400-e29b-41d4-a716-446655440005', '660e8400-e29b-41d4-a716-446655440002', 'e60e8400-e29b-41d4-a716-446655440008', 'intermediate', 2),
('f60e8400-e29b-41d4-a716-446655440006', '660e8400-e29b-41d4-a716-446655440003', 'e60e8400-e29b-41d4-a716-446655440016', 'advanced',    6),
('f60e8400-e29b-41d4-a716-446655440007', '660e8400-e29b-41d4-a716-446655440003', 'e60e8400-e29b-41d4-a716-446655440017', 'advanced',    5),
('f60e8400-e29b-41d4-a716-446655440008', '660e8400-e29b-41d4-a716-446655440004', 'e60e8400-e29b-41d4-a716-446655440014', 'advanced',    4),
('f60e8400-e29b-41d4-a716-446655440009', '660e8400-e29b-41d4-a716-446655440004', 'e60e8400-e29b-41d4-a716-446655440015', 'intermediate', 2),
('f60e8400-e29b-41d4-a716-446655440010', '660e8400-e29b-41d4-a716-446655440005', 'e60e8400-e29b-41d4-a716-446655440022', 'advanced',    7),
('f60e8400-e29b-41d4-a716-446655440011', '660e8400-e29b-41d4-a716-446655440006', 'e60e8400-e29b-41d4-a716-446655440005', 'advanced',    5),
('f60e8400-e29b-41d4-a716-446655440012', '660e8400-e29b-41d4-a716-446655440006', 'e60e8400-e29b-41d4-a716-446655440007', 'advanced',    4),
('f60e8400-e29b-41d4-a716-446655440013', '660e8400-e29b-41d4-a716-446655440006', 'e60e8400-e29b-41d4-a716-446655440010', 'intermediate', 2),
('f60e8400-e29b-41d4-a716-446655440015', '660e8400-e29b-41d4-a716-446655440008', 'e60e8400-e29b-41d4-a716-446655440021', 'advanced',    5),
('f60e8400-e29b-41d4-a716-446655440016', '660e8400-e29b-41d4-a716-446655440008', 'e60e8400-e29b-41d4-a716-446655440022', 'intermediate', 3),
('f60e8400-e29b-41d4-a716-446655440017', '660e8400-e29b-41d4-a716-446655440009', 'e60e8400-e29b-41d4-a716-446655440016', 'advanced',    5),
('f60e8400-e29b-41d4-a716-446655440018', '660e8400-e29b-41d4-a716-446655440009', 'e60e8400-e29b-41d4-a716-446655440017', 'advanced',    4),
('f60e8400-e29b-41d4-a716-446655440019', '660e8400-e29b-41d4-a716-446655440010', 'e60e8400-e29b-41d4-a716-446655440010', 'advanced',    4),
('f60e8400-e29b-41d4-a716-446655440020', '660e8400-e29b-41d4-a716-446655440010', 'e60e8400-e29b-41d4-a716-446655440011', 'intermediate', 2),
('f60e8400-e29b-41d4-a716-446655440021', '660e8400-e29b-41d4-a716-446655440010', 'e60e8400-e29b-41d4-a716-446655440012', 'intermediate', 3),
('f60e8400-e29b-41d4-a716-446655440022', '660e8400-e29b-41d4-a716-446655440011', 'e60e8400-e29b-41d4-a716-446655440018', 'advanced',    8),
('f60e8400-e29b-41d4-a716-446655440023', '660e8400-e29b-41d4-a716-446655440011', 'e60e8400-e29b-41d4-a716-446655440019', 'advanced',    5);
PRINT 'Inserted 23 student-skill links';
GO

DECLARE @cat_it    UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440001';
DECLARE @cat_des   UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440002';
DECLARE @cat_cnt   UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440003';
DECLARE @cat_mkt   UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440004';
DECLARE @cat_trans UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440005';
DECLARE @cat_video UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440006';

DECLARE @b_viettech UNIQUEIDENTIFIER = '760e8400-e29b-41d4-a716-446655440001';
DECLARE @b_green    UNIQUEIDENTIFIER = '760e8400-e29b-41d4-a716-446655440002';
DECLARE @b_studi    UNIQUEIDENTIFIER = '760e8400-e29b-41d4-a716-446655440003';
DECLARE @b_cloud    UNIQUEIDENTIFIER = '760e8400-e29b-41d4-a716-446655440004';
DECLARE @b_brand    UNIQUEIDENTIFIER = '760e8400-e29b-41d4-a716-446655440005';
DECLARE @b_media    UNIQUEIDENTIFIER = '760e8400-e29b-41d4-a716-446655440006';
DECLARE @b_finhub   UNIQUEIDENTIFIER = '760e8400-e29b-41d4-a716-446655440007';
DECLARE @b_nutri    UNIQUEIDENTIFIER = '760e8400-e29b-41d4-a716-446655440008';

INSERT INTO [dbo].[Jobs] ([Id], [BusinessId], [Title], [Description], [CategoryId], [TagsJson], [Status], [SalaryMin], [SalaryMax], [DurationType], [DurationDays], [RequiredSkillsJson], [ExperienceLevel], [SpotsTotal], [SpotsFilled], [Location], [IsRemote], [IsFeatured], [Deadline], [PublishedAt])
VALUES
('960e8400-e29b-41d4-a716-446655440001', @b_viettech, N'Next.js Developer cho dashboard nội bộ', N'Phát triển dashboard quản lý dùng Next.js 14 App Router + tRPC. Có sẵn UI design Figma. Yêu cầu kinh nghiệm React + TypeScript thực tế, biết tự deploy lên Vercel.', @cat_it, N'["Next.js","TypeScript","Vercel"]', 'open', 4000000, 7000000, 'project',  21, N'["Next.js","TypeScript","tRPC"]',  'intermediate', 2, 0, N'Hà Nội',         1, 1, DATEADD(day, 12, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440002', @b_studi,    N'Vue.js Engineer cho LMS', N'Refactor LMS từ Vue 2 sang Vue 3 + Pinia. Có codebase legacy ~30k dòng. Cần kỹ năng đọc code và viết test Vitest.', @cat_it, N'["Vue.js","Pinia","Vitest"]', 'open', 5000000, 8500000, 'project', 30, N'["Vue.js","TypeScript"]', 'advanced', 1, 0, N'Hà Nội',         1, 1, DATEADD(day, 20, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440003', @b_cloud,    N'DevOps Intern — CI/CD pipeline', N'Setup pipeline GitHub Actions cho 3 service .NET + 2 service Node. Học hỏi thực tế cùng senior.', @cat_it, N'["DevOps","CI/CD","GitHub Actions"]', 'open', 3000000, 5000000, 'short-term', 14, N'["Docker","GitHub Actions"]', 'beginner', 1, 0, N'Hồ Chí Minh',    1, 0, DATEADD(day, 7, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440004', @b_finhub,   N'Backend Java Spring Boot - Wallet API', N'Phát triển API ví điện tử: nạp/rút/chuyển. Phải hiểu transaction, idempotency, đối soát.', @cat_it, N'["Java","Spring Boot","PostgreSQL"]', 'open', 4500000, 7000000, 'project', 28, N'["Java","Spring Boot","PostgreSQL"]', 'intermediate', 2, 0, N'Hồ Chí Minh', 0, 1, DATEADD(day, 18, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440005', @b_green,    N'Mobile Developer React Native - app nông dân', N'App giúp nông dân ghi nhật ký mùa vụ, kết nối khách. Cần build + release lên CH Play.', @cat_it, N'["React Native","Mobile","Android"]', 'open', 3500000, 5500000, 'project', 25, N'["React Native","TypeScript"]', 'intermediate', 1, 0, N'TP.HCM', 1, 0, DATEADD(day, 15, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440006', @b_viettech, N'Fix bug Authentication JWT trong app .NET', N'App .NET 8 gặp lỗi token expired không refresh đúng. Cần debug và sửa trong 2 ngày.', @cat_it, N'["C#",".NET","JWT"]', 'open', 1500000, 2500000, 'micro', 2, N'["C#",".NET","JWT"]', 'advanced', 1, 0, N'Remote', 1, 0, DATEADD(day, 4, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440007', @b_studi,    N'Lập trình quiz engine + chấm điểm tự động', N'Build engine quiz: trắc nghiệm + tự luận. Lưu bài làm, hiển thị thống kê. React + .NET hoặc Node.', @cat_it, N'["Quiz","Backend","Frontend"]', 'open', 3500000, 5500000, 'short-term', 18, N'["React","Node.js"]', 'intermediate', 1, 0, N'Hà Nội', 1, 0, DATEADD(day, 10, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440008', @b_cloud,    N'Kubernetes consultant - giúp 1 buổi', N'Hỗ trợ team review setup k8s hiện tại, đề xuất tối ưu. 1 buổi online 2-3 giờ.', @cat_it, N'["Kubernetes","DevOps","Consulting"]', 'open', 800000, 1500000, 'micro', 1, N'["Kubernetes","Helm"]', 'advanced', 1, 0, N'Remote', 1, 0, DATEADD(day, 3, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440009', @b_nutri,    N'Tích hợp API TikTok Shop với hệ thống order', N'Webhook TikTok Shop -> CRM nội bộ. Đồng bộ order, inventory. Node.js + PostgreSQL.', @cat_it, N'["Node.js","API Integration","TikTok"]', 'open', 4000000, 6000000, 'project', 20, N'["Node.js","PostgreSQL","REST API"]', 'intermediate', 1, 0, N'Hà Nội', 1, 0, DATEADD(day, 14, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440010', @b_viettech, N'Data Engineer - ETL pipeline báo cáo', N'Build ETL pipeline đọc dữ liệu giao dịch từ Postgres + Redis, đẩy về data warehouse. Python + Airflow.', @cat_it, N'["Data","ETL","Airflow"]', 'open', 5500000, 8000000, 'project', 25, N'["Python","Airflow","SQL"]', 'advanced', 1, 0, N'Hà Nội', 1, 1, DATEADD(day, 16, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440011', @b_brand,    N'Brand identity cho café boutique', N'Logo + color + typography + brand book 12 trang cho thương hiệu cà phê đặc sản.', @cat_des, N'["Branding","Logo","Print"]', 'open', 2500000, 4500000, 'short-term', 10, N'["Adobe Illustrator","Figma"]', 'intermediate', 1, 0, N'Hà Nội', 1, 1, DATEADD(day, 9, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440012', @b_nutri,    N'Thiết kế bao bì sản phẩm healthy snack', N'5 SKU bao bì + nhãn dán. Cần file in CMYK + die-cut.', @cat_des, N'["Packaging","Print","CMYK"]', 'open', 3500000, 5500000, 'short-term', 12, N'["Adobe Illustrator","Photoshop"]', 'intermediate', 1, 0, N'Remote', 1, 0, DATEADD(day, 11, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440013', @b_studi,    N'Illustration set cho landing page edtech', N'15 illustration phong cách flat, theme học tập. Vector SVG.', @cat_des, N'["Illustration","Vector","Flat Design"]', 'open', 2000000, 3500000, 'short-term', 8, N'["Adobe Illustrator"]', 'intermediate', 2, 0, N'Remote', 1, 0, DATEADD(day, 7, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440014', @b_finhub,   N'UI/UX mobile app ví điện tử', N'Thiết kế lại flow nạp/rút/chuyển tiền + onboarding. Figma + design system 30 component.', @cat_des, N'["UI/UX","Mobile","Figma"]', 'open', 5000000, 8000000, 'project', 21, N'["Figma","UI Design","UX Design"]', 'advanced', 1, 0, N'TP.HCM', 1, 1, DATEADD(day, 14, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440015', @b_brand,    N'Poster A2 cho event ra mắt sản phẩm', N'2 poster A2 + 1 standee. Phong cách neon, năng động.', @cat_des, N'["Poster","Print","Event"]', 'open', 800000, 1500000, 'micro', 3, N'["Photoshop","Illustrator"]', 'intermediate', 1, 0, N'Hà Nội', 0, 0, DATEADD(day, 4, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440016', @b_green,    N'Infographic chuỗi cung ứng nông sản', N'5 infographic dạng dài dùng cho fanpage. Vector + responsive.', @cat_des, N'["Infographic","Social Media"]', 'open', 1500000, 2500000, 'short-term', 6, N'["Adobe Illustrator","Figma"]', 'intermediate', 1, 0, N'Remote', 1, 0, DATEADD(day, 6, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440017', @b_studi,    N'Viết 20 bài blog SEO chủ đề học tập', N'20 bài 1200-1500 từ, từ khóa do team SEO cung cấp. Có outline mẫu. Văn phong gần gũi với sinh viên.', @cat_cnt, N'["SEO","Blog","Education"]', 'open', 3000000, 5000000, 'project', 21, N'["SEO Writing","Copywriting"]', 'intermediate', 3, 0, N'Remote', 1, 1, DATEADD(day, 14, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440018', @b_nutri,    N'Copywriter cho 30 caption TikTok Shop', N'30 caption ngắn bán hàng cho 5 sản phẩm. Cần hook viral.', @cat_cnt, N'["Copywriting","TikTok","Social"]', 'open', 1500000, 2500000, 'short-term', 7, N'["Copywriting","TikTok"]', 'beginner', 2, 0, N'Remote', 1, 0, DATEADD(day, 5, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440019', @b_finhub,   N'Viết case study fintech (3 bài, 2000 từ)', N'3 case study về sản phẩm ví điện tử. Cần phỏng vấn 2 user qua call và viết storytelling.', @cat_cnt, N'["Case Study","Long-form","Fintech"]', 'open', 2500000, 4500000, 'short-term', 10, N'["Copywriting","Editorial"]', 'intermediate', 1, 0, N'TP.HCM', 1, 0, DATEADD(day, 12, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440020', @b_brand,    N'Newsletter editor — 4 số/tháng cho 2 tháng', N'Mỗi số ~600 từ về xu hướng branding. Tự pitch chủ đề, đề xuất tiêu đề.', @cat_cnt, N'["Newsletter","Editorial","Branding"]', 'open', 2000000, 3000000, 'project', 60, N'["Editorial","Newsletter"]', 'intermediate', 1, 0, N'Remote', 1, 0, DATEADD(day, 20, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440021', @b_studi,    N'Biên soạn 50 câu hỏi quiz Toán cao cấp', N'50 câu hỏi + đáp án + giải thích chi tiết. Đúng theo giáo trình DH Bách Khoa.', @cat_cnt, N'["Education","Math","Quiz"]', 'open', 1800000, 3000000, 'short-term', 10, N'["Editorial","Math"]', 'intermediate', 1, 0, N'Remote', 1, 0, DATEADD(day, 12, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440022', @b_nutri,    N'Chạy Facebook Ads cho sản phẩm mới — ngân sách 30tr', N'Setup campaign, audience, creative test. KPI: CPM < 80k, ROAS > 2.5.', @cat_mkt, N'["Facebook Ads","Performance"]', 'open', 3500000, 5500000, 'short-term', 15, N'["Facebook Ads","Analytics"]', 'intermediate', 1, 0, N'Hà Nội', 1, 1, DATEADD(day, 5, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440023', @b_studi,    N'SEO Content Strategist — 1 tháng', N'Audit website hiện tại, build content plan 30 bài, tối ưu on-page.', @cat_mkt, N'["SEO","Content Strategy"]', 'open', 4500000, 7500000, 'project', 30, N'["SEO","Ahrefs"]', 'intermediate', 1, 0, N'Hà Nội', 1, 0, DATEADD(day, 16, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440024', @b_green,    N'Influencer outreach — 20 micro-KOL', N'Liên hệ + thỏa thuận 20 micro-KOL Tiktok/IG. Cung cấp script + sản phẩm test.', @cat_mkt, N'["Influencer","Outreach"]', 'open', 2000000, 3500000, 'short-term', 10, N'["Outreach","Negotiation"]', 'beginner', 2, 0, N'Remote', 1, 0, DATEADD(day, 7, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440025', @b_finhub,   N'Email marketing — chuỗi 5 email onboarding', N'Viết + setup 5 email onboarding cho user mới. A/B test subject line.', @cat_mkt, N'["Email Marketing","CRM"]', 'open', 2500000, 4000000, 'short-term', 8, N'["Mailchimp","Copywriting"]', 'intermediate', 1, 0, N'Remote', 1, 0, DATEADD(day, 8, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440026', @b_brand,    N'Lên kế hoạch ra mắt sản phẩm — Go-to-Market', N'Build GTM plan 30 trang: target, channel, budget, timeline.', @cat_mkt, N'["GTM","Strategy"]', 'open', 4000000, 6500000, 'short-term', 14, N'["Strategy","Marketing"]', 'advanced', 1, 0, N'Hà Nội', 1, 1, DATEADD(day, 10, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440027', @b_viettech, N'Dịch tài liệu kỹ thuật — 50 trang EN-VI', N'Tài liệu kỹ thuật về hệ thống thanh toán. ~50 trang, 25k từ. Yêu cầu chính xác thuật ngữ.', @cat_trans, N'["Translation","Technical"]', 'open', 4000000, 7000000, 'project', 21, N'["English","Technical Writing"]', 'advanced', 1, 0, N'Remote', 1, 0, DATEADD(day, 15, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440028', @b_studi,    N'Việt hóa khóa học online — 8 bài giảng', N'Dịch transcript + subtitle 8 bài giảng (15-20 phút mỗi bài).', @cat_trans, N'["Translation","Subtitle","Edu"]', 'open', 2500000, 4500000, 'short-term', 12, N'["English","Subtitle"]', 'intermediate', 2, 0, N'Remote', 1, 0, DATEADD(day, 14, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440029', @b_brand,    N'Dịch website công ty — 25 trang', N'Dịch 25 trang web Anh-Việt + Việt-Anh. Văn phong corporate.', @cat_trans, N'["Translation","Web"]', 'open', 2000000, 3500000, 'short-term', 8, N'["English","Vietnamese"]', 'intermediate', 1, 0, N'Remote', 1, 0, DATEADD(day, 9, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440030', @b_media,    N'Edit 10 video TikTok cho thương hiệu F&B', N'10 video 30-60s. Có raw footage. Cần edit + nhạc + caption + thumbnail.', @cat_video, N'["Video","TikTok","Editing"]', 'open', 3500000, 5500000, 'short-term', 10, N'["Adobe Premiere","CapCut"]', 'intermediate', 1, 0, N'TP.HCM', 1, 1, DATEADD(day, 8, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440031', @b_media,    N'Motion graphic 30s — explainer fintech', N'Animation 30s giải thích flow chuyển tiền. Style 2D, flat color.', @cat_video, N'["Motion Graphic","After Effects"]', 'open', 4500000, 7500000, 'short-term', 12, N'["After Effects","Illustrator"]', 'advanced', 1, 0, N'Hà Nội', 1, 1, DATEADD(day, 12, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440032', @b_studi,    N'Quay + dựng 5 video bài giảng', N'Setup quay 5 bài giảng (20 phút mỗi bài) + edit. Có sẵn slide.', @cat_video, N'["Video","Education"]', 'open', 3000000, 5000000, 'short-term', 10, N'["Premiere","Lighting"]', 'intermediate', 1, 0, N'Hà Nội', 0, 0, DATEADD(day, 12, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440033', @b_green,    N'Drone footage cho video quảng cáo nông trại', N'Quay drone 1 ngày tại Lâm Đồng. Cung cấp raw 4K + edit highlight 60s.', @cat_video, N'["Drone","Footage","Edit"]', 'open', 4000000, 6500000, 'short-term', 4, N'["Drone","Premiere"]', 'advanced', 1, 0, N'Lâm Đồng', 0, 0, DATEADD(day, 6, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440034', @b_nutri,    N'Thumbnail YouTube — 30 ảnh', N'30 thumbnail style hấp dẫn, tỉ lệ CTR mục tiêu >8%. Có template tham khảo.', @cat_video, N'["Thumbnail","YouTube"]', 'open', 1500000, 2500000, 'micro', 5, N'["Photoshop"]', 'beginner', 2, 0, N'Remote', 1, 0, DATEADD(day, 5, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440035', @b_studi,    N'AI prompt engineer cho hệ thống chatbot', N'Tinh chỉnh prompt cho chatbot tutor. Đo accuracy, refusal rate. Familiar với Groq/OpenAI.', @cat_it, N'["AI","Prompt Engineering","LLM"]', 'open', 4500000, 7500000, 'short-term', 14, N'["LLM","Python"]', 'intermediate', 1, 0, N'Hà Nội', 1, 1, DATEADD(day, 11, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440036', @b_cloud,    N'Azure migration consultant — 2 buổi', N'Đánh giá hệ thống on-prem → Azure. Output: roadmap migration 3 tháng.', @cat_it, N'["Azure","Cloud","Consulting"]', 'open', 2500000, 4000000, 'micro', 2, N'["Azure"]', 'advanced', 1, 0, N'TP.HCM', 0, 0, DATEADD(day, 5, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440037', @b_finhub,   N'Security audit cho API ví — pentest cơ bản', N'OWASP top 10 cho API ví. Output: report + đề xuất fix.', @cat_it, N'["Security","Pentest","OWASP"]', 'open', 5000000, 8000000, 'short-term', 10, N'["Security","OWASP"]', 'advanced', 1, 0, N'Remote', 1, 1, DATEADD(day, 14, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440038', @b_studi,    N'TikTok creator — series 10 video tự học mỗi tuần 1 video', N'Quay 10 video education trên kênh chính thức. 60-90s mỗi video.', @cat_video, N'["TikTok","Education"]', 'open', 5000000, 8500000, 'project', 70, N'["TikTok","Video Editing"]', 'intermediate', 1, 0, N'Hà Nội', 1, 1, DATEADD(day, 14, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440039', @b_finhub,   N'Onboarding tutorial — viết script + quay', N'5 video tutorial 90s mỗi video hướng dẫn dùng ví. Script + voiceover + edit.', @cat_video, N'["Tutorial","Script","Voiceover"]', 'open', 3500000, 5500000, 'short-term', 10, N'["Premiere","Voiceover"]', 'intermediate', 1, 0, N'Hà Nội', 1, 0, DATEADD(day, 9, CAST(GETUTCDATE() AS DATE)), GETUTCDATE()),
('960e8400-e29b-41d4-a716-446655440040', @b_brand,    N'Photoshoot sản phẩm e-com — 30 ảnh', N'30 sản phẩm, mỗi sp 4 góc. Studio sẵn ở Hà Nội.', @cat_des, N'["Photography","Product","Studio"]', 'open', 3000000, 5000000, 'short-term', 4, N'["Photography","Retouching"]', 'intermediate', 1, 0, N'Hà Nội', 0, 0, DATEADD(day, 5, CAST(GETUTCDATE() AS DATE)), GETUTCDATE());
PRINT 'Inserted 40 jobs';
GO

INSERT INTO [dbo].[JobApplications] ([Id], [JobId], [StudentId], [Status], [CoverLetter], [ProposedTimeline], [AppliedAt]) VALUES
('a60e8400-e29b-41d4-a716-446655440001', '960e8400-e29b-41d4-a716-446655440010', '660e8400-e29b-41d4-a716-446655440001', 'pending',  N'Em đã làm 2 dự án ETL với Airflow ở trường, rất muốn được học hỏi thực tế.', N'25 ngày', DATEADD(day, -2, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440002', '960e8400-e29b-41d4-a716-446655440035', '660e8400-e29b-41d4-a716-446655440001', 'pending',  N'Em đã chơi nhiều với Groq, OpenAI API. Có thể demo prompt + eval framework.', N'14 ngày', DATEADD(day, -1, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440003', '960e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440002', 'accepted', N'Em đã làm 3 dự án Next.js production. Sẵn sàng đảm nhận.', N'21 ngày', DATEADD(day, -5, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440004', '960e8400-e29b-41d4-a716-446655440002', '660e8400-e29b-41d4-a716-446655440002', 'pending',  N'Em có 2 năm Vue 2 và đã upgrade 1 project sang Vue 3 + Pinia.', N'30 ngày', DATEADD(day, -3, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440005', '960e8400-e29b-41d4-a716-446655440014', '660e8400-e29b-41d4-a716-446655440003', 'accepted', N'Em có sample design system cho fintech, mời anh chị xem portfolio.', N'21 ngày', DATEADD(day, -4, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440006', '960e8400-e29b-41d4-a716-446655440011', '660e8400-e29b-41d4-a716-446655440003', 'pending',  N'Em rất yêu thích thương hiệu cà phê đặc sản và muốn được đóng góp.', N'10 ngày', DATEADD(day, -2, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440007', '960e8400-e29b-41d4-a716-446655440005', '660e8400-e29b-41d4-a716-446655440004', 'pending',  N'Em đã release 2 app trên Play Store, kinh nghiệm với React Native.', N'25 ngày', DATEADD(day, -3, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440008', '960e8400-e29b-41d4-a716-446655440017', '660e8400-e29b-41d4-a716-446655440005', 'accepted', N'Em chuyên viết SEO blog cho edtech, đã làm cho 3 thương hiệu tương tự.', N'21 ngày', DATEADD(day, -6, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440009', '960e8400-e29b-41d4-a716-446655440020', '660e8400-e29b-41d4-a716-446655440005', 'pending',  N'Em đã viết newsletter cho 2 brand branding, sample đính kèm.', N'60 ngày', DATEADD(day, -1, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440010', '960e8400-e29b-41d4-a716-446655440018', '660e8400-e29b-41d4-a716-446655440005', 'pending',  N'Em có 50+ caption TikTok đã viết trước đây.', N'7 ngày', DATEADD(day, -2, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440011', '960e8400-e29b-41d4-a716-446655440004', '660e8400-e29b-41d4-a716-446655440006', 'accepted', N'Em đã làm Spring Boot 2 năm, có exp với transaction & idempotency.', N'28 ngày', DATEADD(day, -7, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440012', '960e8400-e29b-41d4-a716-446655440022', '660e8400-e29b-41d4-a716-446655440008', 'pending',  N'Em đã chạy ads tổng > 500tr, target ROAS 3.0 cho F&B.', N'15 ngày', DATEADD(day, -3, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440013', '960e8400-e29b-41d4-a716-446655440023', '660e8400-e29b-41d4-a716-446655440008', 'pending',  N'Em audit SEO website cho 2 brand edtech, kèm checklist 50 mục.', N'30 ngày', DATEADD(day, -2, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440014', '960e8400-e29b-41d4-a716-446655440025', '660e8400-e29b-41d4-a716-446655440008', 'rejected', N'Em chuyên FB/Google ads, email marketing chưa chuyên sâu lắm.', N'8 ngày', DATEADD(day, -8, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440015', '960e8400-e29b-41d4-a716-446655440011', '660e8400-e29b-41d4-a716-446655440009', 'pending',  N'Em đã làm 12 brand identity, attached behance.', N'10 ngày', DATEADD(day, -1, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440016', '960e8400-e29b-41d4-a716-446655440012', '660e8400-e29b-41d4-a716-446655440009', 'accepted', N'Em đã thiết kế 5 bộ packaging FMCG, gửi sample.', N'12 ngày', DATEADD(day, -5, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440017', '960e8400-e29b-41d4-a716-446655440003', '660e8400-e29b-41d4-a716-446655440010', 'accepted', N'Em đã setup CI/CD GitHub Actions cho 4 service, học hỏi rất tốt.', N'14 ngày', DATEADD(day, -4, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440018', '960e8400-e29b-41d4-a716-446655440008', '660e8400-e29b-41d4-a716-446655440010', 'pending',  N'Em có exp k8s với 5 service production.', N'1 ngày', DATEADD(day, -1, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440019', '960e8400-e29b-41d4-a716-446655440030', '660e8400-e29b-41d4-a716-446655440011', 'accepted', N'Em đã edit 100+ video TikTok F&B, có 2 video lên top trending.', N'10 ngày', DATEADD(day, -6, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440020', '960e8400-e29b-41d4-a716-446655440031', '660e8400-e29b-41d4-a716-446655440011', 'pending',  N'Em làm motion graphic 2D, có 4 explainer fintech mẫu.', N'12 ngày', DATEADD(day, -2, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440021', '960e8400-e29b-41d4-a716-446655440027', '660e8400-e29b-41d4-a716-446655440012', 'pending',  N'Em đã dịch 4 tài liệu kỹ thuật dày, thuật ngữ IT thông thạo.', N'21 ngày', DATEADD(day, -3, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440022', '960e8400-e29b-41d4-a716-446655440028', '660e8400-e29b-41d4-a716-446655440012', 'accepted', N'Em đã dịch subtitle 30+ video education trên Coursera.', N'12 ngày', DATEADD(day, -5, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440023', '960e8400-e29b-41d4-a716-446655440010', '660e8400-e29b-41d4-a716-446655440007', 'rejected', N'Em mới bắt đầu, SQL tốt nhưng Airflow chưa có exp.', N'25 ngày', DATEADD(day, -10, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440024', '960e8400-e29b-41d4-a716-446655440001', '650e8400-e29b-41d4-a716-446655440001', 'pending',  N'Em đã từng hoàn thành job React tương tự cho TechNova trước đây.', N'21 ngày', DATEADD(day, -1, GETUTCDATE())),
('a60e8400-e29b-41d4-a716-446655440025', '960e8400-e29b-41d4-a716-446655440017', '650e8400-e29b-41d4-a716-446655440003', 'pending',  N'Em đã hoàn thành 3 dự án content SEO cho MarketHub, sẵn sàng nhận thêm.', N'21 ngày', DATEADD(day, -2, GETUTCDATE()));
PRINT 'Inserted 25 applications';
GO

INSERT INTO [dbo].[Reviews] ([Id], [JobId], [JobApplicationId], [FromUserId], [ToUserId], [Rating], [Comment], [SkillEndorsementsJson]) VALUES
('d60e8400-e29b-41d4-a716-446655440001', '960e8400-e29b-41d4-a716-446655440001', 'a60e8400-e29b-41d4-a716-446655440003', '560e8400-e29b-41d4-a716-446655440020', '560e8400-e29b-41d4-a716-446655440002', 5, N'Đức làm việc rất chuyên nghiệp, code clean, deadline đúng giờ. Recommend!', N'["Next.js","TypeScript","Communication"]'),
('d60e8400-e29b-41d4-a716-446655440002', '960e8400-e29b-41d4-a716-446655440014', 'a60e8400-e29b-41d4-a716-446655440005', '560e8400-e29b-41d4-a716-446655440026', '560e8400-e29b-41d4-a716-446655440003', 5, N'Linh có gout thiết kế rất tốt, đề xuất hợp lý và nhanh nhạy với feedback.', N'["Figma","UI Design","Creativity"]'),
('d60e8400-e29b-41d4-a716-446655440003', '960e8400-e29b-41d4-a716-446655440017', 'a60e8400-e29b-41d4-a716-446655440008', '560e8400-e29b-41d4-a716-446655440022', '560e8400-e29b-41d4-a716-446655440005', 5, N'Thảo viết tự nhiên, đúng tone-of-voice, không cần edit nhiều.', N'["SEO Writing","Editorial"]'),
('d60e8400-e29b-41d4-a716-446655440004', '960e8400-e29b-41d4-a716-446655440004', 'a60e8400-e29b-41d4-a716-446655440011', '560e8400-e29b-41d4-a716-446655440026', '560e8400-e29b-41d4-a716-446655440006', 5, N'Minh hiểu transaction sâu, code Spring Boot chất lượng.', N'["Java","Spring Boot","System Design"]'),
('d60e8400-e29b-41d4-a716-446655440005', '960e8400-e29b-41d4-a716-446655440012', 'a60e8400-e29b-41d4-a716-446655440016', '560e8400-e29b-41d4-a716-446655440027', '560e8400-e29b-41d4-a716-446655440009', 4, N'Hà thiết kế nhanh, file đúng yêu cầu in.', N'["Packaging Design","Print"]'),
('d60e8400-e29b-41d4-a716-446655440006', '960e8400-e29b-41d4-a716-446655440003', 'a60e8400-e29b-41d4-a716-446655440017', '560e8400-e29b-41d4-a716-446655440023', '560e8400-e29b-41d4-a716-446655440010', 4, N'Kiên học nhanh, ham hỏi, pipeline chạy ngon.', N'["DevOps","CI/CD"]'),
('d60e8400-e29b-41d4-a716-446655440007', '960e8400-e29b-41d4-a716-446655440030', 'a60e8400-e29b-41d4-a716-446655440019', '560e8400-e29b-41d4-a716-446655440025', '560e8400-e29b-41d4-a716-446655440011', 5, N'Oanh edit cực có gu, hiểu thuật toán TikTok. 2/10 video lên top.', N'["Video Editing","TikTok"]'),
('d60e8400-e29b-41d4-a716-446655440008', '960e8400-e29b-41d4-a716-446655440028', 'a60e8400-e29b-41d4-a716-446655440022', '560e8400-e29b-41d4-a716-446655440022', '560e8400-e29b-41d4-a716-446655440012', 5, N'Phong dịch chính xác, hiểu ngữ cảnh edu.', N'["Translation","English"]'),
('d60e8400-e29b-41d4-a716-446655440009', '960e8400-e29b-41d4-a716-446655440001', 'a60e8400-e29b-41d4-a716-446655440003', '560e8400-e29b-41d4-a716-446655440002', '560e8400-e29b-41d4-a716-446655440020', 5, N'VietTech onboarding rõ ràng, milestone hợp lý, payment đúng hạn.', N'["Onboarding","Payment"]'),
('d60e8400-e29b-41d4-a716-446655440010', '960e8400-e29b-41d4-a716-446655440014', 'a60e8400-e29b-41d4-a716-446655440005', '560e8400-e29b-41d4-a716-446655440003', '560e8400-e29b-41d4-a716-446655440026', 5, N'FinHub support fast, feedback rất chuyên môn.', N'["Communication","Support"]'),
('d60e8400-e29b-41d4-a716-446655440011', '960e8400-e29b-41d4-a716-446655440017', 'a60e8400-e29b-41d4-a716-446655440008', '560e8400-e29b-41d4-a716-446655440005', '560e8400-e29b-41d4-a716-446655440022', 5, N'StudiBox brief chi tiết, không cần hỏi đi hỏi lại.', N'["Brief","Clarity"]'),
('d60e8400-e29b-41d4-a716-446655440012', '960e8400-e29b-41d4-a716-446655440004', 'a60e8400-e29b-41d4-a716-446655440011', '560e8400-e29b-41d4-a716-446655440006', '560e8400-e29b-41d4-a716-446655440026', 5, N'FinHub team kỹ thuật mạnh, học được nhiều thực chiến.', N'["Mentoring","Tech"]'),
('d60e8400-e29b-41d4-a716-446655440013', '960e8400-e29b-41d4-a716-446655440003', 'a60e8400-e29b-41d4-a716-446655440017', '560e8400-e29b-41d4-a716-446655440010', '560e8400-e29b-41d4-a716-446655440023', 4, N'CloudKit senior tận tâm chỉ dạy, ngoài giờ vẫn trả lời câu hỏi.', N'["Mentoring"]'),
('d60e8400-e29b-41d4-a716-446655440014', '960e8400-e29b-41d4-a716-446655440030', 'a60e8400-e29b-41d4-a716-446655440019', '560e8400-e29b-41d4-a716-446655440011', '560e8400-e29b-41d4-a716-446655440025', 5, N'MediaWave cung cấp footage chất lượng, brief sáng.', N'["Asset","Brief"]'),
('d60e8400-e29b-41d4-a716-446655440015', '960e8400-e29b-41d4-a716-446655440028', 'a60e8400-e29b-41d4-a716-446655440022', '560e8400-e29b-41d4-a716-446655440012', '560e8400-e29b-41d4-a716-446655440022', 5, N'StudiBox flexible deadline, payment chính xác.', N'["Trust"]');
PRINT 'Inserted 15 reviews';
GO

INSERT INTO [dbo].[Payments] ([Id], [JobId], [JobApplicationId], [BusinessId], [StudentId], [Amount], [Status], [PaymentMethod], [ReleasedAt]) VALUES
('c60e8400-e29b-41d4-a716-446655440001', '960e8400-e29b-41d4-a716-446655440001', 'a60e8400-e29b-41d4-a716-446655440003', '760e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440002', 5500000, 'released', 'bank_transfer', DATEADD(day, -3, GETUTCDATE())),
('c60e8400-e29b-41d4-a716-446655440002', '960e8400-e29b-41d4-a716-446655440014', 'a60e8400-e29b-41d4-a716-446655440005', '760e8400-e29b-41d4-a716-446655440007', '660e8400-e29b-41d4-a716-446655440003', 6500000, 'released', 'bank_transfer', DATEADD(day, -2, GETUTCDATE())),
('c60e8400-e29b-41d4-a716-446655440003', '960e8400-e29b-41d4-a716-446655440017', 'a60e8400-e29b-41d4-a716-446655440008', '760e8400-e29b-41d4-a716-446655440003', '660e8400-e29b-41d4-a716-446655440005', 4000000, 'released', 'bank_transfer', DATEADD(day, -5, GETUTCDATE())),
('c60e8400-e29b-41d4-a716-446655440004', '960e8400-e29b-41d4-a716-446655440004', 'a60e8400-e29b-41d4-a716-446655440011', '760e8400-e29b-41d4-a716-446655440007', '660e8400-e29b-41d4-a716-446655440006', 6000000, 'escrow',   'bank_transfer', NULL),
('c60e8400-e29b-41d4-a716-446655440005', '960e8400-e29b-41d4-a716-446655440012', 'a60e8400-e29b-41d4-a716-446655440016', '760e8400-e29b-41d4-a716-446655440008', '660e8400-e29b-41d4-a716-446655440009', 4500000, 'released', 'bank_transfer', DATEADD(day, -4, GETUTCDATE())),
('c60e8400-e29b-41d4-a716-446655440006', '960e8400-e29b-41d4-a716-446655440003', 'a60e8400-e29b-41d4-a716-446655440017', '760e8400-e29b-41d4-a716-446655440004', '660e8400-e29b-41d4-a716-446655440010', 4000000, 'released', 'bank_transfer', DATEADD(day, -3, GETUTCDATE())),
('c60e8400-e29b-41d4-a716-446655440007', '960e8400-e29b-41d4-a716-446655440030', 'a60e8400-e29b-41d4-a716-446655440019', '760e8400-e29b-41d4-a716-446655440006', '660e8400-e29b-41d4-a716-446655440011', 4500000, 'released', 'bank_transfer', DATEADD(day, -5, GETUTCDATE())),
('c60e8400-e29b-41d4-a716-446655440008', '960e8400-e29b-41d4-a716-446655440028', 'a60e8400-e29b-41d4-a716-446655440022', '760e8400-e29b-41d4-a716-446655440003', '660e8400-e29b-41d4-a716-446655440012', 3500000, 'released', 'bank_transfer', DATEADD(day, -4, GETUTCDATE())),
('c60e8400-e29b-41d4-a716-446655440009', '960e8400-e29b-41d4-a716-446655440014', 'a60e8400-e29b-41d4-a716-446655440005', '760e8400-e29b-41d4-a716-446655440007', '660e8400-e29b-41d4-a716-446655440003', 1500000, 'escrow',   'bank_transfer', NULL),
('c60e8400-e29b-41d4-a716-446655440010', '960e8400-e29b-41d4-a716-446655440004', 'a60e8400-e29b-41d4-a716-446655440011', '760e8400-e29b-41d4-a716-446655440007', '660e8400-e29b-41d4-a716-446655440006', 1000000, 'pending',  'bank_transfer', NULL);
PRINT 'Inserted 10 payments';
GO

INSERT INTO [dbo].[FAQs] ([Id], [Question], [Answer], [Category], [ViewCount], [OrderIndex]) VALUES
('a70e8400-e29b-41d4-a716-446655440001', N'UniTask là gì?', N'UniTask là nền tảng kết nối sinh viên Việt Nam với doanh nghiệp thông qua các micro-job và dự án ngắn hạn. Sinh viên tích lũy kinh nghiệm thực tế và thu nhập, doanh nghiệp tiếp cận nguồn nhân lực trẻ với chi phí hợp lý.', N'general', 245, 1),
('a70e8400-e29b-41d4-a716-446655440002', N'Sinh viên đăng ký có mất phí không?', N'Hoàn toàn miễn phí. Sinh viên không bị tính phí ứng tuyển hay phí thành viên. Nền tảng chỉ thu phí 10% từ doanh nghiệp khi giao dịch thành công.', N'pricing', 180, 2),
('a70e8400-e29b-41d4-a716-446655440003', N'Hệ thống Escrow bảo vệ thanh toán thế nào?', N'Khi doanh nghiệp tạo payment, tiền được giữ ở Escrow (ví trung gian). Sinh viên hoàn thành job, doanh nghiệp xác nhận thì tiền mới được giải phóng về ví sinh viên.', N'payment', 320, 3),
('a70e8400-e29b-41d4-a716-446655440004', N'Tôi cần kinh nghiệm trước đó để ứng tuyển không?', N'Không bắt buộc. Nhiều job ở mức beginner, đặc biệt phù hợp sinh viên năm 2-3. Hãy viết Cover Letter trung thực về kỹ năng và sự sẵn sàng học hỏi.', N'application', 410, 4),
('a70e8400-e29b-41d4-a716-446655440005', N'Làm thế nào để xác thực sinh viên?', N'Bạn cần upload thẻ sinh viên và sử dụng email có đuôi .edu.vn hoặc tên trường. Sau khi admin duyệt, hồ sơ sẽ có dấu tích xanh.', N'verification', 290, 5),
('a70e8400-e29b-41d4-a716-446655440006', N'Khi nào tôi rút được tiền từ ví?', N'Khi ví có số dư > 100.000 đ, bạn có thể yêu cầu rút. Thời gian xử lý 3-5 ngày làm việc qua chuyển khoản ngân hàng. Phí rút 0 đồng.', N'payment', 215, 6),
('a70e8400-e29b-41d4-a716-446655440007', N'Smart Matching hoạt động ra sao?', N'Hệ thống AI phân tích hồ sơ (ngành học, kỹ năng, dự án đã làm) và đối chiếu với yêu cầu job để cho điểm phù hợp. Job có điểm cao hiển thị trước.', N'ai', 175, 7),
('a70e8400-e29b-41d4-a716-446655440008', N'Tôi có thể nhận nhiều job cùng lúc không?', N'Có, miễn là bạn đảm bảo deadline. Khuyến nghị tối đa 2 job đồng thời cho sinh viên năm 2-3, 3-4 job cho sinh viên năm cuối có kinh nghiệm.', N'application', 130, 8),
('a70e8400-e29b-41d4-a716-446655440009', N'Doanh nghiệp xác thực thế nào?', N'Doanh nghiệp cần cung cấp giấy phép kinh doanh và email tên miền công ty. Sau duyệt, hồ sơ sẽ có dấu tích xanh.', N'verification', 95, 9),
('a70e8400-e29b-41d4-a716-446655440010', N'Nếu doanh nghiệp không trả tiền sau khi tôi hoàn thành?', N'Báo cáo ngay qua mục Hỗ trợ. Vì tiền đã được giữ ở Escrow, admin sẽ giải quyết tranh chấp và giải ngân nếu sinh viên có bằng chứng hoàn thành.', N'payment', 145, 10),
('a70e8400-e29b-41d4-a716-446655440011', N'Tôi nên báo giá thế nào trong Cover Letter?', N'Doanh nghiệp đã có range. Bạn có thể đề xuất mức trong range + lý do (kinh nghiệm, độ phức tạp). Đừng đoán quá thấp để được chọn — chất lượng thường quan trọng hơn giá.', N'application', 160, 11),
('a70e8400-e29b-41d4-a716-446655440012', N'AI matching khác AI Trợ lý nghề nghiệp thế nào?', N'AI Matching tự động gợi ý job cho bạn dựa trên hồ sơ. AI Trợ lý nghề nghiệp là chatbot bạn có thể chat trực tiếp để hỏi: cách build CV, định hướng ngành, học gì tiếp theo.', N'ai', 88, 12);
PRINT 'Inserted 12 FAQs';
GO

INSERT INTO [dbo].[BlogPosts] ([Id], [Title], [Slug], [Content], [Excerpt], [Category], [AuthorId], [Status], [PublishedAt], [ViewCount], [TagsJson]) VALUES
('b70e8400-e29b-41d4-a716-446655440001', N'5 kỹ năng IT đáng học nhất 2026', 'ky-nang-it-2026', N'Năm 2026 thị trường IT Việt Nam đang shift theo hướng AI và cloud. Top 5 kỹ năng được trả cao nhất: (1) Prompt engineering cho LLM, (2) Cloud architecture trên AWS/Azure, (3) Next.js + TypeScript cho fullstack, (4) Data engineering với Python + Airflow, (5) Security audit (OWASP).', N'Top 5 kỹ năng IT đáng học, kèm lộ trình 3 tháng cho mỗi kỹ năng.', N'career', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -30, GETUTCDATE()), 1240, N'["IT","Career","2026"]'),
('b70e8400-e29b-41d4-a716-446655440002', N'Cách build CV ấn tượng khi chưa có kinh nghiệm full-time', 'cv-an-tuong-sinh-vien', N'4 cách bổ sung kinh nghiệm: (1) Làm micro-job trên UniTask, (2) Open source contribution, (3) Build personal project deploy lên Vercel/Render, (4) Tham gia hackathon. Nhớ ghi LINK demo + GitHub trên CV.', N'4 cách bổ sung kinh nghiệm thực tế cho CV sinh viên.', N'cv', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -25, GETUTCDATE()), 980, N'["CV","Sinh Viên","Tips"]'),
('b70e8400-e29b-41d4-a716-446655440003', N'Roadmap UI/UX Designer 2026: từ zero đến junior', 'roadmap-uiux-2026', N'Lộ trình 6 tháng: tháng 1 — design fundamentals, tháng 2 — Figma + design system, tháng 3 — copy work của top design, tháng 4 — redesign 3 app phổ biến VN, tháng 5 — UX research và usability test, tháng 6 — đóng gói portfolio.', N'Lộ trình 6 tháng để thành junior UI/UX.', N'career', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -20, GETUTCDATE()), 760, N'["UI/UX","Design","Roadmap"]'),
('b70e8400-e29b-41d4-a716-446655440004', N'Marketing Digital — học gì để được trả 5-10tr/tháng?', 'marketing-digital-sinh-vien', N'3 kỹ năng cốt lõi: (1) Facebook Ads — học qua Meta Blueprint miễn phí, (2) SEO content — viết 10 bài SEO cho website cá nhân, (3) Email marketing — practice trên Mailchimp free tier. Mỗi kỹ năng cần 1-2 tháng.', N'3 kỹ năng marketing digital học nhanh, có thu nhập sớm.', N'career', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -15, GETUTCDATE()), 845, N'["Marketing","Digital"]'),
('b70e8400-e29b-41d4-a716-446655440005', N'Cách viết Cover Letter trúng ý doanh nghiệp', 'viet-cover-letter', N'5 bước: (1) Đọc kỹ JD, gạch chân 3 yêu cầu cốt lõi, (2) Mở đầu nhắc cụ thể company name + job title, (3) Ghi 2-3 dự án match với 3 yêu cầu cốt lõi, (4) Đính kèm LINK demo/portfolio, (5) Đóng bằng câu cam kết về timeline.', N'5 bước viết Cover Letter chinh phục.', N'application', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -10, GETUTCDATE()), 1120, N'["Cover Letter","Tips"]'),
('b70e8400-e29b-41d4-a716-446655440006', N'Sinh viên năm 1-2 nên ưu tiên job nào?', 'sinh-vien-nam-1-2', N'Ưu tiên: (a) micro-job 1-3 ngày để học cách brief và deliver, (b) job có experience level beginner, (c) job dạng nội dung/translation/data entry để rèn nguyên tắc làm việc, (d) job kèm mentor.', N'Lời khuyên cho sinh viên năm 1-2 lần đầu tham gia.', N'tips', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -7, GETUTCDATE()), 540, N'["Sinh Viên","Beginner"]'),
('b70e8400-e29b-41d4-a716-446655440007', N'Khi nào nên upgrade từ Frontend sang Fullstack?', 'frontend-to-fullstack', N'Khi đã hoàn thành 3-5 job frontend và cảm thấy trần kỹ năng, hoặc muốn build product cá nhân end-to-end. Lộ trình 3 tháng: Node.js + Express → PostgreSQL → REST API design → deploy → đóng portfolio fullstack.', N'Dấu hiệu nên học backend và lộ trình 3 tháng.', N'career', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -5, GETUTCDATE()), 620, N'["Frontend","Fullstack","Career"]'),
('b70e8400-e29b-41d4-a716-446655440008', N'Cách quản lý nhiều job song song mà không burnout', 'quan-ly-nhieu-job', N'4 nguyên tắc: (1) Tối đa 2 dự án deep work cùng lúc, (2) Block time theo dự án, không đan xen, (3) Buffer time — báo deadline cộng thêm 20%, (4) Nghỉ 1 ngày/tuần hoàn toàn không động đến laptop.', N'4 nguyên tắc quản lý đa nhiệm freelance không burnout.', N'tips', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -3, GETUTCDATE()), 480, N'["Productivity","Burnout"]'),
('b70e8400-e29b-41d4-a716-446655440009', N'AI sẽ thay thế công việc IT của sinh viên?', 'ai-thay-the-cong-viec', N'AI hiện tại thay được: code boilerplate, viết test cơ bản, dịch thuật phổ thông. AI CHƯA thay được: hiểu requirement mơ hồ, debug system phức tạp, communicate với khách hàng, sáng tạo concept brand.', N'AI thay được phần nào, chưa thay được phần nào.', N'career', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -2, GETUTCDATE()), 1530, N'["AI","Career","Future"]'),
('b70e8400-e29b-41d4-a716-446655440010', N'Hồ sơ Doanh nghiệp tốt thu hút sinh viên xuất sắc thế nào?', 'ho-so-doanh-nghiep', N'5 yếu tố: (1) JD chi tiết (>200 từ), (2) Mức lương minh bạch (min-max), (3) Phản hồi đơn ứng tuyển trong 48h, (4) Payment đúng hạn (escrow released < 3 ngày), (5) Cho review 2 chiều.', N'5 yếu tố giúp doanh nghiệp build hồ sơ uy tín.', N'business', '550e8400-e29b-41d4-a716-446655440010', 'published', DATEADD(day, -1, GETUTCDATE()), 320, N'["Business","Tips"]');
PRINT 'Inserted 10 blog posts';
GO

COMMIT TRANSACTION;
PRINT '========================================';
PRINT 'Extended Seed Inserted Successfully';
PRINT '========================================';
