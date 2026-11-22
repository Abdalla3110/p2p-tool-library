using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public DiscountTypeEnum DiscountType { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public decimal? DiscountAmount { get; set; }

    public int? CategoryId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? MaxUses { get; set; }

    public int UsedCount { get; set; }

    public PromotionStatusEnum Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ToolCategory? Category { get; set; }
}
