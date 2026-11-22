using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1ADB9DFC50");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F623DBBC03").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
    }
}
