using DA_Common.Localization;

namespace DA_Common.Barony
{
    /// <summary>Combat “Other” column keys (Excel Inne).</summary>
    public static class UnitCombatStatKey
    {
        public const string Attack = "attack";
        public const string Defense = "defense";
        public const string Damage = "damage";
        public const string Move = "move";
        public const string Armor = "armor";
        public const string Hp = "hp";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Attack, Defense, Damage, Move, Armor, Hp,
        };

        public static string Label(string? key) => key?.Trim().ToLowerInvariant() switch
        {
            Attack => "Attack",
            Defense => "Defense",
            Damage => "Damage",
            Move => "Move",
            Armor => "Armor",
            Hp => "Hit points",
            _ => key ?? "Other",
        };
    }

    /// <summary>Named source contributing to a combat Other total.</summary>
    public sealed class UnitCombatModifierEntry
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public static class UnitCombatOtherFormulas
    {
        public static int Sum(IEnumerable<UnitCombatModifierEntry>? entries) =>
            entries?.Sum(e => e.Value) ?? 0;

        public static List<UnitCombatModifierEntry> Get(
            IReadOnlyDictionary<string, List<UnitCombatModifierEntry>>? map,
            string statKey)
        {
            if (map is null || !map.TryGetValue(statKey, out var list) || list is null)
                return new List<UnitCombatModifierEntry>();
            return list;
        }

        public static string TooltipLines(IEnumerable<UnitCombatModifierEntry>? entries)
        {
            var list = entries?.Where(e => !string.IsNullOrWhiteSpace(e.Label) || e.Value != 0).ToList()
                ?? new List<UnitCombatModifierEntry>();
            if (list.Count == 0)
                return Loc.T("No other modifiers.");

            return string.Join("\n", list.Select(e =>
            {
                var name = string.IsNullOrWhiteSpace(e.Label) ? Loc.T("(unnamed)") : e.Label.Trim();
                var sign = e.Value > 0 ? "+" : "";
                return $"{name}: {sign}{e.Value}";
            }));
        }

        public static void ApplySkillOtherTotals(
            Dictionary<string, List<UnitCombatModifierEntry>> sources,
            Dictionary<string, int> skillOther)
        {
            foreach (var key in sources.Keys.ToList())
            {
                var sum = Sum(Get(sources, key));
                if (sum == 0 && Get(sources, key).Count == 0)
                    skillOther.Remove(key);
                else
                    skillOther[key] = sum;
            }
        }

        public static void ApplyAttrOtherTotals(
            IReadOnlyDictionary<string, List<UnitCombatModifierEntry>>? sources,
            out int build,
            out int agility,
            out int will,
            out int perception)
        {
            build = Sum(Get(sources, UnitAttr.Build));
            agility = Sum(Get(sources, UnitAttr.Agility));
            will = Sum(Get(sources, UnitAttr.Will));
            perception = Sum(Get(sources, UnitAttr.Perception));
        }

        public static (int Attack, int Defense, int Damage, int Move, int Armor, int Hp) SumAll(
            IReadOnlyDictionary<string, List<UnitCombatModifierEntry>>? combatOther) =>
        (
            Sum(Get(combatOther, UnitCombatStatKey.Attack)),
            Sum(Get(combatOther, UnitCombatStatKey.Defense)),
            Sum(Get(combatOther, UnitCombatStatKey.Damage)),
            Sum(Get(combatOther, UnitCombatStatKey.Move)),
            Sum(Get(combatOther, UnitCombatStatKey.Armor)),
            Sum(Get(combatOther, UnitCombatStatKey.Hp))
        );
    }
}
