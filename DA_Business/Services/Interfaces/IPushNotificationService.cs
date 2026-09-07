using DA_Models.NotificationModels;

namespace DA_Business.Services.Interfaces
{
    /// <summary>
    /// Web Push (VAPID) delivery to installed PWAs. Sending is best-effort: a failing device
    /// never breaks the caller, and endpoints reported as gone are pruned automatically.
    /// </summary>
    public interface IPushNotificationService
    {
        /// <summary>VAPID keys are present, so notifications can be sent.</summary>
        bool IsConfigured { get; }

        /// <summary>VAPID public key handed to the browser when subscribing.</summary>
        string PublicKey { get; }

        /// <summary>Stores (or refreshes) a device subscription for the given Identity user.</summary>
        Task SaveSubscription(string userId, WebPushSubscriptionDTO subscription, string? userAgent);

        /// <summary>Forgets a single device by its push endpoint.</summary>
        Task RemoveSubscription(string endpoint);

        /// <summary>Number of devices currently subscribed for this user.</summary>
        Task<int> CountSubscriptions(string userId);

        /// <summary>
        /// Sends to every device of one user. Returns how many devices accepted the push.
        /// <paramref name="topic"/> filters devices by opted-in category.
        /// </summary>
        Task<int> SendToUser(string userId, PushNotificationDTO payload, string? topic = null);

        /// <summary>Sends to every device of several users, skipping duplicates.</summary>
        Task<int> SendToUsers(IEnumerable<string> userIds, PushNotificationDTO payload, string? topic = null);
    }
}
