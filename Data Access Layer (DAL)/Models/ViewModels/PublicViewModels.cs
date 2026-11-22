using System.ComponentModel.DataAnnotations;
using DAL.Models.Entities;

namespace DAL.Models.ViewModels;

public class ToolCardViewModel
{
    public int ToolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = "fas fa-tools";
    public string ZoneName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsHighValue { get; set; }
    public decimal RatingAverage { get; set; }
    public int ReviewCount { get; set; }
    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal? PricePerWeek { get; set; }
    public decimal DepositAmount { get; set; }
    public string OwnerName { get; set; } = string.Empty;

    public static ToolCardViewModel FromTool(Tool t) => new()
    {
        ToolId = t.ToolId,
        Name = t.Name,
        Brand = t.Brand,
        Description = t.Description,
        CategoryName = t.Category?.Name ?? string.Empty,
        CategoryIcon = t.Category?.Icon ?? "fas fa-tools",
        ZoneName = t.Zone?.Name ?? string.Empty,
        Status = t.Status.ToString(),
        IsHighValue = t.IsHighValue,
        RatingAverage = t.RatingAverage,
        ReviewCount = t.ReviewCount,
        PricePerHour = t.PricePerHour,
        PricePerDay = t.PricePerDay,
        PricePerWeek = t.PricePerWeek,
        DepositAmount = t.DepositAmount,
        OwnerName = t.OwnerUser?.FullName ?? string.Empty,
    };
}

public class ToolDetailsViewModel
{
    public ToolCardViewModel Card { get; set; } = new();
    public string Condition { get; set; } = string.Empty;
    public decimal InsurancePercentage { get; set; }
    public int MaxBorrowDays { get; set; }
    public int CleaningBufferHours { get; set; }
    public int? BatteryHealthPercentage { get; set; }
    public DateOnly? WarrantyExpiryDate { get; set; }
    public DateOnly? SafetyCertificateExpiry { get; set; }
    public string? SafetyCertificateUrl { get; set; }
    public string? UserManualUrl { get; set; }
    public string? SafetyVideoUrl { get; set; }
    public DateOnly? LastServiceAt { get; set; }
    public int TotalRentals { get; set; }
    public int UsageHours { get; set; }
    public string OwnerTier { get; set; } = string.Empty;
    public int OwnerTrustScore { get; set; }
    public int OwnerUserId { get; set; }
    public List<MaintenanceRecord> RecentMaintenance { get; set; } = new();
    public List<ToolAvailability> UpcomingAvailability { get; set; } = new();
    public List<ToolPhoto> Photos { get; set; } = new();
    public bool IsOwner { get; set; }
}

public class BrowseToolsViewModel
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? ZoneId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string Sort { get; set; } = "rating";
    public DateOnly? AvailableFrom { get; set; }
    public DateOnly? AvailableTo { get; set; }

    [Range(1, 10000)]
    public int Page { get; set; } = 1;

    public List<ToolCardViewModel> Tools { get; set; } = new();
    public List<ToolCategory> Categories { get; set; } = new();
    public List<Zone> Zones { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class BookingDetailsViewModel
{
    public Booking Booking { get; set; } = null!;
    public Tool Tool { get; set; } = null!;
    public User Borrower { get; set; } = null!;
    public User Lender { get; set; } = null!;
    public bool CanCancel { get; set; }
    public bool HasRated { get; set; }
    public List<Handover> Handovers { get; set; } = new();
}
