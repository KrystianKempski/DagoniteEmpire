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

        /// <summary>Czy ten wiersz reprezentuje samego barona.</summary>
        public bool IsBaron { get; set; }

        /// <summary>Umiejętności zarządcze (12 PPB umiejętnościowych) — JSON PpbVector.</summary>
        public string SkillsJson { get; set; } = "{}";

        /// <summary>Wpływ addytywny na PPB — JSON PpbVector.</summary>
        public string AdditiveJson { get; set; } = "{}";

        /// <summary>Wpływ procentowy na PPB (pkt proc.) — JSON PpbVector.</summary>
        public string PercentJson { get; set; } = "{}";

        public string? FormulaText { get; set; }
        public string? Description { get; set; }

        /// <summary>Koszt utrzymania w złocie na turę.</summary>
        public decimal UpkeepGold { get; set; }
    }

    /// <summary>Budynek/ulepszenie działające w mieście głównym (sekcja "Miasto i budynki").</summary>
    public class BaronyBuilding
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        /// <summary>Powiązanie z katalogiem (opcjonalne).</summary>
        public int? TemplateId { get; set; }

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

        /// <summary>Poziom relacji (0 = obojętność).</summary>
        public int RelationLevel { get; set; }

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
    }

    /// <summary>Wydarzenie definiowane przez MG (sekcja "Wydarzenia").</summary>
    public class BaronyEvent
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;
        public int TurnNumber { get; set; }
        public bool IsActive { get; set; } = true;

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
}
