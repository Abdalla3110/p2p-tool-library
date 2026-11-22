using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class ToolAvailabilityConfiguration : IEntityTypeConfiguration<ToolAvailability>
{
    public void Configure(EntityTypeBuilder<ToolAvailability> entity)
    {
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.AvailabilityId).HasName("PK__ToolAvai__DA3979B1C64F8263");

            entity.ToTable("ToolAvailability");

            entity.HasIndex(e => new { e.Date, e.Status }, "IX_Availability_Date");

            entity.HasIndex(e => new { e.ToolId, e.Date }, "UQ_ToolAvailability").IsUnique();

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(AvailabilityStatusEnum.Available);

            entity.HasOne(d => d.Booking).WithMany(p => p.ToolAvailabilities)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Availability_Booking");

            entity.HasOne(d => d.Tool).WithMany(p => p.ToolAvailabilities)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Availability_Tool");
    }
}
