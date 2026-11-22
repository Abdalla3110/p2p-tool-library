using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class DisposalConfiguration : IEntityTypeConfiguration<Disposal>
{
    public void Configure(EntityTypeBuilder<Disposal> entity)
    {
            entity.Property(e => e.Reason).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.DisposalId).HasName("PK__Disposal__206044232569FE92");

            entity.Property(e => e.CancelFutureBookings).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Reason).HasMaxLength(30);
            entity.Property(e => e.RecycleCenter).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(DisposalStatusEnum.Pending);

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.Disposals)
                .HasForeignKey(d => d.ApprovedByUserId)
                .HasConstraintName("FK_Disposals_Approver");

            entity.HasOne(d => d.Tool).WithMany(p => p.Disposals)
                .HasForeignKey(d => d.ToolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Disposals_Tool");
    }
}
