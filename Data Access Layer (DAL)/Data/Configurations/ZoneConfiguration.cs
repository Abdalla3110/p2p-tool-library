using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> entity)
    {
            entity.HasKey(e => e.ZoneId).HasName("PK__Zones__601667B50501D747");

            entity.HasIndex(e => e.Name, "UQ__Zones__737584F6598D9777").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.LibrarianUser).WithMany(p => p.Zones)
                .HasForeignKey(d => d.LibrarianUserId)
                .HasConstraintName("FK_Zones_Librarian");
    }
}
