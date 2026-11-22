using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class HandoverConfiguration : IEntityTypeConfiguration<Handover>
{
    public void Configure(EntityTypeBuilder<Handover> entity)
    {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.HandoverId).HasName("PK__Handover__DB2A1F815038847E");

            entity.HasIndex(e => e.BookingId, "IX_Handovers_Booking");

            entity.HasIndex(e => e.QrCode, "IX_Handovers_QrCode");

            entity.HasIndex(e => e.QrCode, "UQ__Handover__EE32FA443491A866").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.PickupPhotoUrl).HasMaxLength(500);
            entity.Property(e => e.QrCode).HasMaxLength(100);
            entity.Property(e => e.ReturnPhotoUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(HandoverStatusEnum.Scheduled);
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .HasDefaultValue(HandoverTypeEnum.Pickup);

            entity.HasOne(d => d.Booking).WithMany(p => p.Handovers)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Handovers_Booking");

            entity.HasOne(d => d.BorrowerUser).WithMany(p => p.HandoverBorrowerUsers)
                .HasForeignKey(d => d.BorrowerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Handovers_Borrower");

            entity.HasOne(d => d.LenderUser).WithMany(p => p.HandoverLenderUsers)
                .HasForeignKey(d => d.LenderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Handovers_Lender");

            entity.HasOne(d => d.Tool).WithMany(p => p.Handovers)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Handovers_Tool");

            entity.HasOne(d => d.VerifiedByUser).WithMany(p => p.HandoverVerifiedByUsers)
                .HasForeignKey(d => d.VerifiedByUserId)
                .HasConstraintName("FK_Handovers_Verifier");
    }
}
