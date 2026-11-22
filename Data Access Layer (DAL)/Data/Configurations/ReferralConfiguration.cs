using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> entity)
    {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.ReferralId).HasName("PK__Referral__A2C4A96696B80405");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreditAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ReferralCode).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(ReferralStatusEnum.Pending);

            entity.HasOne(d => d.ReferredUser).WithMany(p => p.ReferralReferredUsers)
                .HasForeignKey(d => d.ReferredUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Referrals_Referred");

            entity.HasOne(d => d.ReferrerUser).WithMany(p => p.ReferralReferrerUsers)
                .HasForeignKey(d => d.ReferrerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Referrals_Referrer");
    }
}
