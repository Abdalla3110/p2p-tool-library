using System.ComponentModel.DataAnnotations;
using DAL.Models.Entities;
using DAL.Models.Entities.Enums;

namespace DAL.Models.ViewModels;

public class DisputeFormViewModel
{
    public int BookingId { get; set; }

    [Required]
    public DisputeTypeEnum Type { get; set; } = DisputeTypeEnum.Damage;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;
}

public class InsuranceClaimFormViewModel
{
    public int BookingId { get; set; }
    public int ToolId { get; set; }

    [Range(0, 1000000)]
    public decimal? ToolValue { get; set; }

    public DateOnly? IncidentDate { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? DamagePhotoUrl { get; set; }
}

public class ProfileViewModel
{
    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    public int? ZoneId { get; set; }

    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool SmsNotificationsEnabled { get; set; }
    public bool PushNotificationsEnabled { get; set; } = true;

    public static ProfileViewModel FromUser(DAL.Models.Entities.User u) => new()
    {
        FullName = u.FullName,
        Phone = u.Phone,
        Bio = u.Bio,
        AvatarUrl = u.AvatarUrl,
        ZoneId = u.ZoneId,
        EmailNotificationsEnabled = u.EmailNotificationsEnabled,
        SmsNotificationsEnabled = u.SmsNotificationsEnabled,
        PushNotificationsEnabled = u.PushNotificationsEnabled,
    };
}

public class ChangePasswordViewModel
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ToolFormViewModel
{
    public int ToolId { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Brand { get; set; }

    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int ZoneId { get; set; }

    [Required]
    public ToolConditionEnum Condition { get; set; } = ToolConditionEnum.Good;

    [Range(0, 100000)]
    public decimal PricePerHour { get; set; }

    [Range(0.01, 100000)]
    public decimal PricePerDay { get; set; }

    [Range(0, 1000000)]
    public decimal? PricePerWeek { get; set; }

    [Range(0, 1000000)]
    public decimal DepositAmount { get; set; }

    [Range(0, 100)]
    public decimal InsurancePercentage { get; set; } = 5;

    [Range(1, 90)]
    public int MaxBorrowDays { get; set; } = 7;

    [Range(0, 168)]
    public int CleaningBufferHours { get; set; } = 10;

    [Range(0, 100)]
    public int? BatteryHealthPercentage { get; set; }

    public DateOnly? WarrantyExpiryDate { get; set; }

    public bool IsHighValue { get; set; }

    public static ToolFormViewModel FromTool(Tool t) => new()
    {
        ToolId = t.ToolId,
        Name = t.Name,
        Brand = t.Brand,
        Description = t.Description,
        CategoryId = t.CategoryId,
        ZoneId = t.ZoneId,
        Condition = t.Condition,
        PricePerHour = t.PricePerHour,
        PricePerDay = t.PricePerDay,
        PricePerWeek = t.PricePerWeek,
        DepositAmount = t.DepositAmount,
        InsurancePercentage = t.InsurancePercentage,
        MaxBorrowDays = t.MaxBorrowDays,
        CleaningBufferHours = t.CleaningBufferHours,
        BatteryHealthPercentage = t.BatteryHealthPercentage,
        WarrantyExpiryDate = t.WarrantyExpiryDate,
        IsHighValue = t.IsHighValue,
    };
}
