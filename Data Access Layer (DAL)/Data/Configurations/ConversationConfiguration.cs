using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> entity)
    {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__C050D877DCDF21EE");

            entity.HasIndex(e => e.User1Id, "IX_Conversations_User1");

            entity.HasIndex(e => e.User2Id, "IX_Conversations_User2");

            entity.HasIndex(e => new { e.User1Id, e.User2Id, e.BookingId }, "UX_Conversations_PairBooking")
                .IsUnique()
                .HasFilter("([BookingId] IS NOT NULL)");

            entity.HasIndex(e => new { e.User1Id, e.User2Id }, "UX_Conversations_PairNoBooking")
                .IsUnique()
                .HasFilter("([BookingId] IS NULL)");

            entity.Property(e => e.ConversationId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Subject).HasMaxLength(200);

            entity.HasOne(d => d.Booking).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_Conversations_Booking");

            entity.HasOne(d => d.Tool).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.ToolId)
                .HasConstraintName("FK_Conversations_Tool");

            entity.HasOne(d => d.User1).WithMany(p => p.ConversationUser1s)
                .HasForeignKey(d => d.User1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversations_User1");

            entity.HasOne(d => d.User2).WithMany(p => p.ConversationUser2s)
                .HasForeignKey(d => d.User2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversations_User2");
    }
}
