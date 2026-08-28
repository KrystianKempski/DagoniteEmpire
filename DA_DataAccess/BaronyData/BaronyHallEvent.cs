using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>
    /// MG narrative event on the Audience Hall: long proclamation text + mandatory baron reply.
    /// </summary>
    public class BaronyHallEvent
    {
        [Key]
        public int Id { get; set; }

        public int BaronyId { get; set; }

        public string Title { get; set; } = string.Empty;

        /// <summary>Plain-text body (may be very long). Supports markdown.</summary>
        public string Body { get; set; } = string.Empty;

        /// <summary><see cref="DA_Common.Barony.BaronyHallEventTriggerKind"/>.</summary>
        public string TriggerKind { get; set; } = DA_Common.Barony.BaronyHallEventTriggerKind.Manual;

        /// <summary><see cref="DA_Common.Barony.BaronyHallEventStatus"/>.</summary>
        public string Status { get; set; } = DA_Common.Barony.BaronyHallEventStatus.Pending;

        public int TurnNumber { get; set; }

        /// <summary>wwwroot-relative icon path.</summary>
        public string IconPath { get; set; } = "icons/vertical-banner.svg";

        /// <summary>Baron's single reply; empty until acknowledged.</summary>
        public string ResponseBody { get; set; } = string.Empty;

        /// <summary>When set, the baron's reply spawned this audience for continued dialogue.</summary>
        public int? AudienceId { get; set; }

        /// <summary>Optional linked campaign chapter (Make chapter).</summary>
        public int? ChapterId { get; set; }

        /// <summary>When set, event stays <see cref="BaronyHallEventStatus.Scheduled"/> until this turn, then becomes Pending.</summary>
        public int? PublishAtTurn { get; set; }

        /// <summary>PPB applied as a domain event when the baron acknowledges (JSON PpbVector).</summary>
        public string ConsequenceAdditiveJson { get; set; } = "{}";

        /// <summary>PPB percent applied when acknowledged (JSON PpbVector).</summary>
        public string ConsequencePercentJson { get; set; } = "{}";

        /// <summary>End turn for the consequence domain event. Null = ongoing.</summary>
        public int? ConsequenceEndTurn { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? AcknowledgedAtUtc { get; set; }
    }
}
