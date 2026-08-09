namespace DA_Common.Barony
{
    /// <summary>
    /// Derived trade-good availability: local production (buildings / terrain improvements),
    /// treaty receipts, and optional MG overrides.
    /// </summary>
    public sealed class TradeGoodAvailabilitySnapshot
    {
        public HashSet<string> ProducedKeys { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> TreatyReceivedKeys { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> OverrideKeys { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Produced ∪ treaty-received ∪ MG override — drives display and PPB.</summary>
        public HashSet<string> AvailableKeys { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Produced ∪ MG override — goods the barony may grant in treaties (not re-exports).</summary>
        public HashSet<string> GrantableKeys { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        public bool IsProduced(string key) => ProducedKeys.Contains(key);
        public bool IsTreatyReceived(string key) => TreatyReceivedKeys.Contains(key);
        public bool IsOverride(string key) => OverrideKeys.Contains(key);
        public bool IsAvailable(string key) => AvailableKeys.Contains(key);
        public bool IsGrantable(string key) => GrantableKeys.Contains(key);

        public string SourceLabel(string key)
        {
            var parts = new List<string>(3);
            if (IsProduced(key))
                parts.Add("produced");
            if (IsTreatyReceived(key))
                parts.Add("trade");
            if (IsOverride(key))
                parts.Add("MG override");
            return parts.Count == 0 ? "—" : string.Join(" · ", parts);
        }
    }

    public static class TradeGoodAvailability
    {
        public const string ImportProductionBuilding = "Import";

        public static bool IsImportOnly(TradeGoodEntry entry) =>
            string.Equals(entry.ProductionBuilding, ImportProductionBuilding, StringComparison.OrdinalIgnoreCase);

        public static bool IsImportOnly(string? productionBuilding) =>
            string.Equals(productionBuilding, ImportProductionBuilding, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Goods unlocked by existing city buildings and active terrain improvements
        /// whose name matches <see cref="TradeGoodEntry.ProductionBuilding"/> (exact, case-insensitive).
        /// Import-only catalog entries never match.
        /// </summary>
        public static HashSet<string> ProducedKeysFromFacilityNames(IEnumerable<string?> facilityNames)
        {
            var owned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in facilityNames)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    owned.Add(name.Trim());
            }

            var produced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (owned.Count == 0)
                return produced;

            foreach (var good in TradeGoodsCatalog.All)
            {
                if (IsImportOnly(good))
                    continue;
                if (string.IsNullOrWhiteSpace(good.ProductionBuilding))
                    continue;
                if (owned.Contains(good.ProductionBuilding.Trim()))
                    produced.Add(good.Key);
            }

            return produced;
        }

        /// <summary>
        /// Map pins store <c>Name</c> as the map kind (e.g. Sawmill) and the catalog template
        /// in <c>Description</c> (e.g. Sawmill - common). Trade goods match the catalog name.
        /// Yield both so production unlocks work for improvements and city buildings.
        /// </summary>
        public static IEnumerable<string?> FacilityNamesFromMapImprovement(string? name, string? description)
        {
            if (!string.IsNullOrWhiteSpace(name))
                yield return name;
            if (!string.IsNullOrWhiteSpace(description)
                && !string.Equals(name?.Trim(), description.Trim(), StringComparison.OrdinalIgnoreCase))
                yield return description;
        }

        public static TradeGoodAvailabilitySnapshot Resolve(
            IEnumerable<string?> facilityNames,
            IEnumerable<BaronyTradeTreaty> treaties,
            IEnumerable<string>? mgOverrideKeys)
        {
            var produced = ProducedKeysFromFacilityNames(facilityNames);
            var treaty = new HashSet<string>(
                TradeTreatyCalculator.BaronyReceivedGoods(treaties).Select(g => g.Key),
                StringComparer.OrdinalIgnoreCase);
            var overrides = NormalizeOverrideKeys(mgOverrideKeys);

            var available = new HashSet<string>(produced, StringComparer.OrdinalIgnoreCase);
            available.UnionWith(treaty);
            available.UnionWith(overrides);

            var grantable = new HashSet<string>(produced, StringComparer.OrdinalIgnoreCase);
            grantable.UnionWith(overrides);

            return new TradeGoodAvailabilitySnapshot
            {
                ProducedKeys = produced,
                TreatyReceivedKeys = treaty,
                OverrideKeys = overrides,
                AvailableKeys = available,
                GrantableKeys = grantable,
            };
        }

        public static HashSet<string> NormalizeOverrideKeys(IEnumerable<string>? keys)
        {
            var known = new HashSet<string>(
                TradeGoodsCatalog.All.Select(g => g.Key),
                StringComparer.OrdinalIgnoreCase);
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (keys is null)
                return result;

            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                var trimmed = key.Trim();
                if (known.Contains(trimmed))
                    result.Add(trimmed);
            }

            return result;
        }

        public const string DomainPanelRowLabel = "Trade Goods and treaties";

        /// <summary>
        /// Single Domain Panel (Decrees and Technologies) row: sum of available goods, luxury,
        /// route economy, and net treaty gold (customs / sweeteners).
        /// </summary>
        public static List<(string Label, PpbVector Additive, PpbVector Percent, string? Note)> DomainPanelBonusParts(
            TradeGoodAvailabilitySnapshot availability,
            IEnumerable<BaronyTradeTreaty> treaties,
            string? luxuryAccessKey)
        {
            var treatyList = treaties as IList<BaronyTradeTreaty> ?? treaties.ToList();
            var additive = new PpbVector();
            var percent = new PpbVector();
            var noteParts = new List<string>();

            var availableGoods = TradeGoodsCatalog.All
                .Where(g => availability.AvailableKeys.Contains(g.Key))
                .OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (availableGoods.Count > 0)
            {
                TradeGoodsBonusAggregator.Sum(availableGoods, out var goodsAdd, out var goodsPct);
                additive.AddInPlace(goodsAdd);
                percent.AddInPlace(goodsPct);
                noteParts.Add($"{availableGoods.Count} available good{(availableGoods.Count == 1 ? "" : "s")}");
            }

            var luxury = LuxuryGoodsAccessCatalog.Find(luxuryAccessKey);
            if (PpbCatalog.All.Any(info => luxury.BonusAdditive[info.Key] != 0m))
            {
                additive.AddInPlace(luxury.BonusAdditive);
                noteParts.Add($"luxury — {luxury.NameEn}");
            }

            var routeEconomy = TradeTreatyCalculator.TotalRouteEconomyBonus(treatyList);
            if (routeEconomy != 0m)
            {
                additive[Ppb.Economy] += routeEconomy;
                noteParts.Add($"route economy {PpbFormat.Additive(routeEconomy)}");
            }

            // Positive outflow = gold leaving the barony → negative Treasury on Domain Panel.
            var goldOutflow = TradeTreatyCalculator.TotalGoldOutflowPerTurn(treatyList);
            if (goldOutflow != 0m)
            {
                additive[Ppb.Treasury] -= goldOutflow;
                noteParts.Add($"customs & sweeteners {PpbFormat.Additive(-goldOutflow)} Gold");
            }

            if (!PpbCatalog.All.Any(info => additive[info.Key] != 0m || percent[info.Key] != 0m))
                return new List<(string Label, PpbVector Additive, PpbVector Percent, string? Note)>();

            return new List<(string Label, PpbVector Additive, PpbVector Percent, string? Note)>
            {
                (
                    DomainPanelRowLabel,
                    additive,
                    percent,
                    noteParts.Count == 0 ? null : string.Join(" · ", noteParts)),
            };
        }
    }
}
