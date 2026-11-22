using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class DepositConfiguration : IEntityTypeConfiguration<Deposit>
{
    public void Configure(EntityTypeBuilder<Deposit> entity)
    {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.DepositId).HasName("PK__Deposits__AB60DF71393B7681");

            entity.HasIndex(e => e.BookingId, "UX_Deposits_Booking").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HeldAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(DepositStatusEnum.Held);

            entity.HasOne(d => d.Booking).WithOne(p => p.Deposit)
                .HasForeignKey<Deposit>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deposits_Booking");

            entity.HasOne(d => d.ReleasedToUser).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.ReleasedToUserId)
                .HasConstraintName("FK_Deposits_ReleasedTo");
    }
}
