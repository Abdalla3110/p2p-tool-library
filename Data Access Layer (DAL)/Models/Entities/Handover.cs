using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class Handover
{
    public int HandoverId { get; set; }

    public int BookingId { get; set; }

    public int ToolId { get; set; }

    public int BorrowerUserId { get; set; }

    public int LenderUserId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public string? Location { get; set; }

    public string QrCode { get; set; } = null!;

    public HandoverTypeEnum Type { get; set; }

    public HandoverStatusEnum Status { get; set; }

    public string? PickupPhotoUrl { get; set; }

    public string? ReturnPhotoUrl { get; set; }

    public bool? ChecklistMatchesListing { get; set; }

    public bool? ChecklistAccessories { get; set; }

    public bool? ChecklistNoDamage { get; set; }

    public bool? ChecklistBothPresent { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public int? VerifiedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User BorrowerUser { get; set; } = null!;

    public virtual User LenderUser { get; set; } = null!;

    public virtual Tool Tool { get; set; } = null!;

    public virtual User? VerifiedByUser { get; set; }
}
