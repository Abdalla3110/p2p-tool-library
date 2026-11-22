using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> entity)
    {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Resolution).HasConversion<string>();
            entity.HasKey(e => e.DisputeId).HasName("PK__Disputes__7A5F68AB7460C994");

            entity.HasIndex(e => e.BookingId, "IX_Disputes_Booking");

            entity.HasIndex(e => e.Status, "IX_Disputes_Status");

            entity.Property(e => e.AgainstType)
                .HasMaxLength(10)
                .HasDefaultValue("User");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DamagePhotoUrl).HasMaxLength(500);
            entity.Property(e => e.DepositAtRisk).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OpenedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PickupPhotoUrl).HasMaxLength(500);
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Resolution).HasMaxLength(30);
            entity.Property(e => e.ReturnPhotoUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(DisputeStatusEnum.Open);
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AgainstUser).WithMany(p => p.DisputeAgainstUsers)
                .HasForeignKey(d => d.AgainstUserId)
                .HasConstraintName("FK_Disputes_Against");

            entity.HasOne(d => d.AssignedLibrarianUser).WithMany(p => p.DisputeAssignedLibrarianUsers)
                .HasForeignKey(d => d.AssignedLibrarianUserId)
                .HasConstraintName("FK_Disputes_Librarian");

            entity.HasOne(d => d.Booking).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Disputes_Booking");

            entity.HasOne(d => d.ReportedByUser).WithMany(p => p.DisputeReportedByUsers)
                .HasForeignKey(d => d.ReportedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Disputes_Reporter");

            entity.HasOne(d => d.Tool).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Disputes_Tool");
    }
}
