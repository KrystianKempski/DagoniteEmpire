namespace DA_Models.BaronyModels
{
    public class BaronAudienceDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string PetitionerName { get; set; } = string.Empty;

        /// <summary>wwwroot-relative icon path for the petitioner (e.g. icons/farmer.svg).</summary>
        public string? PetitionerIcon { get; set; }

        /// <summary>Council only: display name of the advisor who owns this topic. Null for plain Audience kind.</summary>
        public string? AssignedAdvisorName { get; set; }

        /// <summary><see cref="DA_Common.Barony.BaronAudienceKind"/>.</summary>
        public string Kind { get; set; } = DA_Common.Barony.BaronAudienceKind.Audience;

        public string Status { get; set; } = DA_Common.Barony.BaronAudienceStatus.Scheduled;

        public int TurnNumber { get; set; }
        public int? ContinuedFromAudienceId { get; set; }

        public string GmSummary { get; set; } = string.Empty;
        public string OutcomeNotes { get; set; } = string.Empty;

        public DA_Common.Barony.PpbVector Additive { get; set; } = new();
        public DA_Common.Barony.PpbVector Percent { get; set; } = new();

        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }

        public List<BaronAudienceExchangeDTO> Exchanges { get; set; } = new();

        public int ExchangeCount => Exchanges.Count;

        public BaronAudienceExchangeDTO? LastExchange => Exchanges
            .OrderByDescending(x => x.SortOrder)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();

        public bool IsActive => DA_Common.Barony.BaronAudienceStatus.IsActive(Status);
        public bool IsClosed => DA_Common.Barony.BaronAudienceStatus.IsClosed(Status);

        public string StatusDisplay => DA_Common.Barony.BaronAudienceStatus.DisplayName(Status);

        public bool ContributesToTurn(int currentTurn) =>
            DA_Common.Barony.BaronAudiencePpb.ContributesToTurn(TurnNumber, Status, currentTurn);

        public bool HasPpb => !Additive.IsEmpty || !Percent.IsEmpty;
        public bool HasPhp => Prestige != 0 || Honor != 0 || Fear != 0;
        public bool HasGrants => HasPpb || HasPhp;
    }

    public class BaronAudienceExchangeDTO
    {
        public int Id { get; set; }
        public int AudienceId { get; set; }

        public string Body { get; set; } = string.Empty;

        /// <summary>True = GM side (petitioner / other NPC); false = baron.</summary>
        public bool IsFromPetitioner { get; set; }

        /// <summary>
        /// Optional speaker label. When empty: petitioner name (GM side) or "Lord" (baron).
        /// </summary>
        public string? SpeakerName { get; set; }

        /// <summary>System line recording a PPB grant (speaker label: Resource change).</summary>
        public bool IsResourceChange { get; set; }

        /// <summary>PPB delta granted by this resource-change line.</summary>
        public DA_Common.Barony.PpbVector Additive { get; set; } = new();

        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }

        public int TurnNumber { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public bool HasPhp => Prestige != 0 || Honor != 0 || Fear != 0;
    }
}
