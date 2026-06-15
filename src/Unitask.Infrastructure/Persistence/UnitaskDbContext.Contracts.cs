using Microsoft.EntityFrameworkCore;
using Unitask.Domain.Entities;

namespace Unitask.Infrastructure.Persistence;

/// <summary>
/// Phần mở rộng (partial) của DbContext cho module "Hợp đồng & Milestone".
/// Tách riêng để dễ gắn/gỡ mà không phải sửa file DbContext sinh tự động.
/// EF tự gộp các partial class + dùng hook OnModelCreatingPartial đã được
/// khai báo sẵn trong file gốc.
/// </summary>
public partial class UnitaskDbContext
{
    public virtual DbSet<Contract> Contracts { get; set; } = null!;

    public virtual DbSet<Milestone> Milestones { get; set; } = null!;

    public virtual DbSet<Submission> Submissions { get; set; } = null!;

    public virtual DbSet<Dispute> Disputes { get; set; } = null!;

    /// <summary>
    /// Cấu hình Fluent API cho 3 bảng mới. Được gọi ở cuối OnModelCreating của file gốc.
    /// </summary>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue("ACTIVE");
            entity.Property(e => e.FinalPrice).HasDefaultValue(0m);

            // NO ACTION ở mọi nhánh để tránh "multiple cascade paths" của SQL Server.
            entity.HasOne(d => d.Job).WithMany()
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.Student).WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.Business).WithMany()
                .HasForeignKey(d => d.BusinessId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue("PENDING");
            entity.Property(e => e.Amount).HasDefaultValue(0m);

            // Xóa Contract -> xóa kèm Milestone.
            entity.HasOne(d => d.Contract).WithMany(p => p.Milestones)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Milestone).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.MilestoneId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Student).WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue("NEGOTIATION");
            entity.Property(e => e.StudentPercent).HasDefaultValue(0);

            entity.HasOne(d => d.Milestone).WithMany()
                .HasForeignKey(d => d.MilestoneId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
