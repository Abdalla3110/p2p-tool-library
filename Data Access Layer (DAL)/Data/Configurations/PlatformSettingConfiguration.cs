using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class PlatformSettingConfiguration : IEntityTypeConfiguration<PlatformSetting>
{
    public void Configure(EntityTypeBuilder<PlatformSetting> entity)
    {
            entity.HasKey(e => e.SettingsId).HasName("PK__Platform__991B19FCE7597595");

            entity.Property(e => e.SettingsId).HasDefaultValue(1);
            entity.Property(e => e.DefaultCleaningBufferHours).HasDefaultValue(10);
            entity.Property(e => e.DefaultDepositPercentage)
                .HasDefaultValue(20m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.DefaultMaxBorrowDays).HasDefaultValue(7);
            entity.Property(e => e.EmailBookingConfirmationsEnabled).HasDefaultValue(true);
            entity.Property(e => e.InsurancePercentage)
                .HasDefaultValue(5m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.LateReturnGraceHours).HasDefaultValue(2);
            entity.Property(e => e.LateReturnPenaltyPerDay)
                .HasDefaultValue(50m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PlatformCommissionPercentage)
                .HasDefaultValue(18m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PushNotificationsEnabled).HasDefaultValue(true);
            entity.Property(e => e.SmsLateReturnAlertsEnabled).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.WeeklyMaintenanceRemindersEnabled).HasDefaultValue(true);

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PlatformSettings)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Settings_Updater");
    }
}
