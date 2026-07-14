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

        /// <summary>Koszt (wymagana alokacja) w PPB — JSON PpbVector.</summary>
        public string CostJson { get; set; } = "{}";

        /// <summary>Rezultat w PPB — JSON PpbVector.</summary>
        public string ResultJson { get; set; } = "{}";

        /// <summary>Ile już zaalokowano — JSON PpbVector.</summary>
        public string AllocatedJson { get; set; } = "{}";

        public string ResultDescription { get; set; } = string.Empty;

        /// <summary>Status (ProjectStatus).</summary>
        public string Status { get; set; } = DA_Common.Barony.ProjectStatus.Draft;

        public int TurnsRemaining { get; set; }

        public string? Notes { get; set; }
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
    }
}
