using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>Doradca / urząd baronii (sekcja "Baron i doradcy" oraz strona "Urzędy").</summary>
    public class Advisor
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        /// <summary>Rodzaj urzędu (OfficeType).</summary>
        public string OfficeType { get; set; } = DA_Common.Barony.OfficeType.Custom;

        /// <summary>Nazwa urzędu / tytuł.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Imię postaci doradcy.</summary>
        public string PersonName { get; set; } = string.Empty;

        /// <summary>Optional link to the available-advisor pool record.</summary>
        public int? AvailableAdvisorId { get; set; }

        /// <summary>Czy ten wiersz reprezentuje samego barona.</summary>
        public bool IsBaron { get; set; }

        /// <summary>Umiejętności zarządcze (12 PPB umiejętnościowych) — JSON PpbVector.</summary>
        public string SkillsJson { get; set; } = "{}";

        /// <summary>Które umiejętności mają wpływ na baronię — JSON lista nazw <see cref="Ppb"/> (max 4).</summary>
        public string SignificantSkillsJson { get; set; } = "[]";

        /// <summary>Wpływ addytywny na PPB — JSON PpbVector.</summary>
        public string AdditiveJson { get; set; } = "{}";

        /// <summary>Wpływ procentowy na PPB (pkt proc.) — JSON PpbVector.</summary>
        public string PercentJson { get; set; } = "{}";

        public string? FormulaText { get; set; }
        public string? Description { get; set; }

        /// <summary>Koszt utrzymania w złocie na turę.</summary>
        public decimal UpkeepGold { get; set; }
    }

    public class AvailableAdvisor
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        /// <summary>Computed administrative PPB vector (from <see cref="SheetJson"/>).</summary>
        public string SkillsJson { get; set; } = "{}";
        /// <summary>Court character sheet JSON (<c>CourtCharacterSheet</c>).</summary>
        public string SheetJson { get; set; } = "{}";
    }

    /// <summary>Budynek/ulepszenie działające w mieście głównym (sekcja "Miasto i budynki").</summary>
    public class BaronyBuilding
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        /// <summary>Powiązanie z katalogiem (opcjonalne).</summary>
        public int? TemplateId { get; set; }

        /// <summary>Stable key when this row overrides a fixed core city building.</summary>
        public string? CoreKey { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = DA_Common.Barony.BuildingKind.Building;

        public string AdditiveJson { get; set; } = "{}";
        public string PercentJson { get; set; } = "{}";

        public string? Description { get; set; }
    }

    /// <summary>Relacja z grupą społeczną (sekcja "Relacje z grupami społecznymi").</summary>
    public class SocialGroupRelation
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        /// <summary>Grupa (SocialGroup).</summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>Poziom relacji (0 = indifference).</summary>
        public int RelationLevel { get; set; }

        public int? InfluencePercent { get; set; }

        public bool? IsActive { get; set; }

        /// <summary>Tax rate (%) for town treasury income from this group.</summary>
        public int? TaxPercent { get; set; }

        public string AdditiveJson { get; set; } = "{}";
        public string PercentJson { get; set; } = "{}";

        public string? FormulaText { get; set; }
    }

    /// <summary>Dekret / technologia (sekcja "Dekrety i technologie").</summary>
    public class Decree
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string AdditiveJson { get; set; } = "{}";
        public string PercentJson { get; set; } = "{}";

        public string? Description { get; set; }
        public string? FormulaText { get; set; }

        /// <summary>When false, PPB is excluded from Domain Panel / budget totals.</summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Wydarzenie definiowane przez MG (sekcja "Wydarzenia").</summary>
    public class BaronyEvent
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>First turn on which the event applies (inclusive).</summary>
        public int StartTurn { get; set; } = 1;

        /// <summary>Last turn (inclusive). Null = ongoing / no end.</summary>
        public int? EndTurn { get; set; }

        public string AdditiveJson { get; set; } = "{}";
        public string PercentJson { get; set; } = "{}";

        public string? Description { get; set; }
    }

    /// <summary>Kara/bonus społeczności (sekcja "Kary i bonusy społeczności").</summary>
    public class CommunityModifier
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        /// <summary>Źródło (CommunitySource).</summary>
        public string Source { get; set; } = string.Empty;

        public string AdditiveJson { get; set; } = "{}";
        public string PercentJson { get; set; } = "{}";

        public string? FormulaText { get; set; }
    }

    /// <summary>Custom baron influence bonus row (Baron Card section).</summary>
    public class BaronInfluenceModifier
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Source { get; set; } = string.Empty;

        public string AdditiveJson { get; set; } = "{}";

        public string? FormulaText { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>Custom advisor influence bonus row (Offices tab).</summary>
    public class AdvisorInfluenceModifier
    {
        [Key]
        public int Id { get; set; }
        public int AdvisorId { get; set; }

        public string Source { get; set; } = string.Empty;

        public string AdditiveJson { get; set; } = "{}";

        public string? FormulaText { get; set; }
        public string? Description { get; set; }

        /// <summary>Additional gold upkeep per turn from this bonus source.</summary>
        public decimal CostGold { get; set; }
    }

    /// <summary>Custom income/expense row on the Resources balance table.</summary>
    public class BaronyResourceSource
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Resource deltas (JSON PpbVector). Positive = income, negative = expense.</summary>
        public string AdditiveJson { get; set; } = "{}";

        /// <summary>
        /// Legacy flag (unused by Resolve Turn). Ledger is wiped wholesale on Resolve;
        /// mid-turn rows fold into <c>PreviousTurnStock</c> on the next resolve.
        /// </summary>
        public bool IsTurnEphemeral { get; set; }

        /// <summary>Legacy: turn visibility for ephemeral sources (unused).</summary>
        public int? VisibleOnTurn { get; set; }
    }

    /// <summary>Custom gold income/expense line for the baron’s personal purse.</summary>
    public class BaronPurseSource
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Gold delta. Positive = income, negative = expense.</summary>
        public decimal Amount { get; set; }
    }

    /// <summary>Manual Prestige / Honor / Fear source on the Baron Card (traits, adventures, etc.).</summary>
    public class BaronPhpSource
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Source { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }
    }

    /// <summary>Trophy, treasure, or artifact displayed in the Lord's Seat (Baron Card).</summary>
    public class BaronArtifact
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary><see cref="DA_Common.Barony.BaronArtifactKind"/>.</summary>
        public string Kind { get; set; } = DA_Common.Barony.BaronArtifactKind.Other;

        /// <summary><see cref="DA_Common.Barony.BaronArtifactOrigin"/>.</summary>
        public string Origin { get; set; } = DA_Common.Barony.BaronArtifactOrigin.Acquired;

        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }

        /// <summary>Lord's Seat chamber where the item is displayed (optional).</summary>
        public int? SeatRoomId { get; set; }

        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>Percent modifier to the baron's BT (time unit) pool for a turn.</summary>
    public class BaronTimeModifier
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Source { get; set; } = string.Empty;
        public decimal Percent { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>Activity the baron spends BT on during the current turn.</summary>
    public class BaronTimeAction
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary><see cref="DA_Common.Barony.BaronTimeActionKind"/>.</summary>
        public string Kind { get; set; } = DA_Common.Barony.BaronTimeActionKind.Other;

        public int CostJc { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        /// <summary>Built-in row (e.g. Barony management) — cannot be deleted from UI.</summary>
        public bool IsSystem { get; set; }
    }

    /// <summary>A correspondence thread (title + correspondent). Messages live in <see cref="BaronLetterMessage"/>.</summary>
    public class BaronLetterThread
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int? RelationId { get; set; }
        public string CorrespondentName { get; set; } = string.Empty;
        public string? CorrespondentTitle { get; set; }
        public string? CorrespondentCategory { get; set; }

        /// <summary><see cref="DA_Common.Barony.BaronLetterReplyRegion"/>.</summary>
        public string ReplyRegion { get; set; } = DA_Common.Barony.BaronLetterReplyRegion.EasternMarch;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public List<BaronLetterMessage> Messages { get; set; } = new();
    }

    /// <summary>A single letter/post inside a <see cref="BaronLetterThread"/>.</summary>
    public class BaronLetterMessage
    {
        [Key]
        public int Id { get; set; }
        public int ThreadId { get; set; }

        public string BodyHtml { get; set; } = string.Empty;

        /// <summary><see cref="DA_Common.Barony.BaronLetterStatus"/>.</summary>
        public string Status { get; set; } = DA_Common.Barony.BaronLetterStatus.Draft;

        /// <summary>True = from correspondent (GM) to baron; false = from baron to correspondent.</summary>
        public bool IsInbound { get; set; }

        public int TurnNumber { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        /// <summary>Day of month in the campaign calendar (1–31). 0 = legacy / unset.</summary>
        public int Day { get; set; }
        public string Season { get; set; } = "Winter";

        /// <summary>False until the baron opens this delivered inbound message.</summary>
        public bool SeenByBaron { get; set; } = true;

        /// <summary>False until the GM opens this delivered outbound (baron→) message.</summary>
        public bool SeenByGm { get; set; } = true;

        public int SortOrder { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? SentAtUtc { get; set; }

        public BaronLetterThread? Thread { get; set; }
    }
}
