using System.Text.Json;

namespace DA_Common.Barony
{
    /// <summary>
    /// Which Domain PPBs receive the baron's full influence this turn.
    /// Unfocused PPBs (except Gold) apply at half. Gold is never focusable.
    /// Slot count = floor(management BT / 20).
    /// </summary>
    public static class BaronFocusPpbs
    {
        public const decimal UnfocusedShare = 0.5m;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static readonly IReadOnlyList<Ppb> Focusable = PpbCatalog.All
            .Where(p => p.Key != Ppb.Treasury)
            .Select(p => p.Key)
            .ToList();

        public static bool IsFocusable(Ppb key) => key != Ppb.Treasury;

        /// <summary>Default focuses when none saved yet (first N focusable PPBs).</summary>
        public static IReadOnlyList<Ppb> DefaultFocuses(int slotCount)
        {
            if (slotCount <= 0)
                return Array.Empty<Ppb>();
            return Focusable.Take(slotCount).ToList();
        }

        /// <summary>
        /// Clamped focus list for the current management BT.
        /// Does not invent defaults — empty saved list means all focusable PPBs at ×½.
        /// </summary>
        public static List<Ppb> Effective(
            IEnumerable<Ppb>? saved,
            int managementJc)
        {
            var slots = BaronTimeRules.FocusSlotCount(managementJc);
            if (slots <= 0)
                return new List<Ppb>();

            return (saved ?? Enumerable.Empty<Ppb>())
                .Where(IsFocusable)
                .Distinct()
                .Where(p => Focusable.Contains(p))
                .Take(slots)
                .ToList();
        }

        public static decimal ShareFor(Ppb key, IReadOnlyCollection<Ppb> focused)
        {
            if (!IsFocusable(key))
                return 1m;
            if (focused is null || focused.Count == 0)
                return UnfocusedShare;
            return focused.Contains(key) ? 1m : UnfocusedShare;
        }

        /// <summary>Multiply each focusable PPB by 1 or ½ according to focus.</summary>
        public static void ApplyToSkillTotals(PpbVector skills, IReadOnlyCollection<Ppb> focused)
        {
            if (skills is null)
                return;
            skills.EnsureSize();
            foreach (var key in Focusable)
            {
                var share = ShareFor(key, focused);
                if (share == 1m)
                    continue;
                skills[key] = decimal.Round(skills[key] * share, 0, MidpointRounding.AwayFromZero);
            }
        }

        public static string Serialize(IEnumerable<Ppb>? skills)
        {
            var list = (skills ?? Enumerable.Empty<Ppb>())
                .Where(IsFocusable)
                .Distinct()
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
                    if (Enum.TryParse<Ppb>(name, ignoreCase: true, out var ppb) && IsFocusable(ppb))
                        result.Add(ppb);
                }
                return result.Distinct().ToList();
            }
            catch
            {
                return new List<Ppb>();
            }
        }
    }
}
