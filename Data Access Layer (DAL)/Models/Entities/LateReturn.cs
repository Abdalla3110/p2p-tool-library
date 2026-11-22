using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class LateReturn
{
    public int LateReturnId { get; set; }

    public int BookingId { get; set; }

    public int ToolId { get; set; }

    public int BorrowerUserId { get; set; }

    public DateOnly DueDate { get; set; }

    public DateOnly? ActualReturnDate { get; set; }

    public int DelayDays { get; set; }

    public int GraceHoursUsed { get; set; }

    public decimal PenaltyAmount { get; set; }

    public LateReturnStatusEnum Status { get; set; }

    public bool WarningSent { get; set; }

    public DateTime? WarningSentAt { get; set; }

    public bool AccountFrozen { get; set; }

    public DateTime? FrozenAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User BorrowerUser { get; set; } = null!;

    public virtual Tool Tool { get; set; } = null!;
}
