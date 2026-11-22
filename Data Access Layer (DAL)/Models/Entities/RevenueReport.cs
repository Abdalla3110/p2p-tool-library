using System;
using System.Collections.Generic;

namespace DAL.Models.Entities;

public partial class RevenueReport
{
    public int RevenueReportId { get; set; }

    public DateOnly PeriodMonth { get; set; }

    public string Source { get; set; } = null!;

    public int TransactionCount { get; set; }

    public decimal Amount { get; set; }

    public decimal? SharePercentage { get; set; }

    public DateTime CreatedAt { get; set; }
}
