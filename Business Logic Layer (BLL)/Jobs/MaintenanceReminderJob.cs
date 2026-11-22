using DAL.Data;
using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BLL.Jobs;

/// <summary>
/// Daily scan: tools that haven't been serviced in 90+ days or have low battery
/// health get an owner notification (deduplicated to once per week).
/// </summary>
public class MaintenanceReminderJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private const int ServiceThresholdDays = 90;
    private const int BatteryThreshold = 30;

    private readonly IServiceProvider _services;
    private readonly ILogger<MaintenanceReminderJob> _logger;

    public MaintenanceReminderJob(IServiceProvider services, ILogger<MaintenanceReminderJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // initial delay so it doesn't fire at startup
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanAsync(stoppingToken);
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MaintenanceReminderJob scan failed");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task ScanAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var settings = await db.PlatformSettings.FirstOrDefaultAsync(ct);
        if (settings?.WeeklyMaintenanceRemindersEnabled == false) return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threshold = today.AddDays(-ServiceThresholdDays);
        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);

        var tools = await db.Tools
            .Where(t => t.IsActive && t.Status != ToolStatusEnum.Unfit &&
                (t.LastServiceAt == null || t.LastServiceAt < threshold ||
                 (t.BatteryHealthPercentage != null && t.BatteryHealthPercentage < BatteryThreshold)))
            .ToListAsync(ct);

        foreach (var t in tools)
        {
            // dedupe: skip if we already reminded this owner about this tool in the last 7 days
            var already = await db.Notifications.AnyAsync(n =>
                n.UserId == t.OwnerUserId && n.Type == "Maintenance" &&
                n.Message != null && n.Message.Contains(t.Name) && n.CreatedAt > weekAgo, ct);
            if (already) continue;

            var reason = t.LastServiceAt == null || t.LastServiceAt < threshold
                ? $"hasn't been serviced since {(t.LastServiceAt?.ToString("yyyy-MM-dd") ?? "ever")}"
                : $"battery health is at {t.BatteryHealthPercentage}%";

            db.Notifications.Add(new Notification
            {
                UserId = t.OwnerUserId, Type = "Maintenance",
                Title = "Maintenance reminder",
                Message = $"Your tool \"{t.Name}\" {reason}. Consider scheduling a check-up.",
                Priority = NotificationPriorityEnum.Normal, CreatedAt = now,
            });
        }
        await db.SaveChangesAsync(ct);
    }
}
