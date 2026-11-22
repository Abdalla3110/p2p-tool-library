using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class ToolPhotoConfiguration : IEntityTypeConfiguration<ToolPhoto>
{
    public void Configure(EntityTypeBuilder<ToolPhoto> entity)
    {
            entity.Property(e => e.DocumentType).HasConversion<string>();
            entity.HasKey(e => e.PhotoId).HasName("PK__ToolPhot__21B7B5E28A2802DC");

            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.DocumentType).HasMaxLength(20);

            entity.HasOne(d => d.Tool).WithMany(p => p.ToolPhotos)
                .HasForeignKey(d => d.ToolId)
                .HasConstraintName("FK_ToolPhotos_Tool");
    }
}
