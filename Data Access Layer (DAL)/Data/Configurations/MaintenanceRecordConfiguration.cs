using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> entity)
    {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.MaintenanceId).HasName("PK__Maintena__E60542D5D77CA1BD");

            entity.HasIndex(e => e.ToolId, "IX_Maintenance_Tool");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DurationHours).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue(MaintenanceStatusEnum.Scheduled);
            entity.Property(e => e.Type).HasMaxLength(20);

            entity.HasOne(d => d.TechnicianUser).WithMany(p => p.MaintenanceRecords)
                .HasForeignKey(d => d.TechnicianUserId)
                .HasConstraintName("FK_Maintenance_Tech");

            entity.HasOne(d => d.Tool).WithMany(p => p.MaintenanceRecords)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Maintenance_Tool");
    }
}
