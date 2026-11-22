using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class Dispute
{
    public int DisputeId { get; set; }

    public int BookingId { get; set; }

    public int ToolId { get; set; }

    public int ReportedByUserId { get; set; }

    public int? AgainstUserId { get; set; }

    public string AgainstType { get; set; } = null!;

    public DisputeTypeEnum Type { get; set; }

    public string Description { get; set; } = null!;

    public string? PickupPhotoUrl { get; set; }

    public string? ReturnPhotoUrl { get; set; }

    public string? DamagePhotoUrl { get; set; }

    public decimal? DepositAtRisk { get; set; }

    public DisputeStatusEnum Status { get; set; }

    public DisputeResolutionEnum? Resolution { get; set; }

    public string? ResolutionNotes { get; set; }

    public decimal? RefundAmount { get; set; }

    public int? AssignedLibrarianUserId { get; set; }

    public DateTime OpenedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? AgainstUser { get; set; }

    public virtual User? AssignedLibrarianUser { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();

    public virtual User ReportedByUser { get; set; } = null!;

    public virtual Tool Tool { get; set; } = null!;
}
