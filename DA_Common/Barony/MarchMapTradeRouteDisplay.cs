using DA_Common.Localization;

namespace DA_Common.Barony
{
    public sealed record MarchMapTradeRouteOverlay(
        string Id,
        string Name,
        string Color,
        IReadOnlyList<string> NodeIds);

    public static class MarchMapTradeRouteDisplay
    {
        private static readonly string[] Palette =
        {
            "#c45c5c",
            "#d4a017",
            "#5f8f5f",
            "#9b6b9e",
            "#c47a3a",
            "#5c8a8a",
            "#a67c52",
            "#7a6a9a",
            "#8b5a3c",
            "#4a7c59",
        };

        public static string ColorForTreaty(string treatyId, int index)
        {
            if (string.IsNullOrWhiteSpace(treatyId))
                return Palette[index % Palette.Length];

            unchecked
            {
                var hash = 17;
                foreach (var ch in treatyId)
                    hash = hash * 31 + char.ToLowerInvariant(ch);
                return Palette[Math.Abs(hash) % Palette.Length];
            }
        }

        public static string RouteName(BaronyTradeTreaty treaty)
        {
            string name;
            if (!string.IsNullOrWhiteSpace(treaty.Title))
                name = treaty.Title.Trim();
            else
            {
                var lord = KnownLordsCatalog.FindByKey(treaty.CounterpartyLordKey);
                name = lord is not null
                    ? Loc.T("Route to {0}", lord.Holdings)
                    : Loc.T("Trade route");
            }

            if (TradeTreatyApproval.IsPending(treaty))
                return Loc.T("{0} (pending)", name);

            return name;
        }

        public static IReadOnlyList<MarchMapTradeRouteOverlay> BuildOverlays(
            MarchMapDocument document,
            string? playerSeatNodeId,
            IReadOnlyList<BaronyTradeTreaty> treaties)
        {
            var list = new List<MarchMapTradeRouteOverlay>();
            if (string.IsNullOrWhiteSpace(playerSeatNodeId) || treaties.Count == 0)
                return list;

            for (var i = 0; i < treaties.Count; i++)
            {
                var treaty = treaties[i];
                var nodeIds = ResolveNodeIds(document, playerSeatNodeId, treaty);
                if (nodeIds.Count < 2)
                    continue;

                list.Add(new MarchMapTradeRouteOverlay(
                    treaty.Id,
                    RouteName(treaty),
                    ColorForTreaty(treaty.Id, i),
                    nodeIds));
            }

            return list;
        }

        public static IReadOnlyList<string> ResolveNodeIds(
            MarchMapDocument document,
            string playerSeatNodeId,
            BaronyTradeTreaty treaty)
        {
            var byLord = document.Nodes
                .Where(n => !string.IsNullOrWhiteSpace(n.LordKey))
                .GroupBy(n => n.LordKey!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var chain = new List<string> { playerSeatNodeId };

            var routeLords = TradeTreatyCalculator.RouteLordKeys(treaty);
            if (routeLords.Count == 0)
            {
                // Legacy: transit legs inside first paragraph + counterparty.
                var transit = treaty.Paragraphs.FirstOrDefault()?.TransitLegs
                    ?? (IReadOnlyList<TradeTreatyTransitLeg>)Array.Empty<TradeTreatyTransitLeg>();
                foreach (var leg in transit)
                {
                    if (!byLord.TryGetValue(leg.LordKey, out var node))
                        return Array.Empty<string>();
                    chain.Add(node.Id);
                }

                if (!byLord.TryGetValue(treaty.CounterpartyLordKey, out var end))
                    return Array.Empty<string>();
                chain.Add(end.Id);
            }
            else
            {
                foreach (var lordKey in routeLords)
                {
                    if (!byLord.TryGetValue(lordKey, out var node))
                        return Array.Empty<string>();
                    chain.Add(node.Id);
                }
            }

            var clean = new List<string>();
            foreach (var id in chain)
            {
                if (clean.Count == 0 || !string.Equals(clean[^1], id, StringComparison.OrdinalIgnoreCase))
                    clean.Add(id);
            }

            return clean;
        }
    }
}
