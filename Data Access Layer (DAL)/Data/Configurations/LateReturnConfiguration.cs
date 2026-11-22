using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class LateReturnConfiguration : IEntityTypeConfiguration<LateReturn>
{
    public void Configure(EntityTypeBuilder<LateReturn> entity)
    {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.LateReturnId).HasName("PK__LateRetu__77CE18EB71F239D1");

            entity.HasIndex(e => e.Status, "IX_LateReturns_Status");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PenaltyAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(LateReturnStatusEnum.Warning);

            entity.HasOne(d => d.Booking).WithMany(p => p.LateReturns)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LateReturns_Booking");

            entity.HasOne(d => d.BorrowerUser).WithMany(p => p.LateReturns)
                .HasForeignKey(d => d.BorrowerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LateReturns_Borrower");

            entity.HasOne(d => d.Tool).WithMany(p => p.LateReturns)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LateReturns_Tool");
    }
}
