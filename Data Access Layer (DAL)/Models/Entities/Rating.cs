using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class Rating
{
    public int RatingId { get; set; }

    public int BookingId { get; set; }

    public int? ToolId { get; set; }

    public int RaterUserId { get; set; }

    public int? RatedUserId { get; set; }

    public int Rating1 { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User? RatedUser { get; set; }

    public virtual User RaterUser { get; set; } = null!;

    public virtual Tool? Tool { get; set; }
}
