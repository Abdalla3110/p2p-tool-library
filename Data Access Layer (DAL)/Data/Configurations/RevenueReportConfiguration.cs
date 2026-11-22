using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class RevenueReportConfiguration : IEntityTypeConfiguration<RevenueReport>
{
    public void Configure(EntityTypeBuilder<RevenueReport> entity)
    {
            entity.HasKey(e => e.RevenueReportId).HasName("PK__RevenueR__86917D6AB82AA3EC");

            entity.HasIndex(e => new { e.PeriodMonth, e.Source }, "UQ_RevenueReports_Period_Source").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.SharePercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Source).HasMaxLength(50);
    }
}
