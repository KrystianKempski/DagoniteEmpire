using DA_Common.Notifications;

namespace DA_Business.Services.Interfaces
{
    /// <summary>
    /// Hand-off point between gameplay code and push delivery. Raising an event must never slow
    /// down or break a save, so <see cref="Enqueue"/> only drops a descriptor into a bounded
    /// queue; a background worker resolves recipients and talks to the push services.
    /// </summary>
    public interface IGameNotificationQueue
    {
        /// <summary>
        /// Queues an event for background delivery. Never throws and never blocks: if the queue
        /// is saturated the event is dropped, because a missed notification is preferable to a
        /// stalled turn resolve.
        /// </summary>
        void Enqueue(GameNotification notification);

        /// <summary>Consumed by the background worker.</summary>
        IAsyncEnumerable<GameNotification> ReadAllAsync(CancellationToken cancellationToken);
    }
}
