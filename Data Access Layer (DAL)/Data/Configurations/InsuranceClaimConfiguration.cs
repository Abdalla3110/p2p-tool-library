using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class InsuranceClaimConfiguration : IEntityTypeConfiguration<InsuranceClaim>
{
    public void Configure(EntityTypeBuilder<InsuranceClaim> entity)
    {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.ClaimId).HasName("PK__Insuranc__EF2E139B36E583EC");

            entity.Property(e => e.BookingRecordUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DamagePhotoUrl).HasMaxLength(500);
            entity.Property(e => e.PayoutAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PoliceReportUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(InsuranceClaimStatusEnum.Open);
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ToolValue).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.AssignedLibrarianUser).WithMany(p => p.InsuranceClaimAssignedLibrarianUsers)
                .HasForeignKey(d => d.AssignedLibrarianUserId)
                .HasConstraintName("FK_Claims_Librarian");

            entity.HasOne(d => d.Booking).WithMany(p => p.InsuranceClaims)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Claims_Booking");

            entity.HasOne(d => d.ClaimantUser).WithMany(p => p.InsuranceClaimClaimantUsers)
                .HasForeignKey(d => d.ClaimantUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Claims_Claimant");

            entity.HasOne(d => d.Tool).WithMany(p => p.InsuranceClaims)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Claims_Tool");
    }
}
