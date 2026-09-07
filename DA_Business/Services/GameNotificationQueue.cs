using System.Globalization;
using System.Threading.Channels;
using DA_Business.Services.Interfaces;
using DA_Common.Notifications;
using Microsoft.Extensions.Logging;

namespace DA_Business.Services
{
    /// <inheritdoc cref="IGameNotificationQueue"/>
    public class GameNotificationQueue : IGameNotificationQueue
    {
        /// <summary>
        /// Generous enough for a turn resolve that notifies every player at once, small enough
        /// that a wedged worker cannot grow the queue without bound.
        /// </summary>
        private const int Capacity = 500;

        private readonly Channel<GameNotification> _channel =
            Channel.CreateBounded<GameNotification>(new BoundedChannelOptions(Capacity)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
            });

        private readonly ILogger<GameNotificationQueue> _logger;

        public GameNotificationQueue(ILogger<GameNotificationQueue> logger)
        {
            _logger = logger;
        }

        public void Enqueue(GameNotification notification)
        {
            if (notification is null)
                return;

            // The worker runs without request localization, so remember the language here.
            if (string.IsNullOrWhiteSpace(notification.CultureName))
                notification = notification with { CultureName = CultureInfo.CurrentUICulture.Name };

            if (!_channel.Writer.TryWrite(notification))
            {
                _logger.LogWarning(
                    "Notification queue is full — dropped {Kind}.",
                    notification.GetType().Name);
            }
        }

        public IAsyncEnumerable<GameNotification> ReadAllAsync(CancellationToken cancellationToken) =>
            _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
