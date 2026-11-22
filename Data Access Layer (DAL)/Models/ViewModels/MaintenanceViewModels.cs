using System.ComponentModel.DataAnnotations;
using DAL.Models.Entities.Enums;

namespace DAL.Models.ViewModels;

public class MaintenanceRecordFormViewModel
{
    public int ToolId { get; set; }

    [Required]
    public MaintenanceTypeEnum Type { get; set; } = MaintenanceTypeEnum.Inspection;

    public string? Description { get; set; }

    [Required]
    public MaintenanceStatusEnum Status { get; set; } = MaintenanceStatusEnum.Scheduled;

    public string? Notes { get; set; }

    [Range(0, 10000)]
    public decimal? DurationHours { get; set; }

    public DateTime? ScheduledAt { get; set; }
}

public class ExternalRepairFormViewModel
{
    public int ToolId { get; set; }

    [Required, StringLength(200)]
    public string RepairCenterName { get; set; } = string.Empty;

    public string? ProblemDescription { get; set; }

    public DateOnly? SentAt { get; set; }

    public DateOnly? ExpectedReturnAt { get; set; }

    [Range(0, 1000000)]
    public decimal? Cost { get; set; }

    [Required]
    public ExternalRepairStatusEnum Status { get; set; } = ExternalRepairStatusEnum.Inspecting;
}
