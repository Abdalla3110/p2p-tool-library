using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> entity)
    {
            entity.Property(e => e.DepositStatus).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951AED97635D39");

            entity.HasIndex(e => e.BorrowerUserId, "IX_Bookings_Borrower");

            entity.HasIndex(e => new { e.PickupDate, e.ReturnDate }, "IX_Bookings_Dates");

            entity.HasIndex(e => e.LenderUserId, "IX_Bookings_Lender");

            entity.HasIndex(e => e.Status, "IX_Bookings_Status");

            entity.HasIndex(e => e.ToolId, "IX_Bookings_Tool");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DepositStatus)
                .HasMaxLength(20)
                .HasDefaultValue(DepositStatusEnum.Pending);
            entity.Property(e => e.FinalAmountDue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InsuranceAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PickupLocation).HasMaxLength(300);
            entity.Property(e => e.PlatformFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PromotionDiscount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RentalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ReturnLocation).HasMaxLength(300);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue(BookingStatusEnum.Pending);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.WalletCreditApplied).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.BorrowerUser).WithMany(p => p.BookingBorrowerUsers)
                .HasForeignKey(d => d.BorrowerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Borrower");

            entity.HasOne(d => d.LenderUser).WithMany(p => p.BookingLenderUsers)
                .HasForeignKey(d => d.LenderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Lender");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("FK_Bookings_Promotion");

            entity.HasOne(d => d.Tool).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Tool");
    }
}
