namespace DA_Common.Barony
{
    /// <summary>Lifecycle of a narrative hall event on the Audience Hall.</summary>
    public readonly struct BaronyHallEventStatus
    {
        public const string Scheduled = "Scheduled";
        public const string Pending = "Pending";
        public const string Acknowledged = "Acknowledged";
        public const string Converted = "Converted";
        public const string Archived = "Archived";

        public static readonly string[] ActiveForBaron = { Pending };
        public static readonly string[] EditableByMg = { Scheduled, Pending };

        public static bool IsScheduled(string? status) =>
            string.Equals(status, Scheduled, StringComparison.OrdinalIgnoreCase);

        public static bool IsPending(string? status) =>
            string.Equals(status, Pending, StringComparison.OrdinalIgnoreCase);

        public static bool IsAcknowledged(string? status) =>
            string.Equals(status, Acknowledged, StringComparison.OrdinalIgnoreCase);

        public static bool IsConverted(string? status) =>
            string.Equals(status, Converted, StringComparison.OrdinalIgnoreCase);

        public static bool IsArchived(string? status) =>
            string.Equals(status, Archived, StringComparison.OrdinalIgnoreCase);

        public static bool CanMgEdit(string? status) =>
            EditableByMg.Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));

        public static bool CanBaronRespond(string? status) => IsPending(status);

        public static string DisplayName(string? status) => status?.Trim() switch
        {
            Scheduled => "Scheduled",
            Pending => "Pending",
            Acknowledged => "Acknowledged",
            Converted => "Converted",
            Archived => "Archived",
            _ => status ?? "Unknown",
        };
    }

    /// <summary>When / why MG published the hall event (informational).</summary>
    public readonly struct BaronyHallEventTriggerKind
    {
        public const string Manual = "Manual";
        public const string CampaignStart = "CampaignStart";
        public const string TurnStart = "TurnStart";

        public static readonly string[] All = { Manual, CampaignStart, TurnStart };

        public static string Normalize(string? kind) => kind?.Trim() switch
        {
            CampaignStart => CampaignStart,
            TurnStart => TurnStart,
            _ => Manual,
        };

        public static string DisplayName(string? kind) => Normalize(kind) switch
        {
            CampaignStart => "Campaign start",
            TurnStart => "Turn start",
            _ => "Manual",
        };
    }

    /// <summary>Chapter naming for hall event → campaign thread links.</summary>
    public static class BaronyHallEventChapter
    {
        public static string FormatName(int year, string? season, string? eventTitle)
        {
            var seasonLabel = BaronyCalendarFormulas.NormalizeSeason(season) switch
            {
                "Fall" => "Autumn",
                var s => s,
            };
            var title = string.IsNullOrWhiteSpace(eventTitle) ? "Untitled" : eventTitle.Trim();
            return $"Event {year}, {seasonLabel}, {title}";
        }
    }

    /// <summary>Defaults when a hall event becomes a regular audience after the baron's reply.</summary>
    public static class BaronHallEventAudience
    {
        /// <summary>Stored in DB; localize in UI via <c>L["Hall event"]</c>.</summary>
        public const string PetitionerLabel = "Hall event";

        public const string LegacyPetitionerLabel = "The court";

        public static bool IsHallEventPetitioner(string? name) =>
            string.Equals(name, PetitionerLabel, StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, LegacyPetitionerLabel, StringComparison.OrdinalIgnoreCase);

        public static string FormatBaronSpeaker(string? characterName) =>
            string.IsNullOrWhiteSpace(characterName) ? "Lord" : $"Lord {characterName.Trim()}";
    }
}
