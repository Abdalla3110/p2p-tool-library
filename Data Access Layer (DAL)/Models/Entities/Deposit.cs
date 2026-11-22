using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class Deposit
{
    public int DepositId { get; set; }

    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public DepositStatusEnum Status { get; set; }

    public DateTime HeldAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public int? ReleasedToUserId { get; set; }

    public string? ForfeitReason { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User? ReleasedToUser { get; set; }
}
