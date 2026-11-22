using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
            entity.Property(e => e.KycStatus).HasConversion<string>();
            entity.Property(e => e.Tier).HasConversion<string>();
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C391143B9");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");

            entity.HasIndex(e => e.ZoneId, "IX_Users_ZoneId");

            entity.HasIndex(e => e.ReferralCode, "UQ__Users__7E0678120DAA43CF").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053481C0BA0E").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.EmailNotificationsEnabled).HasDefaultValue(true);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.KycIdPhotoUrl).HasMaxLength(500);
            entity.Property(e => e.KycNationalId).HasMaxLength(100);
            entity.Property(e => e.KycSelfiePhotoUrl).HasMaxLength(500);
            entity.Property(e => e.KycStatus)
                .HasMaxLength(20)
                .HasDefaultValue(KycStatusEnum.Pending);
            entity.Property(e => e.OnTimeReturnRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.PushNotificationsEnabled).HasDefaultValue(true);
            entity.Property(e => e.RatingAverage).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.ReferralCode).HasMaxLength(50);
            entity.Property(e => e.Tier)
                .HasMaxLength(20)
                .HasDefaultValue(MembershipTierEnum.Member);
            entity.Property(e => e.TrustScore).HasDefaultValue(50);
            entity.Property(e => e.WalletBalance).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.KycReviewedByUser).WithMany(p => p.InverseKycReviewedByUser)
                .HasForeignKey(d => d.KycReviewedByUserId)
                .HasConstraintName("FK_Users_KycReviewer");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");

            entity.HasOne(d => d.Zone).WithMany(p => p.Users)
                .HasForeignKey(d => d.ZoneId)
                .HasConstraintName("FK_Users_Zones");
    }
}
