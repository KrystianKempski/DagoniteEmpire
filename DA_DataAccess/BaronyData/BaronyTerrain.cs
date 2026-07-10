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

        /// <summary>Mnożnik bonusów z ulepszeń na tym lennie (1.0 dla domeny barona, mniej dla lenników).</summary>
        public decimal BonusMultiplier { get; set; } = 1.0m;
    }

    /// <summary>Pojedyncze pole (kwadrat) terenu baronii.</summary>
    public class TerrainTile
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>Rodzaj bazowy (TerrainBaseType).</summary>
        public string BaseType { get; set; } = DA_Common.Barony.TerrainBaseType.Plains;

        /// <summary>Dodatki terenu jako CSV (TerrainFeature), np. "Las,Rzeka".</summary>
        public string FeaturesCsv { get; set; } = string.Empty;

        /// <summary>Żyzność 0-5 (tylko równiny/wzgórza).</summary>
        public int Fertility { get; set; }

        /// <summary>Zasób podziemny/naturalny (dowolny tekst).</summary>
        public string? Resource { get; set; }

        /// <summary>Lenno, do którego należy pole (opcjonalne).</summary>
        public int? FiefId { get; set; }

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
    }
}
