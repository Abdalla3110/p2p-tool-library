using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class InventoryAudit
{
    public int AuditId { get; set; }

    public int ToolId { get; set; }

    public int OwnerUserId { get; set; }

    public int ZoneId { get; set; }

    public DateOnly DueDate { get; set; }

    public AuditStatusEnum Status { get; set; }

    public string? ToolPhotoUrl { get; set; }

    public string? DatePaperPhotoUrl { get; set; }

    public string? ReviewDecision { get; set; }

    public string? ReviewNotes { get; set; }

    public int? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User OwnerUser { get; set; } = null!;

    public virtual User? ReviewedByUser { get; set; }

    public virtual Tool Tool { get; set; } = null!;

    public virtual Zone Zone { get; set; } = null!;
}
