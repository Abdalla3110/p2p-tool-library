using System;
using System.Collections.Generic;

using DAL.Models.Entities.Enums;

namespace DAL.Models.Entities;

public partial class ToolAvailability
{
    public long AvailabilityId { get; set; }

    public int ToolId { get; set; }

    public DateOnly Date { get; set; }

    public AvailabilityStatusEnum Status { get; set; }

    public int? BookingId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Tool Tool { get; set; } = null!;
}
