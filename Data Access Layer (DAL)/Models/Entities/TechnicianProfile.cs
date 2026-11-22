using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class TechnicianProfile
{
    public int UserId { get; set; }

    public string? Specializations { get; set; }

    public string? AssignedZoneIds { get; set; }

    public int ToolsRepairedCount { get; set; }

    public decimal? AvgRepairTimeDays { get; set; }

    public decimal? SuccessRate { get; set; }

    public string? Rating { get; set; }

    public int OpenTasksCount { get; set; }

    public string? Bio { get; set; }

    public virtual User User { get; set; } = null!;
}
