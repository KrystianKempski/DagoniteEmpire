namespace DA_Common.Barony
{
    /// <summary>
    /// English chronicle lines for cumulative resource stock changes
    /// (e.g. "Production increased by 13 from 20 to 33").
    /// </summary>
    public static class ResourceStockChangeLog
    {
        /// <summary>One narrative line per changed resource key, empty when nothing changed.</summary>
        public static IReadOnlyList<string> Describe(PpbVector? before, PpbVector? after)
        {
            var from = ResourceCatalog.Slice(before);
            var to = ResourceCatalog.Slice(after);
            var lines = new List<string>();
            foreach (var info in ResourceCatalog.All)
            {
                var a = PpbFormat.Round(from[info.Key]);
                var b = PpbFormat.Round(to[info.Key]);
                var delta = PpbFormat.Round(b - a);
                if (delta == 0m)
                    continue;
                lines.Add(DescribeOne(info.NameEn, a, delta, b));
            }
            return lines;
        }

        /// <summary>Compact resolve style: "Production: 20 → 33 (Δ +13)".</summary>
        public static IReadOnlyList<string> DescribeCompact(PpbVector? before, PpbVector? after)
        {
            var from = ResourceCatalog.Slice(before);
            var to = ResourceCatalog.Slice(after);
            var lines = new List<string>();
            foreach (var info in ResourceCatalog.All)
            {
                var a = PpbFormat.Round(from[info.Key]);
                var b = PpbFormat.Round(to[info.Key]);
                var delta = PpbFormat.Round(b - a);
                if (delta == 0m)
                    continue;
                lines.Add(
                    $"{info.NameEn}: {PpbFormat.Number(a)} → {PpbFormat.Number(b)} (Δ {PpbFormat.Additive(delta)})");
            }
            return lines;
        }

        /// <summary>
        /// Summary suitable for the chronicle (≤ <paramref name="maxLength"/> chars).
        /// Longer dumps go into <paramref name="details"/>.
        /// </summary>
        public static (string? Summary, string? Details) Summarize(
            PpbVector? before,
            PpbVector? after,
            string? preface = null,
            int maxLength = 400)
        {
            var lines = Describe(before, after);
            if (lines.Count == 0)
                return (null, null);

            var body = string.Join(" ", lines.Select(l => l.EndsWith('.') ? l : l + "."));
            var summary = string.IsNullOrWhiteSpace(preface)
                ? body
                : preface.TrimEnd('.', ' ') + ". " + body;

            if (summary.Length <= maxLength)
                return (summary, lines.Count > 1 ? string.Join("\n", lines) : null);

            var shortSummary = string.IsNullOrWhiteSpace(preface)
                ? $"Resource stocks changed ({lines.Count})."
                : preface.TrimEnd('.', ' ') + $" — {lines.Count} resource(s) changed.";
            if (shortSummary.Length > maxLength)
                shortSummary = shortSummary[..(maxLength - 1)] + "…";

            return (shortSummary, string.Join("\n", lines));
        }

        /// <summary>Additive amounts on a ledger row, e.g. "Prod +13, Gold −5".</summary>
        public static string? DescribeAdditive(PpbVector? additive)
        {
            var sliced = ResourceCatalog.Slice(additive);
            var parts = ResourceCatalog.All
                .Where(info => PpbFormat.Round(sliced[info.Key]) != 0m)
                .Select(info => $"{info.ShortEn} {PpbFormat.Additive(sliced[info.Key])}")
                .ToList();
            return parts.Count == 0 ? null : string.Join(", ", parts);
        }

        private static string DescribeOne(string name, decimal before, decimal delta, decimal after)
        {
            var verb = delta > 0m ? "increased" : "decreased";
            var amount = PpbFormat.Number(Math.Abs(delta));
            return $"{name} {verb} by {amount} from {PpbFormat.Number(before)} to {PpbFormat.Number(after)}";
        }
    }
}
