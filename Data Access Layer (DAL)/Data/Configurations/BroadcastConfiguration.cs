using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class BroadcastConfiguration : IEntityTypeConfiguration<Broadcast>
{
    public void Configure(EntityTypeBuilder<Broadcast> entity)
    {
            entity.HasKey(e => e.BroadcastId).HasName("PK__Broadcas__94A16DC015CE2887");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Normal");
            entity.Property(e => e.SentAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.SentByUser).WithMany(p => p.Broadcasts)
                .HasForeignKey(d => d.SentByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Broadcasts_Sender");

            entity.HasOne(d => d.Zone).WithMany(p => p.Broadcasts)
                .HasForeignKey(d => d.ZoneId)
                .HasConstraintName("FK_Broadcasts_Zone");
    }
}
