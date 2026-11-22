using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class UserSuspensionConfiguration : IEntityTypeConfiguration<UserSuspension>
{
    public void Configure(EntityTypeBuilder<UserSuspension> entity)
    {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Reason).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.SuspensionId).HasName("PK__UserSusp__AB37704BA22B3225");

            entity.HasIndex(e => new { e.UserId, e.Status }, "IX_Suspensions_User");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Reason).HasMaxLength(30);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(SuspensionStatusEnum.Active);
            entity.Property(e => e.Type).HasMaxLength(20);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.UserSuspensionCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Suspensions_Creator");

            entity.HasOne(d => d.LiftedByUser).WithMany(p => p.UserSuspensionLiftedByUsers)
                .HasForeignKey(d => d.LiftedByUserId)
                .HasConstraintName("FK_Suspensions_Lifter");

            entity.HasOne(d => d.User).WithMany(p => p.UserSuspensionUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Suspensions_User");
    }
}
