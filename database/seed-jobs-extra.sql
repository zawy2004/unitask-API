-- =====================================================================
-- SEED THÊM JOB THỰC TẾ (Giai đoạn 4) — deadline động +90 ngày.
-- An toàn & idempotent: chỉ chèn khi chưa tồn tại (kiểm theo Title mốc).
-- Tham chiếu BusinessProfiles & JobCategories CÓ THẬT trong DB.
--
-- Cách chạy:
--   sqlcmd -S taskuni.database.windows.net -U admintask -P <pass> -d UniDb -i database/seed-jobs-extra.sql
--   hoặc trên EC2:  docker run --rm -v $PWD:/sql mcr.microsoft.com/mssql-tools \
--     /opt/mssql-tools/bin/sqlcmd -S <server> -U <user> -P <pass> -d UniDb -i /sql/database/seed-jobs-extra.sql
-- =====================================================================

SET NOCOUNT ON;

DECLARE @deadline DATETIME2 = DATEADD(DAY, 90, GETUTCDATE());
DECLARE @now DATETIME2 = GETUTCDATE();

-- Category IDs (ổn định theo seed gốc)
DECLARE @cat_it   UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440001'; -- IT & Lập Trình
DECLARE @cat_des  UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440002'; -- Thiết Kế
DECLARE @cat_cont UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440003'; -- Nội Dung
DECLARE @cat_mkt  UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440004'; -- Marketing
DECLARE @cat_tran UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440005'; -- Dịch thuật
DECLARE @cat_vid  UNIQUEIDENTIFIER = '850e8400-e29b-41d4-a716-446655440006'; -- Video

-- Business IDs (ổn định theo seed gốc)
DECLARE @biz1 UNIQUEIDENTIFIER = '750e8400-e29b-41d4-a716-446655440001';
DECLARE @biz2 UNIQUEIDENTIFIER = '750e8400-e29b-41d4-a716-446655440002';
DECLARE @biz3 UNIQUEIDENTIFIER = '750e8400-e29b-41d4-a716-446655440003';
DECLARE @biz4 UNIQUEIDENTIFIER = '750e8400-e29b-41d4-a716-446655440004';

IF NOT EXISTS (SELECT 1 FROM dbo.Jobs WHERE Title = N'Lập trình viên Backend Java Spring Boot (Intern)')
BEGIN
    INSERT INTO dbo.Jobs
        (Id, BusinessId, CategoryId, Title, [Description], TagsJson, Status, SalaryMin, SalaryMax, Currency,
         DurationType, DurationDays, RequiredSkillsJson, ExperienceLevel, SpotsTotal, SpotsFilled, Location, IsRemote, IsFeatured, Deadline, CreatedAt, UpdatedAt, PublishedAt)
    VALUES
    (NEWID(), @biz1, @cat_it, N'Lập trình viên Backend Java Spring Boot (Intern)',
     N'Tham gia phát triển REST API cho hệ thống nội bộ bằng Java + Spring Boot. Được mentor review code, làm quen quy trình Git flow và CI/CD cơ bản.',
     N'["IT","Remote","Intern"]', 'open', 3000000, 5000000, 'VND', 'project', 30,
     N'["Java","Spring Boot","REST API","Git"]', 'beginner', 2, 0, N'Remote', 1, 1, @deadline, @now, @now, @now),

    (NEWID(), @biz1, @cat_it, N'Kiểm thử phần mềm (Manual QA / Tester)',
     N'Viết test case, thực hiện kiểm thử chức năng cho ứng dụng web, báo cáo bug trên Jira. Phù hợp sinh viên CNTT muốn theo hướng QA.',
     N'["IT","QA","Remote"]', 'open', 2000000, 3500000, 'VND', 'short-term', 21,
     N'["Testing","Test Case","Jira","Tư duy logic"]', 'beginner', 3, 0, N'Hồ Chí Minh', 0, 0, @deadline, @now, @now, @now),

    (NEWID(), @biz3, @cat_it, N'Phát triển ứng dụng Mobile Flutter',
     N'Xây dựng 5-6 màn hình cho app đặt lịch bằng Flutter, kết nối API có sẵn. Có thiết kế Figma đầy đủ.',
     N'["IT","Flutter","Mobile"]', 'open', 4000000, 7000000, 'VND', 'project', 45,
     N'["Flutter","Dart","REST API","State Management"]', 'intermediate', 1, 0, N'Remote', 1, 1, @deadline, @now, @now, @now),

    (NEWID(), @biz1, @cat_it, N'Nhập & Phân tích dữ liệu Excel / Google Sheets',
     N'Làm sạch, nhập liệu và dựng báo cáo pivot/biểu đồ từ dữ liệu bán hàng. Yêu cầu cẩn thận, thành thạo hàm Excel.',
     N'["Data","Excel","Remote"]', 'open', 1500000, 2500000, 'VND', 'short-term', 14,
     N'["Excel","Google Sheets","Pivot Table","Phân tích dữ liệu"]', 'beginner', 3, 0, N'Remote', 1, 0, @deadline, @now, @now, @now),

    (NEWID(), @biz2, @cat_des, N'Thiết kế Banner & Ấn phẩm Mạng xã hội',
     N'Thiết kế bộ 15 ấn phẩm (post, story, cover) cho fanpage thương hiệu thời trang theo brand guideline có sẵn.',
     N'["Thiết kế","Canva","Remote"]', 'open', 2000000, 3500000, 'VND', 'short-term', 20,
     N'["Photoshop","Canva","Bố cục","Màu sắc"]', 'beginner', 2, 0, N'Remote', 1, 0, @deadline, @now, @now, @now),

    (NEWID(), @biz2, @cat_des, N'Vẽ Minh hoạ (Illustration) cho Fanpage',
     N'Vẽ 8 illustration phong cách flat cho chuỗi bài viết giáo dục. Nộp file AI/PNG độ phân giải cao.',
     N'["Thiết kế","Illustration"]', 'open', 2500000, 4000000, 'VND', 'project', 25,
     N'["Illustrator","Vẽ minh hoạ","Sáng tạo"]', 'intermediate', 1, 0, N'Hà Nội', 0, 0, @deadline, @now, @now, @now),

    (NEWID(), @biz3, @cat_cont, N'Viết Content TikTok / Reels ngành F&B',
     N'Lên kịch bản 20 video ngắn (hook + nội dung + CTA) cho quán cà phê. Hiểu trend, viết lời thoại tự nhiên.',
     N'["Content","TikTok","Remote"]', 'open', 1800000, 3000000, 'VND', 'short-term', 20,
     N'["Copywriting","Kịch bản","TikTok","F&B"]', 'beginner', 2, 0, N'Remote', 1, 1, @deadline, @now, @now, @now),

    (NEWID(), @biz3, @cat_cont, N'Biên tập & Sửa lỗi bài viết (Proofreading)',
     N'Rà soát chính tả, ngữ pháp, văn phong cho 30 bài blog trước khi xuất bản. Yêu cầu tiếng Việt tốt.',
     N'["Content","Biên tập","Remote"]', 'open', 1200000, 2000000, 'VND', 'short-term', 15,
     N'["Biên tập","Chính tả","Tiếng Việt"]', 'beginner', 2, 0, N'Remote', 1, 0, @deadline, @now, @now, @now),

    (NEWID(), @biz4, @cat_mkt, N'Cộng tác viên SEO Off-page & Xây Backlink',
     N'Nghiên cứu từ khoá, đi backlink chất lượng, guest post cho website thương mại điện tử. Báo cáo tuần.',
     N'["Marketing","SEO","Remote"]', 'open', 2000000, 3500000, 'VND', 'project', 30,
     N'["SEO","Backlink","Keyword Research"]', 'beginner', 3, 0, N'Remote', 1, 0, @deadline, @now, @now, @now),

    (NEWID(), @biz4, @cat_mkt, N'Quản lý Fanpage & Lên lịch nội dung',
     N'Lên lịch đăng bài, trả lời comment/inbox, theo dõi chỉ số tương tác cho fanpage 20k followers.',
     N'["Marketing","Social Media"]', 'open', 2500000, 4000000, 'VND', 'short-term', 30,
     N'["Facebook","Content Planning","Chăm sóc cộng đồng"]', 'beginner', 1, 0, N'Hồ Chí Minh', 0, 0, @deadline, @now, @now, @now),

    (NEWID(), @biz1, @cat_tran, N'Dịch phụ đề Video EN → VI',
     N'Dịch & canh timing phụ đề cho 10 video kỹ năng (mỗi video 8-12 phút). Nộp file .srt.',
     N'["Dịch thuật","Remote"]', 'open', 1500000, 2800000, 'VND', 'short-term', 18,
     N'["Tiếng Anh","Dịch thuật","Subtitle","Timing"]', 'intermediate', 2, 0, N'Remote', 1, 0, @deadline, @now, @now, @now),

    (NEWID(), @biz3, @cat_tran, N'Dịch bài Blog Công nghệ EN → VI',
     N'Dịch 15 bài blog công nghệ (~1500 từ/bài), giữ thuật ngữ chuẩn, văn phong mượt.',
     N'["Dịch thuật","Công nghệ","Remote"]', 'open', 1800000, 3200000, 'VND', 'project', 25,
     N'["Tiếng Anh","Dịch thuật","Thuật ngữ IT"]', 'intermediate', 2, 0, N'Remote', 1, 0, @deadline, @now, @now, @now),

    (NEWID(), @biz2, @cat_vid, N'Dựng Video YouTube (Premiere / CapCut)',
     N'Edit 8 video YouTube dạng talkshow (15-20 phút): cắt ghép, chèn phụ đề, hiệu ứng, nhạc nền.',
     N'["Video","Editing","Remote"]', 'open', 3000000, 5000000, 'VND', 'project', 30,
     N'["Premiere Pro","CapCut","Editing","Motion"]', 'intermediate', 1, 0, N'Remote', 1, 1, @deadline, @now, @now, @now),

    (NEWID(), @biz2, @cat_vid, N'Quay & Dựng Video Sự kiện Sinh viên',
     N'Quay hậu trường + dựng recap 3-5 phút cho sự kiện tại TP.HCM. Có thiết bị là lợi thế.',
     N'["Video","Onsite"]', 'open', 2000000, 3500000, 'VND', 'micro', 7,
     N'["Quay phim","Editing","Storytelling"]', 'beginner', 2, 0, N'Hồ Chí Minh', 0, 0, @deadline, @now, @now, @now);

    PRINT N'Seed-jobs-extra: đã chèn 14 job mới (deadline +90 ngày).';
END
ELSE
BEGIN
    PRINT N'Seed-jobs-extra: job mẫu đã tồn tại, bỏ qua (idempotent).';
END
GO
