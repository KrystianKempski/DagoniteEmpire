using DA_Business.Services.Interfaces;
using DA_Common;

namespace DagoniteEmpire.Service
{
    /// <summary>
    /// Periodically purges abandoned "Try baron" demo sessions (and their cloned baronies)
    /// whose heartbeat has gone stale, so demo data never accumulates in the database.
    /// </summary>
    public class DemoSessionSweeper : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DemoSessionSweeper> _logger;

        public DemoSessionSweeper(IServiceScopeFactory scopeFactory, ILogger<DemoSessionSweeper> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Small delay so app startup / migrations finish first.
            try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var demo = scope.ServiceProvider.GetRequiredService<IDemoBaronyService>();
                    var removed = await demo.SweepExpiredAsync(SD.DemoSessionTtl);
                    if (removed > 0)
                        _logger.LogInformation("Demo sweeper purged {Count} expired demo session(s).", removed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Demo session sweep failed.");
                }

                try { await Task.Delay(Interval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
