using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Unitask.Domain.Entities;

namespace Unitask.Infrastructure.Persistence;

public partial class UnitaskDbContext : DbContext
{
    public UnitaskDbContext(DbContextOptions<UnitaskDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<AdminReport> AdminReports { get; set; }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<BusinessDashboardView> BusinessDashboardViews { get; set; }

    public virtual DbSet<BusinessProfile> BusinessProfiles { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<FAQ> FAQs { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobApplication> JobApplications { get; set; }

    public virtual DbSet<JobCategory> JobCategories { get; set; }

    public virtual DbSet<JobDetailsView> JobDetailsViews { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<StudentDashboardView> StudentDashboardViews { get; set; }

    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }

    public virtual DbSet<StudentSkill> StudentSkills { get; set; }

    public virtual DbSet<StudentWallet> StudentWallets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Activity__3214EC072A4CA5A1");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__ActivityL__UserI__367C1819");
        });

        modelBuilder.Entity<AdminReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AdminRep__3214EC0735BD4C61");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue("pending");

            entity.HasOne(d => d.ReportedBy).WithMany(p => p.AdminReportReportedBies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AdminRepo__Repor__1975C517");

            entity.HasOne(d => d.ReportedJob).WithMany(p => p.AdminReports).HasConstraintName("FK__AdminRepo__Repor__1B5E0D89");

            entity.HasOne(d => d.ReportedUser).WithMany(p => p.AdminReportReportedUsers).HasConstraintName("FK__AdminRepo__Repor__1A69E950");
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlogPost__3214EC072AC3FACF");

            entity.ToTable(tb => tb.HasTrigger("trg_BlogPosts_UpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.LikeCount).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue("draft");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ViewCount).HasDefaultValue(0);

            entity.HasOne(d => d.Author).WithMany(p => p.BlogPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogPosts__Autho__24285DB4");
        });

        modelBuilder.Entity<BusinessDashboardView>(entity =>
        {
            entity.ToView("BusinessDashboardView");
        });

        modelBuilder.Entity<BusinessProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Business__3214EC07C9C76523");

            entity.ToTable(tb => tb.HasTrigger("trg_BusinessProfiles_UpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CompanySize).HasDefaultValue("startup");
            entity.Property(e => e.CompletedProjects).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Rating).HasDefaultValue(0m);
            entity.Property(e => e.TotalSpent).HasDefaultValue(0m);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithOne(p => p.BusinessProfile).HasConstraintName("FK__BusinessP__UserI__5441852A");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Conversa__3214EC07003DAE44");

            entity.ToTable(tb => tb.HasTrigger("trg_Conversations_UpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User1).WithMany(p => p.ConversationUser1s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conversat__User1__29E1370A");

            entity.HasOne(d => d.User2).WithMany(p => p.ConversationUser2s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conversat__User2__2AD55B43");
        });

        modelBuilder.Entity<FAQ>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FAQs__3214EC07403668E9");

            entity.ToTable(tb => tb.HasTrigger("trg_FAQs_UpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.HelpfulCount).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ViewCount).HasDefaultValue(0);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Jobs__3214EC076D201E18");

            entity.ToTable(tb => tb.HasTrigger("trg_Jobs_UpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency).HasDefaultValue("VND");
            entity.Property(e => e.DurationType).HasDefaultValue("micro");
            entity.Property(e => e.ExperienceLevel).HasDefaultValue("beginner");
            entity.Property(e => e.IsFeatured).HasDefaultValue(false);
            entity.Property(e => e.IsRemote).HasDefaultValue(false);
            entity.Property(e => e.SpotsFilled).HasDefaultValue(0);
            entity.Property(e => e.SpotsTotal).HasDefaultValue(1);
            entity.Property(e => e.Status).HasDefaultValue("open");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Business).WithMany(p => p.Jobs).HasConstraintName("FK__Jobs__BusinessId__6A30C649");

            entity.HasOne(d => d.Category).WithMany(p => p.Jobs)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Jobs__CategoryId__6B24EA82");
        });

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JobAppli__3214EC078CD78FF4");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AppliedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue("pending");

            entity.HasOne(d => d.Job).WithMany(p => p.JobApplications).HasConstraintName("FK__JobApplic__JobId__0697FACD");

            entity.HasOne(d => d.Student).WithMany(p => p.JobApplications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobApplic__Stude__078C1F06");
        });

        modelBuilder.Entity<JobCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JobCateg__3214EC07850F645F");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.JobCount).HasDefaultValue(0);
        });

        modelBuilder.Entity<JobDetailsView>(entity =>
        {
            entity.ToView("JobDetailsView");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Messages__3214EC07ECFE687E");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages).HasConstraintName("FK__Messages__Conver__308E3499");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__Sender__318258D2");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC077F7DF681");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.RelatedJob).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__Relat__11D4A34F");

            entity.HasOne(d => d.RelatedUser).WithMany(p => p.NotificationRelatedUsers).HasConstraintName("FK__Notificat__Relat__12C8C788");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__10E07F16");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC07BD9B1F00");

            entity.ToTable(tb => tb.HasTrigger("trg_Payments_UpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency).HasDefaultValue("VND");
            entity.Property(e => e.Status).HasDefaultValue("pending");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Business).WithMany(p => p.Payments).HasConstraintName("FK__Payments__Busine__7FB5F314");

            entity.HasOne(d => d.JobApplication).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__JobApp__7EC1CEDB");

            entity.HasOne(d => d.Job).WithMany(p => p.Payments).HasConstraintName("FK__Payments__JobId__7DCDAAA2");

            entity.HasOne(d => d.Student).WithMany(p => p.Payments).HasConstraintName("FK__Payments__Studen__00AA174D");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reviews__3214EC07C82B75F3");

            entity.ToTable(tb => tb.HasTrigger("trg_Reviews_UpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsAnonymous).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.FromUser).WithMany(p => p.ReviewFromUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__FromUse__0A338187");

            entity.HasOne(d => d.JobApplication).WithMany(p => p.Reviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__JobAppl__093F5D4E");

            entity.HasOne(d => d.Job).WithMany(p => p.Reviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__JobId__084B3915");

            entity.HasOne(d => d.ToUser).WithMany(p => p.ReviewToUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__ToUserI__0B27A5C0");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Skills__3214EC073C6B2BBB");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<StudentDashboardView>(entity =>
        {
            entity.ToView("StudentDashboardView");
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentP__3214EC07261DB460");

            entity.ToTable(tb => tb.HasTrigger("trg_StudentProfiles_UpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CompletedJobs).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.TotalEarnings).HasDefaultValue(0m);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithOne(p => p.StudentProfile).HasConstraintName("FK__StudentPr__UserI__47DBAE45");
        });

        modelBuilder.Entity<StudentSkill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentS__3214EC076E534E1D");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AddedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.EndorsementCount).HasDefaultValue(0);
            entity.Property(e => e.Proficiency).HasDefaultValue("beginner");

            entity.HasOne(d => d.Skill).WithMany(p => p.StudentSkills).HasConstraintName("FK__StudentSk__Skill__19DFD96B");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentSkills).HasConstraintName("FK__StudentSk__Stude__18EBB532");
        });

        modelBuilder.Entity<StudentWallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentW__3214EC07C3EEA6EB");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Balance).HasDefaultValue(0m);
            entity.Property(e => e.TotalEarned).HasDefaultValue(0m);
            entity.Property(e => e.TotalWithdrawn).HasDefaultValue(0m);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Student).WithOne(p => p.StudentWallet).HasConstraintName("FK__StudentWa__Stude__7B5B524B");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC076B717E97");

            entity.ToTable(tb => tb.HasTrigger("trg_Users_UpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UserType).HasDefaultValue("student");
        });

        modelBuilder.Entity<WithdrawalRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Withdraw__3214EC07A06B1D8F");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue("pending");

            entity.HasOne(d => d.Student).WithMany(p => p.WithdrawalRequests).HasConstraintName("FK__Withdrawa__Stude__0B91BA14");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
