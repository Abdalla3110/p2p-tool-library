using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class Message
{
    public long MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public int SenderUserId { get; set; }

    public string Body { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime SentAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User SenderUser { get; set; } = null!;
}
