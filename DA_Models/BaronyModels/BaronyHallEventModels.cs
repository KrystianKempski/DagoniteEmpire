using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    public class BaronyHallEventDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string TriggerKind { get; set; } = BaronyHallEventTriggerKind.Manual;
        public string Status { get; set; } = BaronyHallEventStatus.Pending;
        public int TurnNumber { get; set; }
        public string IconPath { get; set; } = "icons/vertical-banner.svg";
        public string ResponseBody { get; set; } = string.Empty;
        public int? ChapterId { get; set; }
        public int? AudienceId { get; set; }
        public int? PublishAtTurn { get; set; }
        public PpbVector ConsequenceAdditive { get; set; } = new();
        public PpbVector ConsequencePercent { get; set; } = new();
        public int? ConsequenceEndTurn { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? AcknowledgedAtUtc { get; set; }

        public bool IsScheduled => BaronyHallEventStatus.IsScheduled(Status);
        public bool IsPending => BaronyHallEventStatus.IsPending(Status);
        public bool IsAcknowledged => BaronyHallEventStatus.IsAcknowledged(Status);
        public bool IsConverted => BaronyHallEventStatus.IsConverted(Status);
        public bool IsArchived => BaronyHallEventStatus.IsArchived(Status);
        public bool NeedsBaronResponse => IsPending;
        public bool CanMgEdit => BaronyHallEventStatus.CanMgEdit(Status);
        public bool ShowsInOrbit => IsPending || IsScheduled;

        public bool HasConsequence =>
            !ConsequenceAdditive.IsEmpty || !ConsequencePercent.IsEmpty;
    }

    public class HallEventAcknowledgeResultDTO
    {
        public BaronyHallEventDTO HallEvent { get; set; } = new();
        public BaronAudienceDTO Audience { get; set; } = new();
    }

    public class BaronyHallEventTemplateDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string TriggerKind { get; set; } = BaronyHallEventTriggerKind.Manual;
        public string IconPath { get; set; } = "icons/vertical-banner.svg";
        public PpbVector ConsequenceAdditive { get; set; } = new();
        public PpbVector ConsequencePercent { get; set; } = new();
        public int? ConsequenceEndTurn { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    /// <summary>Pending hall events badge for the Audience Hall tab.</summary>
    public class BaronyHallEventInboxBadgeDTO
    {
        public int PendingCount { get; set; }
        public int? LatestPendingEventId { get; set; }
    }
}
