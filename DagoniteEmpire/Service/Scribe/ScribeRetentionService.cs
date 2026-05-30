using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DagoniteEmpire.Service.Scribe
{
    /// <summary>
    /// Periodically deletes Scribe conversations whose LastMessageAt (or StartedAt) is older
    /// than the configured retention window. Default retention: 14 days.
    /// Cascading FK removes the related ScribeMessages.
    /// </summary>
    public class ScribeRetentionService : BackgroundService
    {
        private static readonly TimeSpan RetentionWindow = TimeSpan.FromDays(14);
        private static readonly TimeSpan SweepInterval = TimeSpan.FromHours(6);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScribeRetentionService> _logger;

        public ScribeRetentionService(
            IServiceScopeFactory scopeFactory,
            ILogger<ScribeRetentionService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Slight startup delay so we don't compete with app warm-up
            try { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SweepAsync(stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scribe retention sweep failed");
                }

                try { await Task.Delay(SweepInterval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task SweepAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var factory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

            await using var ctx = await factory.CreateDbContextAsync(ct);

            var cutoff = DateTime.UtcNow - RetentionWindow;

            var stale = await ctx.ScribeConversations
                .Where(c => (c.LastMessageAt ?? c.StartedAt) < cutoff)
                .ToListAsync(ct);

            if (stale.Count == 0)
            {
                _logger.LogDebug("Scribe retention: nothing to purge (cutoff={Cutoff:o})", cutoff);
                return;
            }

            ctx.ScribeConversations.RemoveRange(stale);
            var deleted = await ctx.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Scribe retention: purged {Count} conversation(s) older than {Days} days",
                deleted, RetentionWindow.TotalDays);
        }
    }
}
