using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>Lenno — jednostka nadania ziemi lennikowi (lub demena barona).</summary>
    public class Fief
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Imię lennika.</summary>
        public string LiegeName { get; set; } = string.Empty;

        /// <summary>Czy to bezpośrednia domena barona.</summary>
        public bool IsBaronDemesne { get; set; }

        /// <summary>Auto-created default fief for a domain (non-deletable).</summary>
        public bool IsDomainDefault { get; set; }

        /// <summary>Domain that acts as senior for this fief.</summary>
        public int? SeniorDomainId { get; set; }

        /// <summary>#RRGGBB hex color used on terrain fief layer.</summary>
        public string ColorHex { get; set; } = "#4d7ea8";

        /// <summary>Mnożnik bonusów z ulepszeń na tym lennie (1.0 dla domeny barona, mniej dla lenników).</summary>
        public decimal BonusMultiplier { get; set; } = 1.0m;
    }

    /// <summary>Regional domain painted on a barony terrain map.</summary>
    public class TerrainMapDomain
    {
        [Key]
        public int Id { get; set; }

        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Ruling lord display name.</summary>
        public string LordName { get; set; } = string.Empty;

        /// <summary>#RRGGBB hex color.</summary>
        public string ColorHex { get; set; } = "#888888";

        /// <summary>Player barony demesne — rendered with lower overlay opacity.</summary>
        public bool IsPrimary { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>Pojedyncze pole (kwadrat) terenu baronii.</summary>
    public class TerrainTile
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        /// <summary>Shared map id (one grid for neighboring baronies).</summary>
        public int MapId { get; set; } = 1;

        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>Rodzaj bazowy (TerrainBaseType).</summary>
        public string BaseType { get; set; } = DA_Common.Barony.TerrainBaseType.Plains;

        /// <summary>Feature bit flags (TerrainFeature).</summary>
        public int FeaturesMask { get; set; }

        /// <summary>Żyzność 0-5 (tylko równiny/wzgórza).</summary>
        public int Fertility { get; set; }

        /// <summary>Zasób podziemny/naturalny (dowolny tekst).</summary>
        public string? Resource { get; set; }

        /// <summary>Lenno, do którego należy pole (opcjonalne).</summary>
        public int? FiefId { get; set; }

        /// <summary>Regional domain ownership (terrain map layer).</summary>
        public int? MapDomainId { get; set; }

        public string? Comment { get; set; }
    }

    /// <summary>Ulepszenie zbudowane na polu terenu (sekcja "Ulepszenia terenu").</summary>
    public class TerrainImprovement
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public int? TileId { get; set; }
        public int? TemplateId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Efektywny wpływ addytywny (już po ewentualnym pomniejszeniu dla lenna) — JSON PpbVector.</summary>
        public string AdditiveJson { get; set; } = "{}";
        public string PercentJson { get; set; } = "{}";

        public string? Description { get; set; }
        public string? FormulaText { get; set; }

        /// <summary>When false, PPB is shown struck through and excluded from Domain Panel / budget totals.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Why the improvement is inactive (shown on name hover).</summary>
        public string? InactiveReason { get; set; }

        /// <summary>Optional icon override (custom improvements). Relative URL under wwwroot.</summary>
        public string? IconUrl { get; set; }
    }
}
