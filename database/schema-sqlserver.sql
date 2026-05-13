-- UniTask SQL Server Database Schema (idempotent)
-- Target: SQL Server 2019+
-- For use with .NET API and Entity Framework Core

USE master;
GO

-- Create database if it does not exist
IF DB_ID('unitask') IS NULL
BEGIN
    CREATE DATABASE unitask;
END
GO

USE unitask;
GO

-- ==========================================
-- 1. USERS TABLE
-- ==========================================

IF OBJECT_ID('dbo.Users','U') IS NULL
BEGIN
CREATE TABLE [dbo].[Users] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Email] NVARCHAR(255) UNIQUE NOT NULL,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [FullName] NVARCHAR(255) NOT NULL,
    [AvatarUrl] NVARCHAR(MAX),
    [Phone] NVARCHAR(20),
    [Bio] NVARCHAR(MAX),
    [UserType] NVARCHAR(50) NOT NULL DEFAULT 'student'
        CHECK ([UserType] IN ('student', 'business', 'admin')),
    [IsVerified] BIT DEFAULT 0,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [LastLogin] DATETIME2(7) NULL,
    INDEX [idx_users_email] ([Email]),
    INDEX [idx_users_user_type] ([UserType]),
    INDEX [idx_users_created_at] ([CreatedAt])
);
END
GO

PRINT 'Ensured [Users] table';
GO

-- ==========================================
-- 2. STUDENT PROFILES TABLE
-- ==========================================

IF OBJECT_ID('dbo.StudentProfiles','U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentProfiles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL UNIQUE,
    [StudentEmail] NVARCHAR(255),
    [University] NVARCHAR(255),
    [Major] NVARCHAR(255),
    [GraduationYear] INT,
    [CvUrl] NVARCHAR(MAX),
    [GradePoint] DECIMAL(3, 2),
    [IsVerified] BIT DEFAULT 0,
    [VerifiedAt] DATETIME2(7) NULL,
    [Bio] NVARCHAR(MAX),
    [PortfolioUrl] NVARCHAR(MAX),
    [CompletedJobs] INT DEFAULT 0,
    [TotalEarnings] DECIMAL(15, 2) DEFAULT 0,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    INDEX [idx_student_university] ([University]),
    INDEX [idx_student_major] ([Major]),
    INDEX [idx_student_verified] ([IsVerified])
);
END
GO

PRINT 'Ensured [StudentProfiles] table';
GO

-- ==========================================
-- 3. BUSINESS PROFILES TABLE
-- ==========================================

IF OBJECT_ID('dbo.BusinessProfiles','U') IS NULL
BEGIN
CREATE TABLE [dbo].[BusinessProfiles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL UNIQUE,
    [CompanyName] NVARCHAR(255) NOT NULL,
    [CompanyEmail] NVARCHAR(255),
    [CompanyWebsite] NVARCHAR(255),
    [CompanySize] NVARCHAR(50) DEFAULT 'startup'
        CHECK ([CompanySize] IN ('startup', 'sme', 'large')),
    [Industry] NVARCHAR(255),
    [Description] NVARCHAR(MAX),
    [LogoUrl] NVARCHAR(MAX),
    [CoverImageUrl] NVARCHAR(MAX),
    [IsVerified] BIT DEFAULT 0,
    [VerifiedAt] DATETIME2(7) NULL,
    [Address] NVARCHAR(MAX),
    [Phone] NVARCHAR(20),
    [CompletedProjects] INT DEFAULT 0,
    [TotalSpent] DECIMAL(15, 2) DEFAULT 0,
    [Rating] DECIMAL(3, 2) DEFAULT 0,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    INDEX [idx_business_industry] ([Industry]),
    INDEX [idx_business_verified] ([IsVerified])
);
END
GO

PRINT 'Ensured [BusinessProfiles] table';
GO

-- ==========================================
-- 4. JOB CATEGORIES TABLE
-- ==========================================

IF OBJECT_ID('dbo.JobCategories','U') IS NULL
BEGIN
CREATE TABLE [dbo].[JobCategories] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(100) NOT NULL,
    [Slug] NVARCHAR(100) NOT NULL UNIQUE,
    [Description] NVARCHAR(MAX),
    [IconUrl] NVARCHAR(MAX),
    [JobCount] INT DEFAULT 0,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    INDEX [idx_category_slug] ([Slug])
);
END
GO

PRINT 'Ensured [JobCategories] table';
GO

-- ==========================================
-- 5. JOBS TABLE
-- ==========================================

IF OBJECT_ID('dbo.Jobs','U') IS NULL
BEGIN
CREATE TABLE [dbo].[Jobs] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [BusinessId] UNIQUEIDENTIFIER NOT NULL,
    [CategoryId] UNIQUEIDENTIFIER,
    [Title] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [TagsJson] NVARCHAR(MAX),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'open'
        CHECK ([Status] IN ('draft', 'open', 'in_progress', 'completed', 'cancelled')),
    [SalaryMin] DECIMAL(15, 2),
    [SalaryMax] DECIMAL(15, 2),
    [Currency] NVARCHAR(10) DEFAULT 'VND',
    [DurationType] NVARCHAR(50) DEFAULT 'micro'
        CHECK ([DurationType] IN ('micro', 'short-term', 'project')),
    [DurationDays] INT,
    [RequiredSkillsJson] NVARCHAR(MAX),
    [ExperienceLevel] NVARCHAR(50) DEFAULT 'beginner'
        CHECK ([ExperienceLevel] IN ('beginner', 'intermediate', 'advanced')),
    [SpotsTotal] INT DEFAULT 1,
    [SpotsFilled] INT DEFAULT 0,
    [Location] NVARCHAR(255),
    [IsRemote] BIT DEFAULT 0,
    [IsFeatured] BIT DEFAULT 0,
    [Deadline] DATETIME2(7),
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [PublishedAt] DATETIME2(7) NULL,
    FOREIGN KEY ([BusinessId]) REFERENCES [dbo].[BusinessProfiles]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[JobCategories]([Id]) ON DELETE NO ACTION,
    INDEX [idx_job_status] ([Status]),
    INDEX [idx_job_business] ([BusinessId]),
    INDEX [idx_job_deadline] ([Deadline]),
    INDEX [idx_job_featured] ([IsFeatured]),
    INDEX [idx_job_created_at] ([CreatedAt])
);
END
GO

PRINT 'Ensured [Jobs] table';
GO

-- ==========================================
-- 6. JOB APPLICATIONS TABLE
-- ==========================================

IF OBJECT_ID('dbo.JobApplications','U') IS NULL
BEGIN
CREATE TABLE [dbo].[JobApplications] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [JobId] UNIQUEIDENTIFIER NOT NULL,
    [StudentId] UNIQUEIDENTIFIER NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending'
        CHECK ([Status] IN ('pending', 'accepted', 'rejected', 'completed', 'cancelled')),
    [AppliedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [CoverLetter] NVARCHAR(MAX),
    [ProposedTimeline] NVARCHAR(255),
    [AcceptedAt] DATETIME2(7) NULL,
    [RejectedAt] DATETIME2(7) NULL,
    [RejectionReason] NVARCHAR(MAX),
    [CompletedAt] DATETIME2(7) NULL,
    [StartedAt] DATETIME2(7) NULL,
    FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([StudentId]) REFERENCES [dbo].[StudentProfiles]([Id]) ON DELETE NO ACTION,
    INDEX [idx_application_status] ([Status]),
    INDEX [idx_application_student] ([StudentId]),
    INDEX [idx_application_job] ([JobId])
);
END
GO

PRINT 'Ensured [JobApplications] table';
GO

-- ==========================================
-- 7. STUDENT WALLETS TABLE
-- ==========================================

IF OBJECT_ID('dbo.StudentWallets','U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentWallets] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [StudentId] UNIQUEIDENTIFIER NOT NULL UNIQUE,
    [Balance] DECIMAL(15, 2) DEFAULT 0,
    [TotalEarned] DECIMAL(15, 2) DEFAULT 0,
    [TotalWithdrawn] DECIMAL(15, 2) DEFAULT 0,
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    FOREIGN KEY ([StudentId]) REFERENCES [dbo].[StudentProfiles]([Id]) ON DELETE NO ACTION,
    INDEX [idx_wallet_student] ([StudentId])
);
END
GO

PRINT 'Ensured [StudentWallets] table';
GO

-- ==========================================
-- 8. PAYMENTS TABLE
-- ==========================================

IF OBJECT_ID('dbo.Payments','U') IS NULL
BEGIN
CREATE TABLE [dbo].[Payments] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [JobId] UNIQUEIDENTIFIER,
    [JobApplicationId] UNIQUEIDENTIFIER NOT NULL,
    [BusinessId] UNIQUEIDENTIFIER,
    [StudentId] UNIQUEIDENTIFIER,
    [Amount] DECIMAL(15, 2) NOT NULL,
    [Currency] NVARCHAR(10) DEFAULT 'VND',
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending'
        CHECK ([Status] IN ('pending', 'escrow', 'released', 'refunded', 'disputed')),
    [PaymentMethod] NVARCHAR(100),
    [Description] NVARCHAR(MAX),
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [ReleasedAt] DATETIME2(7) NULL,
    FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([JobApplicationId]) REFERENCES [dbo].[JobApplications]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([BusinessId]) REFERENCES [dbo].[BusinessProfiles]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([StudentId]) REFERENCES [dbo].[StudentProfiles]([Id]) ON DELETE NO ACTION,
    INDEX [idx_payment_status] ([Status]),
    INDEX [idx_payment_application] ([JobApplicationId]),
    INDEX [idx_payment_created_at] ([CreatedAt])
);
END
GO

PRINT 'Ensured [Payments] table';
GO

-- ==========================================
-- 9. WITHDRAWAL REQUESTS TABLE
-- ==========================================

IF OBJECT_ID('dbo.WithdrawalRequests','U') IS NULL
BEGIN
CREATE TABLE [dbo].[WithdrawalRequests] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [StudentId] UNIQUEIDENTIFIER NOT NULL,
    [Amount] DECIMAL(15, 2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending'
        CHECK ([Status] IN ('pending', 'approved', 'rejected', 'completed')),
    [BankAccountName] NVARCHAR(255),
    [BankAccountNumber] NVARCHAR(50),
    [BankName] NVARCHAR(255),
    [RequestedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [CompletedAt] DATETIME2(7) NULL,
    [Reason] NVARCHAR(MAX),
    FOREIGN KEY ([StudentId]) REFERENCES [dbo].[StudentProfiles]([Id]) ON DELETE NO ACTION,
    INDEX [idx_withdrawal_status] ([Status]),
    INDEX [idx_withdrawal_student] ([StudentId])
);
END
GO

PRINT 'Ensured [WithdrawalRequests] table';
GO

-- ==========================================
-- 10. SKILLS TABLE
-- ==========================================

IF OBJECT_ID('dbo.Skills','U') IS NULL
BEGIN
CREATE TABLE [dbo].[Skills] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL UNIQUE,
    [Category] NVARCHAR(100),
    [IconUrl] NVARCHAR(MAX),
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    INDEX [idx_skill_name] ([Name]),
    INDEX [idx_skill_category] ([Category])
);
END
GO

PRINT 'Ensured [Skills] table';
GO

-- ==========================================
-- 11. STUDENT SKILLS TABLE
-- ==========================================

IF OBJECT_ID('dbo.StudentSkills','U') IS NULL
BEGIN
CREATE TABLE [dbo].[StudentSkills] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [StudentId] UNIQUEIDENTIFIER NOT NULL,
    [SkillId] UNIQUEIDENTIFIER NOT NULL,
    [Proficiency] NVARCHAR(50) DEFAULT 'beginner'
        CHECK ([Proficiency] IN ('beginner', 'intermediate', 'advanced', 'expert')),
    [EndorsementCount] INT DEFAULT 0,
    [AddedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    FOREIGN KEY ([StudentId]) REFERENCES [dbo].[StudentProfiles]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([SkillId]) REFERENCES [dbo].[Skills]([Id]) ON DELETE NO ACTION,
    UNIQUE ([StudentId], [SkillId]),
    INDEX [idx_student_skill_student] ([StudentId]),
    INDEX [idx_student_skill_skill] ([SkillId])
);
END
GO

PRINT 'Ensured [StudentSkills] table';
GO

-- ==========================================
-- 12. REVIEWS TABLE
-- ==========================================

IF OBJECT_ID('dbo.Reviews','U') IS NULL
BEGIN
CREATE TABLE [dbo].[Reviews] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [JobId] UNIQUEIDENTIFIER NOT NULL,
    [JobApplicationId] UNIQUEIDENTIFIER NOT NULL,
    [FromUserId] UNIQUEIDENTIFIER NOT NULL,
    [ToUserId] UNIQUEIDENTIFIER NOT NULL,
    [Rating] INT NOT NULL CHECK ([Rating] >= 1 AND [Rating] <= 5),
    [Comment] NVARCHAR(MAX),
    [SkillEndorsementsJson] NVARCHAR(MAX),
    [IsAnonymous] BIT DEFAULT 0,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([JobApplicationId]) REFERENCES [dbo].[JobApplications]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([FromUserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([ToUserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    INDEX [idx_review_rating] ([Rating]),
    INDEX [idx_review_to_user] ([ToUserId])
);
END
GO

PRINT 'Ensured [Reviews] table';
GO

-- ==========================================
-- 13. BLOG POSTS TABLE
-- ==========================================

IF OBJECT_ID('dbo.BlogPosts','U') IS NULL
BEGIN
CREATE TABLE [dbo].[BlogPosts] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AuthorId] UNIQUEIDENTIFIER NOT NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Slug] NVARCHAR(255) NOT NULL UNIQUE,
    [Content] NVARCHAR(MAX) NOT NULL,
    [Excerpt] NVARCHAR(MAX),
    [FeaturedImageUrl] NVARCHAR(MAX),
    [Category] NVARCHAR(100),
    [TagsJson] NVARCHAR(MAX),
    [Status] NVARCHAR(50) DEFAULT 'draft'
        CHECK ([Status] IN ('draft', 'published', 'archived')),
    [ViewCount] INT DEFAULT 0,
    [LikeCount] INT DEFAULT 0,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [PublishedAt] DATETIME2(7) NULL,
    FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    INDEX [idx_blog_status] ([Status]),
    INDEX [idx_blog_slug] ([Slug]),
    INDEX [idx_blog_published_at] ([PublishedAt])
);
END
GO

PRINT 'Ensured [BlogPosts] table';
GO

-- ==========================================
-- 14. FAQ TABLE
-- ==========================================

IF OBJECT_ID('dbo.FAQs','U') IS NULL
BEGIN
CREATE TABLE [dbo].[FAQs] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Question] NVARCHAR(MAX) NOT NULL,
    [Answer] NVARCHAR(MAX) NOT NULL,
    [Category] NVARCHAR(100),
    [ViewCount] INT DEFAULT 0,
    [HelpfulCount] INT DEFAULT 0,
    [OrderIndex] INT,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    INDEX [idx_faq_category] ([Category]),
    INDEX [idx_faq_order] ([OrderIndex])
);
END
GO

PRINT 'Ensured [FAQs] table';
GO

-- ==========================================
-- 15. CONVERSATIONS TABLE
-- ==========================================

IF OBJECT_ID('dbo.Conversations','U') IS NULL
BEGIN
CREATE TABLE [dbo].[Conversations] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [User1Id] UNIQUEIDENTIFIER NOT NULL,
    [User2Id] UNIQUEIDENTIFIER NOT NULL,
    [LastMessageAt] DATETIME2(7) NULL,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    FOREIGN KEY ([User1Id]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([User2Id]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    INDEX [idx_conversation_users] ([User1Id], [User2Id])
);
END
GO

PRINT 'Ensured [Conversations] table';
GO

-- ==========================================
-- 16. MESSAGES TABLE
-- ==========================================

IF OBJECT_ID('dbo.Messages','U') IS NULL
BEGIN
CREATE TABLE [dbo].[Messages] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ConversationId] UNIQUEIDENTIFIER NOT NULL,
    [SenderId] UNIQUEIDENTIFIER NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [AttachmentUrl] NVARCHAR(MAX),
    [IsRead] BIT DEFAULT 0,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [ReadAt] DATETIME2(7) NULL,
    FOREIGN KEY ([ConversationId]) REFERENCES [dbo].[Conversations]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([SenderId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    INDEX [idx_message_conversation] ([ConversationId]),
    INDEX [idx_message_sender] ([SenderId]),
    INDEX [idx_message_is_read] ([IsRead])
);
END
GO

PRINT 'Ensured [Messages] table';
GO

-- ==========================================
-- 17. NOTIFICATIONS TABLE
-- ==========================================

IF OBJECT_ID('dbo.Notifications','U') IS NULL
BEGIN
CREATE TABLE [dbo].[Notifications] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Type] NVARCHAR(100),
    [Title] NVARCHAR(255),
    [Message] NVARCHAR(MAX) NOT NULL,
    [RelatedJobId] UNIQUEIDENTIFIER,
    [RelatedUserId] UNIQUEIDENTIFIER,
    [IsRead] BIT DEFAULT 0,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [ReadAt] DATETIME2(7) NULL,
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([RelatedJobId]) REFERENCES [dbo].[Jobs]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([RelatedUserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    INDEX [idx_notification_user] ([UserId]),
    INDEX [idx_notification_is_read] ([IsRead]),
    INDEX [idx_notification_created_at] ([CreatedAt])
);
END
GO

PRINT 'Ensured [Notifications] table';
GO

-- ==========================================
-- 18. ADMIN REPORTS TABLE
-- ==========================================

IF OBJECT_ID('dbo.AdminReports','U') IS NULL
BEGIN
CREATE TABLE [dbo].[AdminReports] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ReportedById] UNIQUEIDENTIFIER NOT NULL,
    [ReportedUserId] UNIQUEIDENTIFIER,
    [ReportedJobId] UNIQUEIDENTIFIER,
    [ReportType] NVARCHAR(100),
    [Reason] NVARCHAR(MAX) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending'
        CHECK ([Status] IN ('pending', 'investigating', 'resolved', 'dismissed')),
    [ActionTaken] NVARCHAR(MAX),
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    [ResolvedAt] DATETIME2(7) NULL,
    FOREIGN KEY ([ReportedById]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([ReportedUserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    FOREIGN KEY ([ReportedJobId]) REFERENCES [dbo].[Jobs]([Id]) ON DELETE NO ACTION,
    INDEX [idx_report_status] ([Status]),
    INDEX [idx_report_created_at] ([CreatedAt])
);
END
GO

PRINT 'Ensured [AdminReports] table';
GO

-- ==========================================
-- 19. ACTIVITY LOGS TABLE
-- ==========================================

IF OBJECT_ID('dbo.ActivityLogs','U') IS NULL
BEGIN
CREATE TABLE [dbo].[ActivityLogs] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER,
    [ActionType] NVARCHAR(100),
    [EntityType] NVARCHAR(100),
    [EntityId] UNIQUEIDENTIFIER,
    [Description] NVARCHAR(MAX),
    [IpAddress] NVARCHAR(45),
    [UserAgent] NVARCHAR(MAX),
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
    INDEX [idx_activity_user] ([UserId]),
    INDEX [idx_activity_created_at] ([CreatedAt])
);
END
GO

PRINT 'Ensured [ActivityLogs] table';
GO

-- ==========================================
-- TRIGGERS for automatic UpdatedAt (create if not exists)
-- ==========================================

-- Drop and (re)create triggers so script can be re-run safely
DROP TRIGGER IF EXISTS [dbo].[trg_Users_UpdatedAt];
GO
CREATE TRIGGER [dbo].[trg_Users_UpdatedAt]
ON [dbo].[Users]
AFTER UPDATE
AS
BEGIN
    UPDATE [dbo].[Users]
    SET [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM inserted)
END
GO

DROP TRIGGER IF EXISTS [dbo].[trg_StudentProfiles_UpdatedAt];
GO
CREATE TRIGGER [dbo].[trg_StudentProfiles_UpdatedAt]
ON [dbo].[StudentProfiles]
AFTER UPDATE
AS
BEGIN
    UPDATE [dbo].[StudentProfiles]
    SET [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM inserted)
END
GO

DROP TRIGGER IF EXISTS [dbo].[trg_BusinessProfiles_UpdatedAt];
GO
CREATE TRIGGER [dbo].[trg_BusinessProfiles_UpdatedAt]
ON [dbo].[BusinessProfiles]
AFTER UPDATE
AS
BEGIN
    UPDATE [dbo].[BusinessProfiles]
    SET [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM inserted)
END
GO

DROP TRIGGER IF EXISTS [dbo].[trg_Jobs_UpdatedAt];
GO
CREATE TRIGGER [dbo].[trg_Jobs_UpdatedAt]
ON [dbo].[Jobs]
AFTER UPDATE
AS
BEGIN
    UPDATE [dbo].[Jobs]
    SET [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM inserted)
END
GO

DROP TRIGGER IF EXISTS [dbo].[trg_Payments_UpdatedAt];
GO
CREATE TRIGGER [dbo].[trg_Payments_UpdatedAt]
ON [dbo].[Payments]
AFTER UPDATE
AS
BEGIN
    UPDATE [dbo].[Payments]
    SET [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM inserted)
END
GO

DROP TRIGGER IF EXISTS [dbo].[trg_Reviews_UpdatedAt];
GO
CREATE TRIGGER [dbo].[trg_Reviews_UpdatedAt]
ON [dbo].[Reviews]
AFTER UPDATE
AS
BEGIN
    UPDATE [dbo].[Reviews]
    SET [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM inserted)
END
GO

DROP TRIGGER IF EXISTS [dbo].[trg_BlogPosts_UpdatedAt];
GO
CREATE TRIGGER [dbo].[trg_BlogPosts_UpdatedAt]
ON [dbo].[BlogPosts]
AFTER UPDATE
AS
BEGIN
    UPDATE [dbo].[BlogPosts]
    SET [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM inserted)
END
GO

DROP TRIGGER IF EXISTS [dbo].[trg_FAQs_UpdatedAt];
GO
CREATE TRIGGER [dbo].[trg_FAQs_UpdatedAt]
ON [dbo].[FAQs]
AFTER UPDATE
AS
BEGIN
    UPDATE [dbo].[FAQs]
    SET [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM inserted)
END
GO

DROP TRIGGER IF EXISTS [dbo].[trg_Conversations_UpdatedAt];
GO
CREATE TRIGGER [dbo].[trg_Conversations_UpdatedAt]
ON [dbo].[Conversations]
AFTER UPDATE
AS
BEGIN
    UPDATE [dbo].[Conversations]
    SET [UpdatedAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT [Id] FROM inserted)
END
GO

PRINT 'Ensured all triggers for auto UpdatedAt';
GO

-- ==========================================
-- VIEWS for common queries (create if not exists)
-- ==========================================

-- Recreate views so script can be re-run; views must be created as standalone batches
DROP VIEW IF EXISTS [dbo].[StudentDashboardView];
GO
CREATE VIEW [dbo].[StudentDashboardView] AS
SELECT 
    sp.[Id] as StudentId,
    u.[Email],
    u.[FullName],
    sp.[University],
    sp.[Major],
    sp.[CompletedJobs],
    sp.[TotalEarnings],
    sw.[Balance],
    (SELECT COUNT(*) FROM [dbo].[JobApplications] WHERE [StudentId] = sp.[Id] AND [Status] = 'pending') as PendingApplications,
    (SELECT COUNT(*) FROM [dbo].[JobApplications] WHERE [StudentId] = sp.[Id] AND [Status] = 'accepted') as ActiveJobs
FROM [dbo].[StudentProfiles] sp
INNER JOIN [dbo].[Users] u ON sp.[UserId] = u.[Id]
LEFT JOIN [dbo].[StudentWallets] sw ON sp.[Id] = sw.[StudentId];
GO

DROP VIEW IF EXISTS [dbo].[BusinessDashboardView];
GO
CREATE VIEW [dbo].[BusinessDashboardView] AS
SELECT 
    bp.[Id] as BusinessId,
    u.[Email],
    u.[FullName],
    bp.[CompanyName],
    bp.[Industry],
    bp.[CompletedProjects],
    bp.[TotalSpent],
    bp.[Rating],
    (SELECT COUNT(*) FROM [dbo].[Jobs] WHERE [BusinessId] = bp.[Id] AND [Status] = 'open') as OpenJobs,
    (SELECT COUNT(*) FROM [dbo].[JobApplications] ja 
        INNER JOIN [dbo].[Jobs] j ON ja.[JobId] = j.[Id] 
        WHERE j.[BusinessId] = bp.[Id] AND ja.[Status] = 'pending') as PendingApplications
FROM [dbo].[BusinessProfiles] bp
INNER JOIN [dbo].[Users] u ON bp.[UserId] = u.[Id];
GO

DROP VIEW IF EXISTS [dbo].[JobDetailsView];
GO
CREATE VIEW [dbo].[JobDetailsView] AS
SELECT 
    j.[Id],
    j.[Title],
    j.[Description],
    j.[Status],
    j.[SalaryMin],
    j.[SalaryMax],
    j.[Currency],
    j.[DurationType],
    j.[DurationDays],
    j.[ExperienceLevel],
    j.[SpotsTotal],
    j.[SpotsFilled],
    j.[Location],
    j.[IsRemote],
    j.[Deadline],
    j.[CreatedAt],
    j.[UpdatedAt],
    bp.[CompanyName],
    u.[FullName] as CompanyContactName,
    u.[Email] as CompanyEmail,
    jc.[Name] as CategoryName,
    (SELECT COUNT(*) FROM [dbo].[JobApplications] WHERE [JobId] = j.[Id]) as TotalApplications
FROM [dbo].[Jobs] j
INNER JOIN [dbo].[BusinessProfiles] bp ON j.[BusinessId] = bp.[Id]
INNER JOIN [dbo].[Users] u ON bp.[UserId] = u.[Id]
LEFT JOIN [dbo].[JobCategories] jc ON j.[CategoryId] = jc.[Id];
GO

PRINT 'Ensured all views successfully!';
GO

PRINT '========================================';
PRINT 'UniTask Database Schema ensured (idempotent)';
PRINT '========================================';
