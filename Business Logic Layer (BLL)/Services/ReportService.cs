using BLL.Interfaces;
using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    public ReportService(AppDbContext db) => _db = db;

    public async Task<RevenueSummary> GetRevenueSummaryAsync() => new()
    {
        TotalUsers = await _db.Users.CountAsync(),
        ActiveUsers = await _db.Users.CountAsync(u => u.IsActive),
        TotalTools = await _db.Tools.CountAsync(),
        TotalBookings = await _db.Bookings.CountAsync(),
        TotalRevenue = await _db.Bookings.Where(b => b.Status == BookingStatusEnum.Completed)
            .SumAsync(b => (decimal?)b.RentalAmount) ?? 0,
    };

    public async Task<HomepageStats> GetHomepageStatsAsync() => new()
    {
        TotalTools = await _db.Tools.CountAsync(t => t.IsActive),
        TotalUsers = await _db.Users.CountAsync(u => u.IsActive),
        TotalBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatusEnum.Completed),
    };

    public async Task<List<AdminActivityRow>> GetRecentActivityAsync(int take = 50) =>
        await _db.AuditLogs.OrderByDescending(a => a.CreatedAt).Take(take)
            .Select(a => new AdminActivityRow
            {
                At = a.CreatedAt,
                Description = $"{a.Action} on {a.EntityType} #{a.EntityId} by user {a.UserId}",
            }).ToListAsync();
}
