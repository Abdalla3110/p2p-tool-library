using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class Disposal
{
    public int DisposalId { get; set; }

    public int ToolId { get; set; }

    public DisposalReasonEnum Reason { get; set; }

    public string? ReasonDescription { get; set; }

    public string? RecycleCenter { get; set; }

    public string? Photos { get; set; }

    public bool CancelFutureBookings { get; set; }

    public DateOnly? DisposedAt { get; set; }

    public DisposalStatusEnum Status { get; set; }

    public int? ApprovedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ApprovedByUser { get; set; }

    public virtual Tool Tool { get; set; } = null!;
}
