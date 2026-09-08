using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.Notifications
{
    /// <summary>
    /// One browser push endpoint (device) registered by an installed PWA. Rows are keyed by the
    /// push service <see cref="Endpoint"/> URL, so re-subscribing the same device updates in place.
    /// Deleted when the push service answers 404 / 410 Gone.
    /// </summary>
    public class WebPushSubscription
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Identity user id (AspNetUsers.Id) — not a character id.</summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Push service URL issued by the browser; unique per device.</summary>
        [Required]
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>Client public key (base64url) used to encrypt the payload.</summary>
        [Required]
        public string P256dh { get; set; } = string.Empty;

        /// <summary>Client auth secret (base64url).</summary>
        [Required]
        public string Auth { get; set; } = string.Empty;

        /// <summary>
        /// JSON array of opted-in <see cref="DA_Common.Notifications.NotificationTopic"/> keys.
        /// Null or empty-looking storage historically meant every topic.
        /// An explicit JSON empty array means mute everything on this device.
        /// </summary>
        public string? TopicsJson { get; set; }

        /// <summary>Browser user agent, to tell devices apart in the UI.</summary>
        public string? UserAgent { get; set; }

        public DateTime CreatedUtc { get; set; }

        /// <summary>Last successful delivery; helps spot dead devices.</summary>
        public DateTime? LastSentUtc { get; set; }
    }
}
