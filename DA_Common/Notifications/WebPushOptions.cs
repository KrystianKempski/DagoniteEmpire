namespace DA_Common.Notifications
{
    /// <summary>
    /// VAPID credentials for Web Push, bound from the "WebPush" configuration section.
    /// Keep <see cref="PrivateKey"/> in user secrets / environment variables, never in appsettings.json.
    /// </summary>
    public class WebPushOptions
    {
        public const string SectionName = "WebPush";

        /// <summary>VAPID public key (base64url). Safe to hand to the browser.</summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>VAPID private key (base64url). Secret.</summary>
        public string PrivateKey { get; set; } = string.Empty;

        /// <summary>VAPID subject — a "mailto:" or "https://" contact required by push services.</summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>Both keys and a subject present, so notifications can actually be sent.</summary>
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(PublicKey)
            && !string.IsNullOrWhiteSpace(PrivateKey)
            && !string.IsNullOrWhiteSpace(Subject)
            && !PublicKey.StartsWith("SET_IN_", StringComparison.OrdinalIgnoreCase)
            && !PrivateKey.StartsWith("SET_IN_", StringComparison.OrdinalIgnoreCase);
    }
}
