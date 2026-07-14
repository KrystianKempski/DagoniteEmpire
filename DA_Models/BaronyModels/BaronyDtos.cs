using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    public class BaronyDTO
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public string Name { get; set; } = "Nowa Baronia";
        public int Size { get; set; }

        public int Year { get; set; } = 625;
        public int Month { get; set; } = 1;
        public int TurnNumber { get; set; } = 1;
        public string Season { get; set; } = "Winter";

        public decimal TreasuryGold { get; set; }
        public decimal BaronPurseGold { get; set; }
        public decimal FoodInGranaries { get; set; }
        public int Unrest { get; set; }

        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }

        public PpbVector BaseParameters { get; set; } = new();

        public string? Notes { get; set; }
    }

    /// <summary>Lightweight barony row for MG list / selector.</summary>
    public class BaronyListItemDTO
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaronName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class AdvisorDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string OfficeType { get; set; } = DA_Common.Barony.OfficeType.Custom;
        public string Title { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public bool IsBaron { get; set; }
        public PpbVector Skills { get; set; } = new();
        /// <summary>Administrative skills that affect barony parameters (up to 4).</summary>
        public List<Ppb> SignificantSkills { get; set; } = new();
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? FormulaText { get; set; }
        public string? Description { get; set; }
        public decimal UpkeepGold { get; set; }
    }

    public class BaronyBuildingDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public int? TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = DA_Common.Barony.BuildingKind.Building;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? Description { get; set; }
    }

    public class SocialGroupRelationDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Group { get; set; } = string.Empty;
        public int RelationLevel { get; set; }
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? FormulaText { get; set; }
    }

    public class DecreeDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? Description { get; set; }
        public string? FormulaText { get; set; }
    }

    public class BaronyEventDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TurnNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? Description { get; set; }
    }

    public class CommunityModifierDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Source { get; set; } = string.Empty;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? FormulaText { get; set; }
    }

    public class FiefDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LiegeName { get; set; } = string.Empty;
        public bool IsBaronDemesne { get; set; }
        public decimal BonusMultiplier { get; set; } = 1.0m;
    }

    public class TerrainTileDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string BaseType { get; set; } = DA_Common.Barony.TerrainBaseType.Plains;
        public string FeaturesCsv { get; set; } = string.Empty;
        public int Fertility { get; set; }
        public string? Resource { get; set; }
        public int? FiefId { get; set; }
        public string? Comment { get; set; }

        public List<string> Features =>
            string.IsNullOrWhiteSpace(FeaturesCsv)
                ? new List<string>()
                : FeaturesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    public class TerrainImprovementDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public int? TileId { get; set; }
        public int? TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? Description { get; set; }
        public string? FormulaText { get; set; }
    }

    public class BaronyProjectDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public PpbVector Cost { get; set; } = new();
        public PpbVector Result { get; set; } = new();
        public PpbVector Allocated { get; set; } = new();
        public string ResultDescription { get; set; } = string.Empty;
        public string Status { get; set; } = DA_Common.Barony.ProjectStatus.Draft;
        public int TurnsRemaining { get; set; }
        public string? Notes { get; set; }

        /// <summary>Procent alokacji względem kosztu (0-100), średnia po niezerowych kosztach PPB.</summary>
        public int AllocationPercent
        {
            get
            {
                var ratios = new List<decimal>();
                foreach (var info in PpbCatalog.All)
                {
                    var cost = Cost[info.Key];
                    if (cost <= 0m)
                        continue;
                    var alloc = Allocated[info.Key];
                    ratios.Add(Math.Clamp(alloc / cost, 0m, 1m));
                }
                if (ratios.Count == 0)
                    return 100;
                return (int)Math.Round(ratios.Average() * 100m);
            }
        }
    }

    public class BuildingTemplateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int RequiredLordshipLevel { get; set; }
        public string Kind { get; set; } = DA_Common.Barony.BuildingKind.Building;
        public decimal GoldCost { get; set; }
        public decimal ProductionCost { get; set; }
        public PpbVector EffectAdditive { get; set; } = new();
        public PpbVector EffectPercent { get; set; } = new();
        public string? Description { get; set; }
        public string? TerrainRequirement { get; set; }
    }
}
