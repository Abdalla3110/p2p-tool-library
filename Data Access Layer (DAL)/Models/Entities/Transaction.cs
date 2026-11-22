using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int UserId { get; set; }

    public int? BookingId { get; set; }

    public TransactionTypeEnum Type { get; set; }

    public decimal Amount { get; set; }

    public decimal? BalanceAfter { get; set; }

    public TransactionStatusEnum Status { get; set; }

    public string? Description { get; set; }

    public string? Reference { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual User User { get; set; } = null!;
}
