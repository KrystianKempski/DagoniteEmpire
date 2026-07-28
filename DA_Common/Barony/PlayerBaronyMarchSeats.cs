namespace DA_Common.Barony
{
    /// <summary>
    /// Player baronies seeded in the app and their matching Eastern March map seats (holdings labels).
    /// </summary>
    public static class PlayerBaronyMarchSeats
    {
        public static readonly string[] SeedBaronyNames =
        {
            "Darkhold",
            "Bonefyre",
            "Blackhammer",
            "LoneHill",
        };

        private static readonly Dictionary<string, string> MapLabelByBaronyName =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Darkhold"] = "Darkhold",
                ["Bonefyre"] = "Bonefyre",
                ["Blackhammer"] = "Blackhammer",
                ["LoneHill"] = "Lonehill",
                ["Lonehill"] = "Lonehill",
            };

        public static string? MapLabelForBaronyName(string? baronyName)
        {
            if (string.IsNullOrWhiteSpace(baronyName))
                return null;

            var trimmed = baronyName.Trim();
            if (MapLabelByBaronyName.TryGetValue(trimmed, out var label))
                return label;

            var compact = CompactName(trimmed);
            foreach (var pair in MapLabelByBaronyName)
            {
                if (string.Equals(CompactName(pair.Key), compact, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(CompactName(pair.Value), compact, StringComparison.OrdinalIgnoreCase))
                    return pair.Value;
            }

            return null;
        }

        public static string? ResolveSeatNodeId(IEnumerable<MarchMapNode> nodes, string? baronyName) =>
            FindSeatNode(nodes, baronyName)?.Id;

        public static MarchMapNode? FindSeatNode(IEnumerable<MarchMapNode> nodes, string? baronyName)
        {
            var mapLabel = MapLabelForBaronyName(baronyName);
            if (mapLabel is null)
                return null;

            var list = nodes as IList<MarchMapNode> ?? nodes.ToList();

            foreach (var node in list)
            {
                if (LabelsMatch(node.Label, mapLabel))
                    return node;
            }

            var lordKey = KnownLordsCatalog.LordKeyForPlaceLabel(mapLabel);
            if (lordKey is not null)
            {
                var byKey = list.FirstOrDefault(n =>
                    string.Equals(n.LordKey, lordKey, StringComparison.OrdinalIgnoreCase));
                if (byKey is not null)
                    return byKey;

                var holdings = KnownLordsCatalog.FindByKey(lordKey)?.Holdings;
                if (!string.IsNullOrWhiteSpace(holdings))
                {
                    return list.FirstOrDefault(n => LabelsMatch(n.Label, holdings));
                }
            }

            return null;
        }

        public static bool IsPlayerSeat(MarchMapNode node, string? playerSeatNodeId) =>
            !string.IsNullOrWhiteSpace(playerSeatNodeId) &&
            string.Equals(node.Id, playerSeatNodeId, StringComparison.OrdinalIgnoreCase);

        private static bool LabelsMatch(string? a, string? b) =>
            !string.IsNullOrWhiteSpace(a) &&
            !string.IsNullOrWhiteSpace(b) &&
            string.Equals(CompactName(a), CompactName(b), StringComparison.OrdinalIgnoreCase);

        private static string CompactName(string value) =>
            new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }

    public sealed class MarchMapPlayerBaronyContext
    {
        public string BaronyName { get; init; } = string.Empty;
        public string BaronName { get; init; } = string.Empty;
        public string SeatMapLabel { get; init; } = string.Empty;
    }
}
