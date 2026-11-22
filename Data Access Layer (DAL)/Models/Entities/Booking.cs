using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class Booking
{
    public int BookingId { get; set; }

    public int ToolId { get; set; }

    public int BorrowerUserId { get; set; }

    public int LenderUserId { get; set; }

    public DateOnly PickupDate { get; set; }

    public DateOnly ReturnDate { get; set; }

    public TimeOnly? PickupTime { get; set; }

    public TimeOnly? ReturnTime { get; set; }

    public string? PickupLocation { get; set; }

    public string? ReturnLocation { get; set; }

    public string? PickupNotes { get; set; }

    public decimal RentalAmount { get; set; }

    public decimal InsuranceAmount { get; set; }

    public decimal PlatformFee { get; set; }

    public decimal DepositAmount { get; set; }

    public DepositStatusEnum DepositStatus { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal WalletCreditApplied { get; set; }

    public decimal FinalAmountDue { get; set; }

    public BookingStatusEnum Status { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public int? PromotionId { get; set; }

    public decimal PromotionDiscount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User BorrowerUser { get; set; } = null!;

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();

    public virtual Deposit? Deposit { get; set; }

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual ICollection<Handover> Handovers { get; set; } = new List<Handover>();

    public virtual ICollection<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();

    public virtual ICollection<LateReturn> LateReturns { get; set; } = new List<LateReturn>();

    public virtual User LenderUser { get; set; } = null!;

    public virtual Promotion? Promotion { get; set; }

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual Tool Tool { get; set; } = null!;

    public virtual ICollection<ToolAvailability> ToolAvailabilities { get; set; } = new List<ToolAvailability>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
