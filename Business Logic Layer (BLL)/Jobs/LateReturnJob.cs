using BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BLL.Jobs;

/// <summary>
/// Scans for overdue rentals every 30 minutes and escalates:
/// Warning → FineApplied (wallet debit) → AccountFrozen.
/// Delegates to <see cref="IPenaltyService"/>.
/// </summary>
public class LateReturnJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    private readonly IServiceProvider _services;
    private readonly ILogger<LateReturnJob> _logger;

    public LateReturnJob(IServiceProvider services, ILogger<LateReturnJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);
                using var scope = _services.CreateScope();
                var penalties = scope.ServiceProvider.GetRequiredService<IPenaltyService>();
                await penalties.ProcessLateReturnsAsync();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LateReturnJob scan failed");
            }
        }
    }
}
