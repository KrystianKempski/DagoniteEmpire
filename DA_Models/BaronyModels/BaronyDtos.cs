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

        /// <summary>Cumulative stocks for Food, Gold, Production, Science, Magic, Culture, Intelligence, Defense.</summary>
        public PpbVector ResourceStocks { get; set; } = new();

        /// <summary>Income from the previous turn (editable on turn 1 for MG starting grants).</summary>
        public PpbVector PreviousTurnIncome { get; set; } = new();

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
        public int? AvailableAdvisorId { get; set; }
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

    public class AvailableAdvisorDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PpbVector Skills { get; set; } = new();
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

        /// <summary>Display-only population (e.g. town row in City Buildings).</summary>
        public int Population { get; set; }
    }

    public class SocialGroupRelationDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Group { get; set; } = string.Empty;
        public int RelationLevel { get; set; }
        public int? InfluencePercent { get; set; }
        public bool? IsActive { get; set; }
        public int? TaxPercent { get; set; }
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

        /// <summary>When false, PPB is excluded from totals.</summary>
        public bool IsActive { get; set; } = true;
    }

    public class BaronyEventDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>First turn on which the event applies (inclusive).</summary>
        public int StartTurn { get; set; } = 1;

        /// <summary>Last turn (inclusive). Null = ongoing / no end.</summary>
        public int? EndTurn { get; set; }

        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? Description { get; set; }

        public bool IsActiveOnTurn(int turn) =>
            turn >= StartTurn && (EndTurn is null || turn <= EndTurn.Value);

        public string TurnRangeLabel =>
            EndTurn is int end ? $"{StartTurn}–{end}" : $"{StartTurn}–∞";
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

    public class BaronyResourceSourceDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Resource deltas. Positive = income, negative = expense.</summary>
        public PpbVector Additive { get; set; } = new();
    }

    public class BaronPurseSourceDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Gold delta. Positive = income, negative = expense.</summary>
        public decimal Amount { get; set; }
    }

    public class FiefDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LiegeName { get; set; } = string.Empty;
        public bool IsBaronDemesne { get; set; }
        public bool IsDomainDefault { get; set; }
        public int? SeniorDomainId { get; set; }
        public string ColorHex { get; set; } = "#4d7ea8";
        public decimal BonusMultiplier { get; set; } = 1.0m;
    }

    public class TerrainTileDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public int MapId { get; set; } = 1;
        public int X { get; set; }
        public int Y { get; set; }
        public string BaseType { get; set; } = DA_Common.Barony.TerrainBaseType.Plains;
        /// <summary>Feature bit flags (TerrainFeature).</summary>
        public int FeaturesMask { get; set; }
        public int Fertility { get; set; }
        public string? Resource { get; set; }
        public int? FiefId { get; set; }
        public int? MapDomainId { get; set; }
        public string? Comment { get; set; }

        public bool HasFeature(int flag) => DA_Common.Barony.TerrainFeature.Has(FeaturesMask, flag);
    }

    public class TerrainMapDomainDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LordName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#888888";
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
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

        /// <summary>When false, PPB is excluded from totals and shown struck through.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Why inactive — shown on Domain Panel name hover.</summary>
        public string? InactiveReason { get; set; }

        /// <summary>Optional icon override for custom map improvements.</summary>
        public string? IconUrl { get; set; }

        /// <summary>Settlement population (villages and towns).</summary>
        public int Population { get; set; }

        /// <summary>Village palisade (+5 Def, +3 Stab, +1 Law).</summary>
        public bool HasPalisade { get; set; }
    }

    public class BaronyProjectDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OutputKind { get; set; } = DA_Common.Barony.ProjectOutputKind.DecreeOrTechnology;
        public PpbVector CostGoldProduction { get; set; } = new();
        public PpbVector CostMaterials { get; set; } = new();
        public string AllowedCostModes { get; set; } = DA_Common.Barony.ProjectAllowedCostModes.PlayerChoice;
        public string? SelectedCostMode { get; set; }
        public PpbVector ResultAdditive { get; set; } = new();
        public PpbVector ResultPercent { get; set; } = new();
        public PpbVector Allocated { get; set; } = new();
        public string ResultDescription { get; set; } = string.Empty;
        public string Status { get; set; } = DA_Common.Barony.ProjectStatus.Draft;
        public int TurnsRemaining { get; set; }
        public string? Notes { get; set; }

        /// <summary>Active payment track after GM rules and player selection.</summary>
        public string EffectiveCostMode => ResolveEffectiveCostMode();

        public IReadOnlyList<PpbInfo> ActiveCostColumns =>
            EffectiveCostMode == DA_Common.Barony.ProjectCostMode.GoldProduction
                ? ProjectCostCatalog.GoldProduction
                : ProjectCostCatalog.Materials;

        public PpbVector GetActiveCost() =>
            EffectiveCostMode == DA_Common.Barony.ProjectCostMode.GoldProduction
                ? ProjectCostCatalog.SliceGoldProduction(CostGoldProduction)
                : ProjectCostCatalog.SliceMaterials(CostMaterials);

        public List<string> GetSelectableCostModes()
        {
            var hasGoldProduction = ProjectCostCatalog.HasRequirement(CostGoldProduction);
            var hasMaterials = ProjectCostCatalog.HasRequirement(CostMaterials);

            return AllowedCostModes switch
            {
                DA_Common.Barony.ProjectAllowedCostModes.GoldProductionOnly when hasGoldProduction =>
                    new List<string> { DA_Common.Barony.ProjectCostMode.GoldProduction },
                DA_Common.Barony.ProjectAllowedCostModes.MaterialsOnly when hasMaterials =>
                    new List<string> { DA_Common.Barony.ProjectCostMode.Materials },
                _ => BuildChoiceModes(hasGoldProduction, hasMaterials),
            };
        }

        public bool HasAllocationOnActiveTrack() =>
            ActiveCostColumns.Any(info => Allocated[info.Key] > 0m);

        public bool CanSwitchCostMode =>
            Status is not (ProjectStatus.Completed or ProjectStatus.Cancelled)
            && GetSelectableCostModes().Count > 1
            && !HasAllocationOnActiveTrack();

        /// <summary>Procent alokacji względem kosztu aktywnej ścieżki (0-100).</summary>
        public int AllocationPercent
        {
            get
            {
                var cost = GetActiveCost();
                var ratios = new List<decimal>();
                foreach (var info in ActiveCostColumns)
                {
                    var required = cost[info.Key];
                    if (required <= 0m)
                        continue;
                    ratios.Add(Math.Clamp(Allocated[info.Key] / required, 0m, 1m));
                }

                if (ratios.Count == 0)
                    return ProjectCostCatalog.HasRequirement(cost) ? 0 : 100;
                return (int)Math.Round(ratios.Average() * 100m);
            }
        }

        /// <summary>Remaining cost on the active payment track only.</summary>
        public PpbVector RemainingCost()
        {
            var cost = GetActiveCost();
            var v = new PpbVector();
            foreach (var info in ActiveCostColumns)
                v[info.Key] = Math.Max(0m, cost[info.Key] - Allocated[info.Key]);
            return v;
        }

        public bool HasRemainingCost =>
            ActiveCostColumns.Any(info => RemainingCost()[info.Key] > 0m);

        public bool CanAcceptAllocation =>
            Status is not (ProjectStatus.Completed or ProjectStatus.Cancelled) && HasRemainingCost;

        public bool HasAnyAllocation =>
            ResourceCatalog.All.Any(info => Allocated[info.Key] > 0m);

        public bool CanClearAllocation =>
            Status == ProjectStatus.Draft && HasAnyAllocation;

        /// <summary>
        /// Negative Resources balance row: allocated + remaining on active track.
        /// Only when at least one resource was allocated; otherwise no stock impact.
        /// </summary>
        public PpbVector ResourcesBalanceImpact()
        {
            var v = new PpbVector();
            if (!HasAnyAllocation)
                return v;
            if (Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
                return v;

            var remaining = RemainingCost();
            foreach (var info in ResourceCatalog.All)
                v[info.Key] -= Allocated[info.Key] + remaining[info.Key];
            return v;
        }

        private string ResolveEffectiveCostMode()
        {
            var selectable = GetSelectableCostModes();
            if (selectable.Count == 0)
                return DA_Common.Barony.ProjectCostMode.GoldProduction;
            if (selectable.Count == 1)
                return selectable[0];
            if (!string.IsNullOrWhiteSpace(SelectedCostMode) && selectable.Contains(SelectedCostMode))
                return SelectedCostMode;
            return selectable[0];
        }

        private static List<string> BuildChoiceModes(bool hasGoldProduction, bool hasMaterials)
        {
            var modes = new List<string>();
            if (hasGoldProduction)
                modes.Add(DA_Common.Barony.ProjectCostMode.GoldProduction);
            if (hasMaterials)
                modes.Add(DA_Common.Barony.ProjectCostMode.Materials);
            return modes;
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
