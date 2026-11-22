using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class UserSuspension
{
    public int SuspensionId { get; set; }

    public int UserId { get; set; }

    public SuspensionTypeEnum Type { get; set; }

    public SuspensionReasonEnum Reason { get; set; }

    public string? ReasonDescription { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public SuspensionStatusEnum Status { get; set; }

    public DateTime? LiftedAt { get; set; }

    public int? LiftedByUserId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual User? LiftedByUser { get; set; }

    public virtual User User { get; set; } = null!;
}
