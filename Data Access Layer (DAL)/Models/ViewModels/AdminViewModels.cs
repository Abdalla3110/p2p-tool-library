using System.ComponentModel.DataAnnotations;
using DAL.Models.Entities.Enums;

namespace DAL.Models.ViewModels;

public class PromotionFormViewModel
{
    public int PromotionId { get; set; }

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DiscountTypeEnum DiscountType { get; set; } = DiscountTypeEnum.Percentage;

    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }

    [Range(0, 100000)]
    public decimal? DiscountAmount { get; set; }

    public int? CategoryId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Range(1, 100000)]
    public int? MaxUses { get; set; }

    [Required]
    public PromotionStatusEnum Status { get; set; } = PromotionStatusEnum.Active;
}

public class AdminUserFormViewModel
{
    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }

    public int? ZoneId { get; set; }
}
