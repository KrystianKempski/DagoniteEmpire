using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>Projekt baronii: alokacja zasobów PPB → rezultat (strona "Projekty").</summary>
    public class BaronyProject
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>What the project becomes when finished (<see cref="ProjectOutputKind"/>).</summary>
        public string OutputKind { get; set; } = DA_Common.Barony.ProjectOutputKind.DecreeOrTechnology;

        /// <summary>Legacy combined cost JSON; superseded by track-specific costs.</summary>
        public string CostJson { get; set; } = "{}";

        /// <summary>Cost when paying with Gold + Production — JSON PpbVector.</summary>
        public string CostGoldProductionJson { get; set; } = "{}";

        /// <summary>Cost when paying with other cumulative resources — JSON PpbVector.</summary>
        public string CostMaterialsJson { get; set; } = "{}";

        /// <summary>Which payment tracks are allowed (<see cref="ProjectAllowedCostModes"/>).</summary>
        public string AllowedCostModes { get; set; } = DA_Common.Barony.ProjectAllowedCostModes.PlayerChoice;

        /// <summary>Player-selected payment track (<see cref="ProjectCostMode"/>).</summary>
        public string? SelectedCostMode { get; set; }

        /// <summary>Rezultat addytywny — JSON PpbVector.</summary>
        public string ResultJson { get; set; } = "{}";

        /// <summary>Rezultat procentowy — JSON PpbVector.</summary>
        public string ResultPercentJson { get; set; } = "{}";

        /// <summary>Ile już zaalokowano — JSON PpbVector.</summary>
        public string AllocatedJson { get; set; } = "{}";

        public string ResultDescription { get; set; } = string.Empty;

        /// <summary>
        /// When true, non-MG viewers do not see result resources / output description
        /// until the project is completed.
        /// </summary>
        public bool HideResultFromBaron { get; set; }

        /// <summary>Status (ProjectStatus).</summary>
        public string Status { get; set; } = DA_Common.Barony.ProjectStatus.Draft;

        public int TurnsRemaining { get; set; }

        public string? Notes { get; set; }

        /// <summary>Terrain tile this building/improvement project targets (map construction).</summary>
        public int? TileId { get; set; }

        /// <summary>Catalog template chosen for a tile construction project.</summary>
        public int? BuildingTemplateId { get; set; }

        /// <summary>Army unit produced by a Unit Training project.</summary>
        public int? UnitId { get; set; }
    }

    /// <summary>
    /// Globalny katalog budynków/ulepszeń możliwych do zbudowania (strona "Budynki i Ulepszenia").
    /// Edytowalny przez MG, do wglądu dla gracza. Nie jest powiązany z konkretną baronią.
    /// </summary>
    public class BuildingTemplate
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public bool IsCustom { get; set; }

        /// <summary>Minimalny wymagany poziom władzy lordowskiej.</summary>
        public int RequiredLordshipLevel { get; set; }

        /// <summary>Rodzaj (BuildingKind: Budynek / Ulepszenie).</summary>
        public string Kind { get; set; } = DA_Common.Barony.BuildingKind.Building;

        public decimal GoldCost { get; set; }
        public decimal ProductionCost { get; set; }

        public string EffectAdditiveJson { get; set; } = "{}";
        public string EffectPercentJson { get; set; } = "{}";

        public string? Description { get; set; }

        /// <summary>Wymagania terenowe dla ulepszeń (np. "Las", "Zasób").</summary>
        public string? TerrainRequirement { get; set; }

        /// <summary>Map pin kind (<see cref="MapImprovement"/>) for terrain improvements.</summary>
        public string? MapPinKind { get; set; }

        /// <summary>Optional map icon override (defaults from <see cref="MapPinKind"/>).</summary>
        public string? IconUrl { get; set; }
    }
}
