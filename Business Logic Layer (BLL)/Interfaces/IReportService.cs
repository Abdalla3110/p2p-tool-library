namespace BLL.Interfaces;

public class RevenueSummary
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalTools { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
}

public interface IReportService
{
    Task<RevenueSummary> GetRevenueSummaryAsync();
    Task<HomepageStats> GetHomepageStatsAsync();
    Task<List<AdminActivityRow>> GetRecentActivityAsync(int take = 50);
}

public class HomepageStats
{
    public int TotalTools { get; set; }
    public int TotalUsers { get; set; }
    public int TotalBookings { get; set; }
}

public class AdminActivityRow
{
    public DateTime At { get; set; }
    public string Description { get; set; } = "";
}
