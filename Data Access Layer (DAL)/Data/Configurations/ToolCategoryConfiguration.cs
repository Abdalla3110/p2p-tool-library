using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class ToolCategoryConfiguration : IEntityTypeConfiguration<ToolCategory>
{
    public void Configure(EntityTypeBuilder<ToolCategory> entity)
    {
            entity.HasKey(e => e.CategoryId).HasName("PK__ToolCate__19093A0B1FE1825F");

            entity.HasIndex(e => e.Name, "UQ__ToolCate__737584F65D5829D8").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ToolCategories_Parent");
    }
}
