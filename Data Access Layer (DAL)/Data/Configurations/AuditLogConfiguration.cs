using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CBDD8B9AFFC");

            entity.HasIndex(e => e.CreatedAt, "IX_AuditLogs_Created");

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "IX_AuditLogs_Entity");

            entity.Property(e => e.Action).HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLogs_User");
    }
}
