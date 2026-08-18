namespace DA_Common.Barony
{
    /// <summary>
    /// Herd / stud improvements that require prior trade access to breeding stock.
    /// Once built, the improvement becomes a local production source of that good.
    /// </summary>
    public static class HerdStockRequirements
    {
        /// <summary>Catalog template name → required trade-good key.</summary>
        public static readonly IReadOnlyDictionary<string, string> RequiredGoodByTemplate =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Sheep pastures"] = "sheep",
                ["Pastures (cattle)"] = "cattle",
                ["Horse Stud (regular)"] = "horses",
                ["Horse Stud (military)"] = "war-horses",
                ["Horse Stud (noble)"] = "noble-horses",
            };

        public static bool TryGetRequiredGoodKey(string? templateName, out string goodKey)
        {
            goodKey = string.Empty;
            if (string.IsNullOrWhiteSpace(templateName))
                return false;
            return RequiredGoodByTemplate.TryGetValue(templateName.Trim(), out goodKey!);
        }

        public static string? RequiredGoodDisplayName(string? templateName)
        {
            if (!TryGetRequiredGoodKey(templateName, out var key))
                return null;
            return TradeGoodsDisplay.DisplayName(key);
        }

        public static bool HasAccess(string? templateName, TradeGoodAvailabilitySnapshot? availability)
        {
            if (!TryGetRequiredGoodKey(templateName, out var key))
                return true;
            return availability?.IsAvailable(key) == true;
        }

        public static string RequiresTradeAccessLine(string goodDisplayName) =>
            $"Requires trade access to {goodDisplayName} (breeding stock via treaty, import, or MG). Once built, this site becomes a local source.";

        /// <summary>
        /// Open herd/stud catalog names placeable on farmable plains/hills tiles
        /// (not resolved from the Farm/Mine brush — listed explicitly).
        /// </summary>
        public static void AddPlaceableTemplateNames(
            HashSet<string> set,
            int fertility,
            int featuresMask,
            string? baseType)
        {
            if (TerrainImprovementCatalogMap.IsWaterTile(baseType))
                return;
            if (!TerrainImprovementCatalogMap.CanPlaceFarm(baseType, featuresMask))
                return;
            if (!TerrainFertility.SupportsFarm(fertility))
                return;

            foreach (var name in RequiredGoodByTemplate.Keys)
                set.Add(name);
        }
    }
}
