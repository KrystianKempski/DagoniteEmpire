namespace DA_Models.BaronyModels
{
    public class BaronLetterThreadDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int? RelationId { get; set; }
        public string CorrespondentName { get; set; } = string.Empty;
        public string? CorrespondentTitle { get; set; }
        public string? CorrespondentCategory { get; set; }

        public string ReplyRegion { get; set; } = DA_Common.Barony.BaronLetterReplyRegion.EasternMarch;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public List<BaronLetterMessageDTO> Messages { get; set; } = new();

        public int MessageCount => Messages.Count;

        public int? LastTurnNumber => Messages
            .Where(m => !m.IsDraft)
            .Select(m => (int?)m.TurnNumber)
            .DefaultIfEmpty(null)
            .Max();

        /// <summary>Turn used for folder grouping (latest delivered letter, else 0).</summary>
        public int ActivityTurn => LastTurnNumber ?? 0;

        public BaronLetterMessageDTO? LastDeliveredMessage => Messages
            .Where(m => !m.IsDraft)
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ThenByDescending(m => m.Day)
            .ThenByDescending(m => m.SortOrder)
            .ThenByDescending(m => m.Id)
            .FirstOrDefault();

        public bool HasUnreadForBaron => Messages.Any(m => m.HasUnreadForBaron);
        public bool HasUnreadForGm => Messages.Any(m => m.HasUnreadForGm);
    }

    /// <summary>Lightweight unread badge for the letters FAB (baron or GM).</summary>
    public class BaronLetterInboxBadgeDTO
    {
        public int UnreadCount { get; set; }
        public int? LatestThreadId { get; set; }
        public int? BaronyId { get; set; }
        public int? CharacterId { get; set; }
    }

    public class BaronLetterMessageDTO
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }

        public string BodyHtml { get; set; } = string.Empty;

        public string Status { get; set; } = DA_Common.Barony.BaronLetterStatus.Draft;

        /// <summary>
        /// True = letter written by a correspondent (GM) to the baron.
        /// False = letter written by the baron to a correspondent.
        /// </summary>
        public bool IsInbound { get; set; }

        public int TurnNumber { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        /// <summary>Day of month in the campaign calendar (1–31). 0 = legacy / unset.</summary>
        public int Day { get; set; }
        public string Season { get; set; } = "Winter";

        /// <summary>False until the baron opens a delivered inbound message.</summary>
        public bool SeenByBaron { get; set; } = true;

        /// <summary>False until the GM opens a delivered outbound (baron→) message.</summary>
        public bool SeenByGm { get; set; } = true;

        public int SortOrder { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? SentAtUtc { get; set; }

        public bool IsDraft =>
            string.Equals(Status, DA_Common.Barony.BaronLetterStatus.Draft, StringComparison.OrdinalIgnoreCase);

        /// <summary>Baron should see the inbox badge for this message.</summary>
        public bool HasUnreadForBaron =>
            !SeenByBaron
            && !IsDraft
            && IsInbound;

        /// <summary>GM should see the inbox badge for this message (baron wrote to a correspondent).</summary>
        public bool HasUnreadForGm =>
            !SeenByGm
            && !IsDraft
            && !IsInbound;

        public string FormatInWorldDate() =>
            DA_Common.Barony.BaronLetterCalendar.Format(Day, Month, Year);
    }
}
