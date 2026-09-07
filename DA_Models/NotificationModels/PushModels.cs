namespace DA_Models.NotificationModels
{
    /// <summary>Browser push subscription as posted by the service worker registration.</summary>
    public class WebPushSubscriptionDTO
    {
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>Client public key (base64url), from <c>subscription.keys.p256dh</c>.</summary>
        public string P256dh { get; set; } = string.Empty;

        /// <summary>Client auth secret (base64url), from <c>subscription.keys.auth</c>.</summary>
        public string Auth { get; set; } = string.Empty;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Endpoint)
            && !string.IsNullOrWhiteSpace(P256dh)
            && !string.IsNullOrWhiteSpace(Auth);
    }

    /// <summary>Payload delivered to the service worker's <c>push</c> handler.</summary>
    public class PushNotificationDTO
    {
        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        /// <summary>App-relative path opened when the notification is tapped.</summary>
        public string Url { get; set; } = "/";

        /// <summary>
        /// Groups notifications so a newer one replaces the previous of the same kind
        /// instead of stacking (e.g. one entry per chapter).
        /// </summary>
        public string? Tag { get; set; }
    }
}
