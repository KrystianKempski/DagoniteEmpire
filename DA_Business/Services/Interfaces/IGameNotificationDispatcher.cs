using DA_Common.Notifications;

namespace DA_Business.Services.Interfaces
{
    /// <summary>
    /// Turns a queued <see cref="GameNotification"/> into an actual push: resolves who should
    /// hear about it, writes the text, and hands it to the push service.
    /// </summary>
    public interface IGameNotificationDispatcher
    {
        /// <summary>Number of devices that accepted the push (0 when nobody had to be told).</summary>
        Task<int> Dispatch(GameNotification notification, CancellationToken cancellationToken = default);
    }
}
