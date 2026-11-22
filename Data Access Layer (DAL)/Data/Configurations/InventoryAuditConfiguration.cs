using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class InventoryAuditConfiguration : IEntityTypeConfiguration<InventoryAudit>
{
    public void Configure(EntityTypeBuilder<InventoryAudit> entity)
    {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.AuditId).HasName("PK__Inventor__A17F23988971B1E5");

            entity.HasIndex(e => e.Status, "IX_Audits_Status");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DatePaperPhotoUrl).HasMaxLength(500);
            entity.Property(e => e.ReviewDecision).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(AuditStatusEnum.Pending);
            entity.Property(e => e.ToolPhotoUrl).HasMaxLength(500);

            entity.HasOne(d => d.OwnerUser).WithMany(p => p.InventoryAuditOwnerUsers)
                .HasForeignKey(d => d.OwnerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Audits_Owner");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.InventoryAuditReviewedByUsers)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK_Audits_Reviewer");

            entity.HasOne(d => d.Tool).WithMany(p => p.InventoryAudits)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Audits_Tool");

            entity.HasOne(d => d.Zone).WithMany(p => p.InventoryAudits)
                .HasForeignKey(d => d.ZoneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Audits_Zone");
    }
}
