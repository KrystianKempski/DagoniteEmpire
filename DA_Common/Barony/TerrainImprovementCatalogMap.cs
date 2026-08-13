namespace DA_Common.Barony
{
    /// <summary>
    /// Maps a map-placed improvement + tile context (fertility / resource / features)
    /// to a Building catalog template name.
    /// </summary>
    public static class TerrainImprovementCatalogMap
    {
        public const string UnfortifiedVillage = "Unfortified Village";
        public const string HuntersLodge = "Hunter's Lodge";
        public const string HuntersLodgeFurs = "Hunter's Lodge - Furs";
        public const string FishingPier = "Fishing Pier";
        public const string SawmillCommon = "Sawmill - common";
        public const string ClayPit = "Clay pit";
        public const string FarmDyePlant = "Farm (Dye plant)";

        /// <summary>Extra Food when Fishing harbor sits on a Fishery deposit.</summary>
        public const decimal FishingHarborFisheryFoodBonus = 1m;

        /// <summary>Extra Treasury when Fishing harbor sits on a Fishery deposit.</summary>
        public const decimal FishingHarborFisheryTreasuryBonus = 10m;

        /// <summary>
        /// Resolves the Buildings catalog template for a map pin on the given tile context.
        /// Returns null when the pin is not catalog-backed (e.g. Town) or context is invalid.
        /// </summary>
        public static string? ResolveTemplateName(
            string? mapKind,
            int fertility,
            string? resource,
            int featuresMask = 0,
            string? baseType = null)
        {
            if (string.IsNullOrWhiteSpace(mapKind))
                return null;

            return mapKind switch
            {
                MapImprovement.Farm => CanPlaceFarm(baseType, featuresMask)
                    ? ResolveFarm(fertility, resource)
                    : null,
                MapImprovement.Mine => ResolveExtractive(resource),
                MapImprovement.Sawmill => CanPlaceSawmill(featuresMask, resource)
                    ? ResolveSawmill(resource)
                    : null,
                MapImprovement.HuntersLodge => ResolveHuntersLodge(resource),
                MapImprovement.FishingHarbor => CanPlaceFishingHarbor(featuresMask) ? FishingPier : null,
                MapImprovement.Village => UnfortifiedVillage,
                MapImprovement.Town => null,
                _ => null,
            };
        }

        /// <summary>True when this map kind needs a catalog template (and usually tile context).</summary>
        public static bool IsCatalogBacked(string? mapKind) => mapKind switch
        {
            MapImprovement.Farm
                or MapImprovement.Mine
                or MapImprovement.Sawmill
                or MapImprovement.HuntersLodge
                or MapImprovement.FishingHarbor
                or MapImprovement.Village => true,
            _ => false,
        };

        public static bool CanPlaceFishingHarbor(int featuresMask) =>
            TerrainFeature.Has(featuresMask, TerrainFeature.Coast)
            || TerrainFeature.Has(featuresMask, TerrainFeature.River);

        /// <summary>Sawmill needs forest / dense forest, or a timber deposit (Ironwood / Elven alder / Shipbuilding wood).</summary>
        public static bool CanPlaceSawmill(int featuresMask, string? resource) =>
            IsWoodResource(resource)
            || TerrainFeature.Has(featuresMask, TerrainFeature.Forest)
            || TerrainFeature.Has(featuresMask, TerrainFeature.DenseForest);

        /// <summary>
        /// Farms need plains or hills, and must not sit on forest, dense forest, or swamp.
        /// </summary>
        public static bool CanPlaceFarm(string? baseType, int featuresMask) =>
            TerrainBaseType.SupportsFertility(baseType)
            && !TerrainFeature.Has(featuresMask, TerrainFeature.Forest)
            && !TerrainFeature.Has(featuresMask, TerrainFeature.DenseForest)
            && !TerrainFeature.Has(featuresMask, TerrainFeature.Swamp);

        public static bool HasFisheryBonus(string? resource) =>
            string.Equals(resource, TerrainResource.Fishery, StringComparison.Ordinal);

        /// <summary>No map improvements may be placed on water tiles.</summary>
        public static bool IsWaterTile(string? baseType) => TerrainBaseType.IsWater(baseType);

        /// <summary>
        /// Catalog template names that are valid to build / paint on this tile
        /// (same rules as the improvement brush). Empty on water.
        /// </summary>
        public static HashSet<string> AllowedCatalogTemplateNames(
            int fertility,
            string? resource,
            int featuresMask = 0,
            string? baseType = null,
            IEnumerable<BuildingTemplatePin>? extraCatalogTemplates = null)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (IsWaterTile(baseType))
                return set;

            foreach (var mapKind in new[]
                     {
                         MapImprovement.Farm,
                         MapImprovement.Mine,
                         MapImprovement.Sawmill,
                         MapImprovement.HuntersLodge,
                         MapImprovement.FishingHarbor,
                         MapImprovement.Village,
                     })
            {
                var name = ResolveTemplateName(mapKind, fertility, resource, featuresMask, baseType);
                if (!string.IsNullOrWhiteSpace(name))
                    set.Add(name);
            }

            HerdStockRequirements.AddPlaceableTemplateNames(set, fertility, featuresMask, baseType);

            if (extraCatalogTemplates is not null)
            {
                foreach (var entry in extraCatalogTemplates)
                {
                    if (string.IsNullOrWhiteSpace(entry.Name) || string.IsNullOrWhiteSpace(entry.MapPinKind))
                        continue;

                    if (CanPlaceMapImprovement(entry.MapPinKind, fertility, resource, featuresMask, baseType)
                        && MeetsTerrainRequirement(entry.TerrainRequirement, featuresMask, baseType))
                        set.Add(entry.Name.Trim());
                }
            }

            return set;
        }

        /// <summary>MG catalog entry with explicit map pin for terrain placement.</summary>
        public readonly record struct BuildingTemplatePin(string Name, string? MapPinKind, string? TerrainRequirement = null);

        private static bool MeetsTerrainRequirement(string? requirement, int featuresMask, string? baseType)
        {
            if (string.IsNullOrWhiteSpace(requirement))
                return true;

            return requirement.Trim() switch
            {
                "Water" or "Woda" => TerrainBaseType.IsWater(baseType),
                "Plains" or "Równiny" => string.Equals(baseType, TerrainBaseType.Plains, StringComparison.OrdinalIgnoreCase),
                "Hills" or "Wzgórza" => string.Equals(baseType, TerrainBaseType.Hills, StringComparison.OrdinalIgnoreCase),
                "Forest" or "Las" => TerrainFeature.Has(featuresMask, TerrainFeature.Forest),
                "Dense forest" or "Gęsty las" => TerrainFeature.Has(featuresMask, TerrainFeature.DenseForest),
                "Swamp" or "Bagna" => TerrainFeature.Has(featuresMask, TerrainFeature.Swamp),
                "Coast or river" or "Wybrzeże lub rzeka" =>
                    TerrainFeature.Has(featuresMask, TerrainFeature.Coast) || TerrainFeature.Has(featuresMask, TerrainFeature.River),
                _ => true,
            };
        }

        /// <summary>
        /// Whether a map brush kind may be placed here (water ban + catalog rules when applicable).
        /// Town / Custom only require non-water.
        /// </summary>
        public static bool CanPlaceMapImprovement(
            string? mapKind,
            int fertility,
            string? resource,
            int featuresMask = 0,
            string? baseType = null)
        {
            if (IsWaterTile(baseType))
                return false;

            if (string.IsNullOrWhiteSpace(mapKind))
                return false;

            if (mapKind is MapImprovement.Town or MapImprovement.Custom)
                return true;

            if (!IsCatalogBacked(mapKind))
                return true;

            return ResolveTemplateName(mapKind, fertility, resource, featuresMask, baseType) is not null;
        }

        /// <summary>Human-readable reason when ResolveTemplateName returns null for a catalog-backed kind.</summary>
        public static string? FailureReason(
            string? mapKind,
            int fertility,
            string? resource,
            int featuresMask = 0,
            string? baseType = null)
        {
            if (IsWaterTile(baseType))
                return "Cannot place improvements on water.";

            if (mapKind == MapImprovement.Farm && !TerrainBaseType.SupportsFertility(baseType))
                return "Farms require plains or hills.";

            if (mapKind == MapImprovement.Farm
                && (TerrainFeature.Has(featuresMask, TerrainFeature.Forest)
                    || TerrainFeature.Has(featuresMask, TerrainFeature.DenseForest)
                    || TerrainFeature.Has(featuresMask, TerrainFeature.Swamp)))
                return "Farms cannot be placed on forest, dense forest, or swamp.";

            if (mapKind == MapImprovement.Farm && !TerrainFertility.SupportsFarm(fertility))
                return "Farms require fertility 2–5 (poor through exceptional).";

            if (mapKind == MapImprovement.Mine
                && string.Equals(resource, TerrainResource.Fishery, StringComparison.Ordinal))
                return "Cannot place a mine on a fishery. Use Fishing harbor instead.";

            if (mapKind == MapImprovement.Mine && IsWoodResource(resource))
                return "Cannot place a mine on timber. Use a sawmill instead.";

            if (mapKind == MapImprovement.Mine && TerrainResource.IsDyePlant(resource))
                return "Cannot place a mine on dye plants. Use a farm instead.";

            if (mapKind == MapImprovement.Mine && string.IsNullOrWhiteSpace(resource))
                return "Mines require a resource deposit on the tile (metal, stone, clay, salt, sulfur, gems…).";

            if (mapKind == MapImprovement.Mine && ResolveExtractive(resource) is null)
                return $"Cannot place a mine on “{TerrainResource.DisplayName(resource)}”. Needs metal, stone, clay, salt, sulfur, or gems.";

            if (mapKind == MapImprovement.FishingHarbor && !CanPlaceFishingHarbor(featuresMask))
                return "Fishing harbor requires coast or river on the tile.";

            if (mapKind == MapImprovement.Sawmill && !CanPlaceSawmill(featuresMask, resource))
                return "Sawmills require forest, dense forest, or a timber deposit (Ironwood / Elven alder / Shipbuilding wood).";

            return null;
        }

        private static string? ResolveFarm(int fertility, string? resource)
        {
            if (!TerrainFertility.SupportsFarm(fertility))
                return null;

            // Woad / madder / weld deposits host the dye triad farm instead of a grain farm.
            if (TerrainResource.IsDyePlant(resource))
                return FarmDyePlant;

            return TerrainFertility.FarmTemplateName(fertility);
        }

        private static bool IsWoodResource(string? resource) =>
            string.Equals(resource, TerrainResource.Ironwood, StringComparison.Ordinal)
            || string.Equals(resource, TerrainResource.ElvenAlder, StringComparison.Ordinal)
            || string.Equals(resource, TerrainResource.ShipbuildingWood, StringComparison.Ordinal);

        private static string ResolveHuntersLodge(string? resource) =>
            string.Equals(resource, TerrainResource.Furs, StringComparison.Ordinal)
                ? HuntersLodgeFurs
                : HuntersLodge;

        private static string? ResolveExtractive(string? resource) => resource switch
        {
            TerrainResource.SoftMetals => "Mine - soft metals",
            TerrainResource.Iron => "Mine - Iron",
            TerrainResource.Silver => "Mine - Silver",
            TerrainResource.Gold => "Mine - Gold",
            TerrainResource.Dagoferryt => "Mine - Dagoferryt",
            TerrainResource.Salt => "Mine - Salt",
            TerrainResource.Sulfur => "Mine - Sulfur",
            TerrainResource.Gemstones => "Mine - precious gems (luxury)",
            TerrainResource.Stone => "Quarry - common stone",
            TerrainResource.Granite => "Quarry - Granite",
            TerrainResource.Tarnit => "Quarry - Tarnit",
            TerrainResource.Obsidian => "Quarry - Obsidian",
            TerrainResource.Clay => ClayPit,
            TerrainResource.Fishery => null,
            // Dye plants are farmed, not mined.
            TerrainResource.Woad or TerrainResource.Madder or TerrainResource.Weld => null,
            _ => null,
        };

        private static string ResolveSawmill(string? resource) => resource switch
        {
            TerrainResource.Ironwood => "Sawmill - Ironwood",
            TerrainResource.ElvenAlder => "Sawmill - Elven alder",
            TerrainResource.ShipbuildingWood => "Sawmill - Shipbuilding wood",
            _ => SawmillCommon,
        };

        /// <summary>
        /// Reverse of <see cref="ResolveTemplateName"/>: catalog template name → map pin kind
        /// (<see cref="MapImprovement"/>). Null when the template is not a map improvement.
        /// </summary>
        public static string? MapKindFromCatalogTemplateName(string? catalogTemplateName)
        {
            if (string.IsNullOrWhiteSpace(catalogTemplateName))
                return null;

            var name = catalogTemplateName.Trim();

            if (string.Equals(name, UnfortifiedVillage, StringComparison.OrdinalIgnoreCase))
                return MapImprovement.Village;

            if (string.Equals(name, HuntersLodge, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, HuntersLodgeFurs, StringComparison.OrdinalIgnoreCase))
                return MapImprovement.HuntersLodge;

            if (string.Equals(name, FishingPier, StringComparison.OrdinalIgnoreCase))
                return MapImprovement.FishingHarbor;

            if (string.Equals(name, ClayPit, StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Mine -", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Quarry -", StringComparison.OrdinalIgnoreCase))
                return MapImprovement.Mine;

            if (name.StartsWith("Sawmill", StringComparison.OrdinalIgnoreCase))
                return MapImprovement.Sawmill;

            if (name.StartsWith("Farm", StringComparison.OrdinalIgnoreCase))
                return MapImprovement.Farm;

            return null;
        }
    }
}
