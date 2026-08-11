using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>An audience granted to a petitioner at the lord's seat.</summary>
    public class BaronAudience
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        /// <summary>Subject of the audience, e.g. "Plea for harvest help".</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Name of the petitioner (the person seeking audience).</summary>
        public string PetitionerName { get; set; } = string.Empty;

        /// <summary><see cref="DA_Common.Barony.BaronAudienceKind"/> — Audience (default) or Council.</summary>
        public string Kind { get; set; } = DA_Common.Barony.BaronAudienceKind.Audience;

        /// <summary><see cref="DA_Common.Barony.BaronAudienceStatus"/>.</summary>
        public string Status { get; set; } = DA_Common.Barony.BaronAudienceStatus.Scheduled;

        /// <summary>Turn this audience belongs to (current schedule / archive folder).</summary>
        public int TurnNumber { get; set; }

        /// <summary>When this audience was spawned by deferring another; null if original.</summary>
        public int? ContinuedFromAudienceId { get; set; }

        /// <summary>MG closing summary (archive). Filled on Resolve; optional on Dismiss.</summary>
        public string GmSummary { get; set; } = string.Empty;

        /// <summary>Free-text outcome of resources / PPB gained (archive). Structured hooks later.</summary>
        public string OutcomeNotes { get; set; } = string.Empty;

        /// <summary>PPB granted through this audience (JSON <see cref="DA_Common.Barony.PpbVector"/>).</summary>
        public string AdditiveJson { get; set; } = "[]";

        /// <summary>PPB percent granted through this audience.</summary>
        public string PercentJson { get; set; } = "[]";

        /// <summary>Prestige granted through this audience (Baron Card PHP).</summary>
        public int Prestige { get; set; }

        /// <summary>Honor granted through this audience (Baron Card PHP).</summary>
        public int Honor { get; set; }

        /// <summary>Fear granted through this audience (Baron Card PHP).</summary>
        public int Fear { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }

        public List<BaronAudienceExchange> Exchanges { get; set; } = new();
    }

    /// <summary>One spoken turn in an audience (petitioner or baron).</summary>
    public class BaronAudienceExchange
    {
        [Key]
        public int Id { get; set; }
        public int AudienceId { get; set; }

        /// <summary>Plain text body.</summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>True = GM side (petitioner / other NPC); false = baron.</summary>
        public bool IsFromPetitioner { get; set; }

        /// <summary>
        /// Optional speaker label. When empty: petitioner name (GM side) or "Lord" (baron).
        /// Used when the GM speaks as a character other than the petitioner.
        /// </summary>
        public string? SpeakerName { get; set; }

        /// <summary>System line recording a PPB grant (speaker label: Resource change).</summary>
        public bool IsResourceChange { get; set; }

        /// <summary>PPB delta for resource-change lines (JSON <see cref="DA_Common.Barony.PpbVector"/>).</summary>
        public string AdditiveJson { get; set; } = "[]";

        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }

        public int TurnNumber { get; set; }
        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public BaronAudience? Audience { get; set; }
    }
}
