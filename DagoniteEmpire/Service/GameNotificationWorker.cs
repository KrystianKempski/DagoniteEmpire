using DA_Business.Services.Interfaces;

namespace DagoniteEmpire.Service
{
    /// <summary>
    /// Drains the game notification queue and delivers each event as a web push. Runs outside the
    /// request that raised the event, so a slow or unreachable push service can never delay a
    /// post, a letter or a turn resolve.
    /// </summary>
    public class GameNotificationWorker : BackgroundService
    {
        private readonly IGameNotificationQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GameNotificationWorker> _logger;

        public GameNotificationWorker(
            IGameNotificationQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<GameNotificationWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var notification in _queue.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var dispatcher = scope.ServiceProvider
                            .GetRequiredService<IGameNotificationDispatcher>();

                        await dispatcher.Dispatch(notification, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        // One bad event must not take the worker down for the rest of the app's life.
                        _logger.LogError(
                            ex,
                            "Could not deliver notification {Kind}.",
                            notification.GetType().Name);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Shutting down.
            }
        }
    }
}
