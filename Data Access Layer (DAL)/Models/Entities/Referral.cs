using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class Referral
{
    public int ReferralId { get; set; }

    public int ReferrerUserId { get; set; }

    public int ReferredUserId { get; set; }

    public string ReferralCode { get; set; } = null!;

    public decimal CreditAmount { get; set; }

    public ReferralStatusEnum Status { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User ReferredUser { get; set; } = null!;

    public virtual User ReferrerUser { get; set; } = null!;
}
