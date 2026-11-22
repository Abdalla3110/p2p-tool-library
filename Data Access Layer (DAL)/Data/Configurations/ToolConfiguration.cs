using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class ToolConfiguration : IEntityTypeConfiguration<Tool>
{
    public void Configure(EntityTypeBuilder<Tool> entity)
    {
            entity.Property(e => e.Condition).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.ToolId).HasName("PK__Tools__CC0CEB91FA573A2C");

            entity.HasIndex(e => e.CategoryId, "IX_Tools_Category");

            entity.HasIndex(e => e.OwnerUserId, "IX_Tools_Owner");

            entity.HasIndex(e => e.Status, "IX_Tools_Status");

            entity.HasIndex(e => e.ZoneId, "IX_Tools_Zone");

            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.CleaningBufferHours).HasDefaultValue(10);
            entity.Property(e => e.Condition)
                .HasMaxLength(20)
                .HasDefaultValue(ToolConditionEnum.Good);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InsurancePercentage)
                .HasDefaultValue(5m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxBorrowDays).HasDefaultValue(7);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PricePerDay).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PricePerHour).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PricePerWeek).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RatingAverage).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.SafetyCertificateUrl).HasMaxLength(500);
            entity.Property(e => e.SafetyVideoUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue(ToolStatusEnum.Available);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UserManualUrl).HasMaxLength(500);

            entity.HasOne(d => d.Category).WithMany(p => p.Tools)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tools_Category");

            entity.HasOne(d => d.OwnerUser).WithMany(p => p.Tools)
                .HasForeignKey(d => d.OwnerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tools_Owner");

            entity.HasOne(d => d.Zone).WithMany(p => p.Tools)
                .HasForeignKey(d => d.ZoneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tools_Zone");
    }
}
