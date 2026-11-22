using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class ExternalRepair
{
    public int ExternalRepairId { get; set; }

    public int ToolId { get; set; }

    public int? MaintenanceRecordId { get; set; }

    public string RepairCenterName { get; set; } = null!;

    public string? ProblemDescription { get; set; }

    public DateOnly? SentAt { get; set; }

    public DateOnly? ExpectedReturnAt { get; set; }

    public DateOnly? ActualReturnAt { get; set; }

    public decimal? Cost { get; set; }

    public string? InvoiceUrl { get; set; }

    public ExternalRepairStatusEnum Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual MaintenanceRecord? MaintenanceRecord { get; set; }

    public virtual Tool Tool { get; set; } = null!;
}
