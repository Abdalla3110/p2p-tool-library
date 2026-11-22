using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class LibrarianTaskConfiguration : IEntityTypeConfiguration<LibrarianTask>
{
    public void Configure(EntityTypeBuilder<LibrarianTask> entity)
    {
            entity.HasKey(e => e.TaskId).HasName("PK__Libraria__7C6949B1012CF692");

            entity.HasIndex(e => new { e.LibrarianUserId, e.Status }, "IX_Tasks_Librarian");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Normal");
            entity.Property(e => e.RelatedEntityId).HasMaxLength(50);
            entity.Property(e => e.RelatedEntityType).HasMaxLength(30);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(40);

            entity.HasOne(d => d.LibrarianUser).WithMany(p => p.LibrarianTasks)
                .HasForeignKey(d => d.LibrarianUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_Librarian");

            entity.HasOne(d => d.Zone).WithMany(p => p.LibrarianTasks)
                .HasForeignKey(d => d.ZoneId)
                .HasConstraintName("FK_Tasks_Zone");
    }
}
