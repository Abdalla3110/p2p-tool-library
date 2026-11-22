using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> entity)
    {
            entity.HasKey(e => e.RatingId).HasName("PK__Ratings__FCCDF87C03C3016B");

            entity.HasIndex(e => new { e.BookingId, e.RaterUserId }, "UX_Ratings_BookingRaterNoTarget")
                .IsUnique()
                .HasFilter("([RatedUserId] IS NULL)");

            entity.HasIndex(e => new { e.BookingId, e.RaterUserId, e.RatedUserId }, "UX_Ratings_BookingRaterTarget")
                .IsUnique()
                .HasFilter("([RatedUserId] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Rating1).HasColumnName("Rating");

            entity.HasOne(d => d.Booking).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ratings_Booking");

            entity.HasOne(d => d.RatedUser).WithMany(p => p.RatingRatedUsers)
                .HasForeignKey(d => d.RatedUserId)
                .HasConstraintName("FK_Ratings_Rated");

            entity.HasOne(d => d.RaterUser).WithMany(p => p.RatingRaterUsers)
                .HasForeignKey(d => d.RaterUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ratings_Rater");

            entity.HasOne(d => d.Tool).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.ToolId)
                .HasConstraintName("FK_Ratings_Tool");
    }
}
