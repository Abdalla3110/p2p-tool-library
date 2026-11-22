using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class LibrarianTask
{
    public int TaskId { get; set; }

    public int LibrarianUserId { get; set; }

    public int? ZoneId { get; set; }

    public string Type { get; set; } = null!;

    public string? RelatedEntityType { get; set; }

    public string? RelatedEntityId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DueAt { get; set; }

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User LibrarianUser { get; set; } = null!;

    public virtual Zone? Zone { get; set; }
}
