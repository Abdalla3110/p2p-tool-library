using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class Conversation
{
    public Guid ConversationId { get; set; }

    public int? BookingId { get; set; }

    public int User1Id { get; set; }

    public int User2Id { get; set; }

    public int? ToolId { get; set; }

    public string? Subject { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Tool? Tool { get; set; }

    public virtual User User1 { get; set; } = null!;

    public virtual User User2 { get; set; } = null!;
}
