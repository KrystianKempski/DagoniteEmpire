using System.Reflection;
using System.Text.Json;

namespace DA_Common.Barony
{
    public static class EasternMarchMapDefaults
    {
        private const string ResourceName = "DA_Common.Barony.SeedData.eastern-march-map.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Authored Eastern March map (hand-placed node positions + road/river links) shared by every
        /// barony. Falls back to a procedural layout only if the embedded resource is missing/unreadable.
        /// </summary>
        public static MarchMapDocument CreateSeedDocument()
        {
            return LoadAuthoredDocument() ?? CreateProceduralDocument();
        }

        private static MarchMapDocument? LoadAuthoredDocument()
        {
            var assembly = typeof(EasternMarchMapDefaults).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName);
            if (stream is null)
                return null;

            try
            {
                using var reader = new StreamReader(stream);
                var doc = JsonSerializer.Deserialize<MarchMapDocument>(reader.ReadToEnd(), JsonOptions);
                if (doc is null || doc.Nodes is null || doc.Nodes.Count == 0)
                    return null;
                return doc;
            }
            catch
            {
                return null;
            }
        }

        private static MarchMapDocument CreateProceduralDocument()
        {
            var doc = new MarchMapDocument();
            var byHolding = new Dictionary<string, MarchMapNode>(StringComparer.OrdinalIgnoreCase);
            var col = 0;
            var row = 0;
            const int cols = 8;

            foreach (var lord in KnownLordsCatalog.EasternMarch)
            {
                var holding = lord.Holdings.Trim();
                if (holding.Length == 0 || byHolding.ContainsKey(holding))
                    continue;

                var id = Slug(holding);
                var x = 80 + col * 110;
                var y = 80 + row * 95;
                col++;
                if (col >= cols)
                {
                    col = 0;
                    row++;
                }

                byHolding[holding] = new MarchMapNode
                {
                    Id = id,
                    Label = holding,
                    Kind = InferPlaceKind(lord, holding),
                    LordKey = KnownLordsCatalog.LordKey(lord),
                    X = Math.Clamp(x, 40, 960),
                    Y = Math.Clamp(y, 40, 960),
                };
            }

            doc.Nodes = byHolding.Values.OrderBy(n => n.Label, StringComparer.OrdinalIgnoreCase).ToList();

            var nodeByLord = doc.Nodes
                .Where(n => !string.IsNullOrWhiteSpace(n.LordKey))
                .ToDictionary(n => n.LordKey!, StringComparer.OrdinalIgnoreCase);

            foreach (var (a, b) in EasternMarchLordAdjacency.SeedPairs)
            {
                if (!nodeByLord.TryGetValue(a, out var from) || !nodeByLord.TryGetValue(b, out var to))
                    continue;

                doc.Routes.Add(new MarchMapRoute
                {
                    Id = Guid.NewGuid().ToString("N"),
                    FromNodeId = from.Id,
                    ToNodeId = to.Id,
                    Kind = MarchRouteKind.Road,
                });
            }

            return doc;
        }

        private static string InferPlaceKind(KnownLordEntry lord, string holding)
        {
            if (string.Equals(holding, "Warrington", StringComparison.OrdinalIgnoreCase))
                return MarchMapNodeKind.MarchCapital;
            if (holding.Contains("village", StringComparison.OrdinalIgnoreCase))
                return MarchMapNodeKind.Village;
            if (lord.Wealth >= 6)
                return MarchMapNodeKind.LargeCity;
            return MarchMapNodeKind.City;
        }

        private static string Slug(string text)
        {
            var chars = text.ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray();
            return string.Concat(chars).Trim('-');
        }
    }
}
