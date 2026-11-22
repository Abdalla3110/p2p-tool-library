using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class Tool
{
    public int ToolId { get; set; }

    public int OwnerUserId { get; set; }

    public int CategoryId { get; set; }

    public int ZoneId { get; set; }

    public string Name { get; set; } = null!;

    public string? Brand { get; set; }

    public string? Description { get; set; }

    public ToolConditionEnum Condition { get; set; }

    public ToolStatusEnum Status { get; set; }

    public decimal PricePerHour { get; set; }

    public decimal PricePerDay { get; set; }

    public decimal? PricePerWeek { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal InsurancePercentage { get; set; }

    public int MaxBorrowDays { get; set; }

    public int CleaningBufferHours { get; set; }

    public int? BatteryHealthPercentage { get; set; }

    public int UsageHours { get; set; }

    public int TotalRentals { get; set; }

    public int? WeeklyMaintenanceThreshold { get; set; }

    public DateOnly? LastServiceAt { get; set; }

    public DateOnly? WarrantyExpiryDate { get; set; }

    public DateOnly? SafetyCertificateExpiry { get; set; }

    public string? SafetyCertificateUrl { get; set; }

    public string? UserManualUrl { get; set; }

    public string? SafetyVideoUrl { get; set; }

    public decimal RatingAverage { get; set; }

    public int ReviewCount { get; set; }

    public bool IsHighValue { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ToolCategory Category { get; set; } = null!;

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();

    public virtual ICollection<Disposal> Disposals { get; set; } = new List<Disposal>();

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual ICollection<ExternalRepair> ExternalRepairs { get; set; } = new List<ExternalRepair>();

    public virtual ICollection<Handover> Handovers { get; set; } = new List<Handover>();

    public virtual ICollection<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();

    public virtual ICollection<InventoryAudit> InventoryAudits { get; set; } = new List<InventoryAudit>();

    public virtual ICollection<LateReturn> LateReturns { get; set; } = new List<LateReturn>();

    public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();

    public virtual User OwnerUser { get; set; } = null!;

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<ToolAvailability> ToolAvailabilities { get; set; } = new List<ToolAvailability>();

    public virtual ICollection<ToolPhoto> ToolPhotos { get; set; } = new List<ToolPhoto>();

    public virtual Zone Zone { get; set; } = null!;
}
