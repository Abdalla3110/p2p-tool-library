using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class DamageReportConfiguration : IEntityTypeConfiguration<DamageReport>
{
    public void Configure(EntityTypeBuilder<DamageReport> entity)
    {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.DamageReportId).HasName("PK__DamageRe__E448C0B5772813BD");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PhotoUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(DamageReportStatusEnum.Submitted);

            entity.HasOne(d => d.Booking).WithMany(p => p.DamageReports)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DamageReports_Booking");

            entity.HasOne(d => d.Dispute).WithMany(p => p.DamageReports)
                .HasForeignKey(d => d.DisputeId)
                .HasConstraintName("FK_DamageReports_Dispute");

            entity.HasOne(d => d.ReportedByUser).WithMany(p => p.DamageReports)
                .HasForeignKey(d => d.ReportedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DamageReports_User");

            entity.HasOne(d => d.Tool).WithMany(p => p.DamageReports)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DamageReports_Tool");
    }
}
