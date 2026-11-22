using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class DamageReport
{
    public int DamageReportId { get; set; }

    public int BookingId { get; set; }

    public int ToolId { get; set; }

    public int ReportedByUserId { get; set; }

    public string? Description { get; set; }

    public string? PhotoUrl { get; set; }

    public DamageReportStatusEnum Status { get; set; }

    public int? DisputeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Dispute? Dispute { get; set; }

    public virtual User ReportedByUser { get; set; } = null!;

    public virtual Tool Tool { get; set; } = null!;
}
