using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class MaintenanceRecord
{
    public int MaintenanceId { get; set; }

    public int ToolId { get; set; }

    public int? TechnicianUserId { get; set; }

    public MaintenanceTypeEnum Type { get; set; }

    public string? Description { get; set; }

    public MaintenanceStatusEnum Status { get; set; }

    public string? Notes { get; set; }

    public decimal? DurationHours { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ExternalRepair> ExternalRepairs { get; set; } = new List<ExternalRepair>();

    public virtual User? TechnicianUser { get; set; }

    public virtual Tool Tool { get; set; } = null!;
}
