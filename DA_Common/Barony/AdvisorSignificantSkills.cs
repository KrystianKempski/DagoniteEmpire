using System.Text.Json;

namespace DA_Common.Barony
{
    /// <summary>Which administrative skills matter for a given advisor office.</summary>
    public static class AdvisorSignificantSkills
    {
        public const int MaxCount = 3;

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static IReadOnlyList<Ppb> DefaultForOffice(string officeType) => officeType switch
        {
            OfficeType.Chancellor => new[] { Ppb.Loyalty, Ppb.Stability, Ppb.Culture },
            OfficeType.GuardCaptain => new[] { Ppb.Law, Ppb.Corruption, Ppb.Defense },
            OfficeType.Steward => new[] { Ppb.Food, Ppb.Production, Ppb.Economy },
            _ => Array.Empty<Ppb>(),
        };

        public static IEnumerable<PpbInfo> SelectableSkills
            => PpbCatalog.All.Where(p => p.Key != Ppb.Treasury);

        /// <summary>
        /// Persist significant skill picks. Duplicates are allowed (each pick is a focus claim).
        /// </summary>
        public static string Serialize(IEnumerable<Ppb>? skills)
        {
            var list = (skills ?? Enumerable.Empty<Ppb>())
                .Where(p => p != Ppb.Treasury)
                .Take(MaxCount)
                .ToList();
            return JsonSerializer.Serialize(list.Select(p => p.ToString()).ToList(), JsonOptions);
        }

        public static List<Ppb> Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<Ppb>();

            try
            {
                var names = JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>();
                var result = new List<Ppb>();
                foreach (var name in names)
                {
                    if (result.Count >= MaxCount)
                        break;
                    if (Enum.TryParse<Ppb>(name, ignoreCase: true, out var ppb) && ppb != Ppb.Treasury)
                        result.Add(ppb);
                }
                return result;
            }
            catch
            {
                return new List<Ppb>();
            }
        }

        public static PpbVector MaskToSignificant(PpbVector skills, IEnumerable<Ppb> significant)
        {
            var set = significant as IReadOnlySet<Ppb> ?? significant.ToHashSet();
            var masked = skills.Clone();
            foreach (var info in PpbCatalog.All)
            {
                if (info.Key == Ppb.Treasury)
                    continue;
                if (!set.Contains(info.Key))
                    masked[info.Key] = 0m;
            }
            return masked;
        }

        public static int PickCount(IEnumerable<Ppb>? significant, Ppb key)
            => significant?.Count(p => p == key) ?? 0;
    }
}
