using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class TechnicianProfileConfiguration : IEntityTypeConfiguration<TechnicianProfile>
{
    public void Configure(EntityTypeBuilder<TechnicianProfile> entity)
    {
            entity.HasKey(e => e.UserId).HasName("PK__Technici__1788CC4C85D5E2CE");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.AvgRepairTimeDays).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Rating).HasMaxLength(20);
            entity.Property(e => e.SuccessRate).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.User).WithOne(p => p.TechnicianProfile)
                .HasForeignKey<TechnicianProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TechProfiles_User");
    }
}
