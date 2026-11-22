using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class ExternalRepairConfiguration : IEntityTypeConfiguration<ExternalRepair>
{
    public void Configure(EntityTypeBuilder<ExternalRepair> entity)
    {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.ExternalRepairId).HasName("PK__External__7D8FAB86F0CB7F4A");

            entity.Property(e => e.Cost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.InvoiceUrl).HasMaxLength(500);
            entity.Property(e => e.RepairCenterName).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(ExternalRepairStatusEnum.Inspecting);

            entity.HasOne(d => d.MaintenanceRecord).WithMany(p => p.ExternalRepairs)
                .HasForeignKey(d => d.MaintenanceRecordId)
                .HasConstraintName("FK_ExtRepairs_Maint");

            entity.HasOne(d => d.Tool).WithMany(p => p.ExternalRepairs)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExtRepairs_Tool");
    }
}
