namespace DA_Common.Barony
{
    /// <summary>Lifecycle of a baronial audience with a petitioner.</summary>
    public readonly struct BaronAudienceStatus
    {
        public const string Scheduled = "Scheduled";
        public const string InProgress = "InProgress";
        public const string Deferred = "Deferred";
        public const string Resolved = "Resolved";
        public const string Dismissed = "Dismissed";

        public static readonly string[] All =
        {
            Scheduled, InProgress, Deferred, Resolved, Dismissed,
        };

        public static readonly string[] Active =
        {
            Scheduled, InProgress, Deferred,
        };

        public static readonly string[] Archived =
        {
            Resolved, Dismissed, Deferred,
        };

        public static bool IsActive(string? status) =>
            Active.Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));

        public static bool IsClosed(string? status) =>
            string.Equals(status, Resolved, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, Dismissed, StringComparison.OrdinalIgnoreCase);

        public static string DisplayName(string? status) => status?.Trim() switch
        {
            Scheduled => "Scheduled",
            InProgress => "In progress",
            Deferred => "Deferred",
            Resolved => "Resolved",
            Dismissed => "Dismissed",
            _ => status ?? "Unknown",
        };
    }

    /// <summary>How audience PPB splits into Domain Events vs Project Summary.</summary>
    public static class BaronAudiencePpb
    {
        public const string SummaryRowName = "Audiences";

        public static readonly Ppb[] NonCumulativeKeys =
        {
            Ppb.Economy, Ppb.Loyalty, Ppb.Stability, Ppb.Law, Ppb.Corruption,
        };

        public static bool ContributesToTurn(int audienceTurn, string? status, int currentTurn)
        {
            if (audienceTurn != currentTurn)
                return false;
            return !string.Equals(status, BaronAudienceStatus.Dismissed, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Lifetime PHP on Baron Card: active + resolved grants.
        /// Deferred (superseded) and dismissed audiences are excluded so defer chains do not double-count.
        /// </summary>
        public static bool ContributesToPhp(string? status) =>
            !string.Equals(status, BaronAudienceStatus.Deferred, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(status, BaronAudienceStatus.Dismissed, StringComparison.OrdinalIgnoreCase);

        public static PpbVector SliceNonCumulative(PpbVector? source)
        {
            var result = new PpbVector();
            if (source is null)
                return result;
            foreach (var key in NonCumulativeKeys)
                result[key] = source[key];
            return result;
        }

        public static PpbVector SliceCumulative(PpbVector? source)
        {
            var result = new PpbVector();
            if (source is null)
                return result;
            foreach (var info in PpbCatalog.All)
            {
                if (info.IsCumulative)
                    result[info.Key] = source[info.Key];
            }
            return result;
        }
    }

    /// <summary>
    /// Campaign chapter “Barony resources” grants — same split as audiences:
    /// non-cumulative → Domain Panel Events row, cumulative → Resources balance, PHP → From Adventures.
    /// </summary>
    public static class BaronAdventurePpb
    {
        public const string SummaryRowName = "Adventures";

        /// <summary>
        /// Internal BaronPhpSource key folded into the system “From Adventures” row (not shown as custom).
        /// </summary>
        public const string PhpSourceKey = "__CampaignAdventuresPhp__";

        public static bool IsAdventureEvent(string? name) =>
            string.Equals(name, SummaryRowName, StringComparison.OrdinalIgnoreCase);

        public static bool IsAdventurePhpSource(string? source) =>
            string.Equals(source, PhpSourceKey, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Chapter naming for audience → campaign thread links.</summary>
    public static class BaronAudienceChapter
    {
        /// <summary>
        /// Detailed chapter title: <c>Audience {year}, {season}, {audience title}</c>.
        /// Fall is shown as Autumn to match Domain Panel labels.
        /// </summary>
        public static string FormatName(int year, string? season, string? audienceTitle)
        {
            var seasonLabel = BaronyCalendarFormulas.NormalizeSeason(season) switch
            {
                "Fall" => "Autumn",
                var s => s,
            };
            var title = string.IsNullOrWhiteSpace(audienceTitle) ? "Untitled" : audienceTitle.Trim();
            return $"Audience {year}, {seasonLabel}, {title}";
        }
    }
}
