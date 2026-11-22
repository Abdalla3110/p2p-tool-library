using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class Broadcast
{
    public int BroadcastId { get; set; }

    public int? ZoneId { get; set; }

    public string? Title { get; set; }

    public string Message { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public int RecipientCount { get; set; }

    public int SentByUserId { get; set; }

    public DateTime SentAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User SentByUser { get; set; } = null!;

    public virtual Zone? Zone { get; set; }
}
