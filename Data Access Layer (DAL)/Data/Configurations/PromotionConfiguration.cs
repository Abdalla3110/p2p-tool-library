using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> entity)
    {
            entity.Property(e => e.DiscountType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42FCFBCC24D21");

            entity.HasIndex(e => e.Code, "UQ__Promotio__A25C5AA7766F56A5").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(10)
                .HasDefaultValue(DiscountTypeEnum.Percentage);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue(PromotionStatusEnum.Scheduled);

            entity.HasOne(d => d.Category).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Promotions_Category");
    }
}
