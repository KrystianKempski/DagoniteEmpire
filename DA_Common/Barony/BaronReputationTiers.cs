using System.Text;
using DA_Common.Localization;
using static DA_Common.Barony.BaronSkillPpbFormulas;

namespace DA_Common.Barony
{
    /// <summary>One Prestige / Honor / Fear reputation tier and its barony PPB + character skill bonuses.</summary>
    public sealed class ReputationTier
    {
        private string _name = "";
        private string _thresholdLabel = "";
        private string _bonusSummary = "";

        // Backing values store the English key; getters localize per current culture.
        public required string Name
        {
            get => Loc.T(_name);
            init => _name = value;
        }

        /// <summary>Minimum score to reach this tier (inclusive). Highest matching tier wins.</summary>
        public int MinRequired { get; init; }

        /// <summary>Human-readable threshold for the table (e.g. “less than 300”, “1000”).</summary>
        public required string ThresholdLabel
        {
            get => Loc.T(_thresholdLabel);
            init => _thresholdLabel = value;
        }

        public PpbVector BaronyBonus { get; init; } = new();

        /// <summary>Special-skill deltas applied on the baron’s character sheet (Temp column).</summary>
        public IReadOnlyList<(string Skill, int Value)> SkillBonuses { get; init; } = [];

        /// <summary>Localized bonus blurb for tooltips / detail panels (English key, falls back to English).</summary>
        public required string BonusSummary
        {
            get => Loc.T(_bonusSummary);
            init => _bonusSummary = value;
        }

        public string? SkillBonusText
        {
            get
            {
                if (SkillBonuses.Count == 0)
                    return null;
                return string.Join(", ", SkillBonuses.Select(s =>
                {
                    var sign = s.Value > 0 ? "+" : "";
                    return $"{sign}{s.Value} {LocCatalog.Name(s.Skill)}";
                }));
            }
        }
    }

    /// <summary>Prestige, Honor, and Fear tier ladders → PPB and character skill bonuses.</summary>
    public static class BaronReputationTiers
    {
        /// <summary>Fear tables say “Command”; the character sheet skill is Inspire.</summary>
        public const string CommandSkill = Skills.Inspire;

        public static readonly IReadOnlyList<ReputationTier> Prestige =
        [
            new()
            {
                Name = "Nobody",
                MinRequired = int.MinValue,
                ThresholdLabel = "less than 300",
                BonusSummary = "No bonuses or penalties.",
            },
            new()
            {
                Name = "Known locally",
                MinRequired = 300,
                ThresholdLabel = "300",
                BaronyBonus = Vec(
                    (Ppb.Stability, 3), (Ppb.Loyalty, 3),
                    (Ppb.Culture, 5), (Ppb.Science, 5), (Ppb.Magic, 5)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 1),
                BonusSummary =
                    "+3 Stability and Loyalty; +5 Culture, Science, and Magic; +1 Diplomacy and Persuasion.",
            },
            new()
            {
                Name = "Popular",
                MinRequired = 1000,
                ThresholdLabel = "1000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 5), (Ppb.Loyalty, 5),
                    (Ppb.Culture, 8), (Ppb.Science, 8), (Ppb.Magic, 8)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 2),
                BonusSummary =
                    "+5 Stability and Loyalty; +8 Culture, Science, and Magic; +2 Diplomacy and Persuasion.",
            },
            new()
            {
                Name = "Famous",
                MinRequired = 3000,
                ThresholdLabel = "3000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 8), (Ppb.Loyalty, 8),
                    (Ppb.Culture, 15), (Ppb.Science, 15), (Ppb.Magic, 15)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 3),
                BonusSummary =
                    "+8 Stability and Loyalty; +15 Culture, Science, and Magic; +3 Diplomacy and Persuasion.",
            },
            new()
            {
                Name = "Known by everyone",
                MinRequired = 5000,
                ThresholdLabel = "5000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 12), (Ppb.Loyalty, 12),
                    (Ppb.Culture, 30), (Ppb.Science, 30), (Ppb.Magic, 30)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 4),
                BonusSummary =
                    "+12 Stability and Loyalty; +30 Culture, Science, and Magic; +4 Diplomacy and Persuasion.",
            },
            new()
            {
                Name = "Living Legend",
                MinRequired = 10000,
                ThresholdLabel = "10000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 15), (Ppb.Loyalty, 15),
                    (Ppb.Culture, 50), (Ppb.Science, 50), (Ppb.Magic, 50)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 5),
                BonusSummary =
                    "+15 Stability and Loyalty; +50 Culture, Science, and Magic; +5 Diplomacy and Persuasion.",
            },
        ];

        public static readonly IReadOnlyList<ReputationTier> Honor =
        [
            new()
            {
                Name = "Lying traitor",
                MinRequired = int.MinValue,
                ThresholdLabel = "−150",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, -15), (Ppb.Corruption, 5),
                    (Ppb.Economy, -15), (Ppb.Defense, -15), (Ppb.Intelligence, -15)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, -3),
                BonusSummary =
                    "−15 Loyalty and +5 Corruption; −15 Economy, Defense, and Intelligence; −3 Bluff and Trade.",
            },
            new()
            {
                Name = "Without a shred of honor",
                MinRequired = -100,
                ThresholdLabel = "−100",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, -8), (Ppb.Corruption, 3),
                    (Ppb.Economy, -8), (Ppb.Defense, -8), (Ppb.Intelligence, -8)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, -2),
                BonusSummary =
                    "−8 Loyalty and +3 Corruption; −8 Economy, Defense, and Intelligence; −2 Bluff and Trade.",
            },
            new()
            {
                Name = "Of doubtful nature",
                MinRequired = -50,
                ThresholdLabel = "−50",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, -3), (Ppb.Corruption, 1),
                    (Ppb.Economy, -3), (Ppb.Defense, -3), (Ppb.Intelligence, -3)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, -1),
                BonusSummary =
                    "−3 Loyalty and +1 Corruption; −3 Economy, Defense, and Intelligence; −1 Bluff and Trade.",
            },
            new()
            {
                Name = "Undefined",
                MinRequired = 0,
                ThresholdLabel = "0",
                BonusSummary = "No bonuses or penalties.",
            },
            new()
            {
                Name = "Renowned",
                MinRequired = 100,
                ThresholdLabel = "100",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, 3), (Ppb.Corruption, -1),
                    (Ppb.Economy, 3), (Ppb.Defense, 3), (Ppb.Intelligence, 3)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, 1),
                BonusSummary =
                    "+3 Loyalty and −1 Corruption; +3 Economy, Defense, and Intelligence; +1 Bluff and Trade.",
            },
            new()
            {
                Name = "Honest",
                MinRequired = 300,
                ThresholdLabel = "300",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, 5), (Ppb.Corruption, -3),
                    (Ppb.Economy, 8), (Ppb.Defense, 8), (Ppb.Intelligence, 8)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, 2),
                BonusSummary =
                    "+5 Loyalty and −3 Corruption; +8 Economy, Defense, and Intelligence; +2 Bluff and Trade.",
            },
            new()
            {
                Name = "Honorable",
                MinRequired = 600,
                ThresholdLabel = "600",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, 8), (Ppb.Corruption, -5),
                    (Ppb.Economy, 15), (Ppb.Defense, 15), (Ppb.Intelligence, 15)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, 3),
                BonusSummary =
                    "+8 Loyalty and −5 Corruption; +15 Economy, Defense, and Intelligence; +3 Bluff and Trade.",
            },
            new()
            {
                Name = "Immaculate",
                MinRequired = 1000,
                ThresholdLabel = "1000",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, 15), (Ppb.Corruption, -10),
                    (Ppb.Economy, 30), (Ppb.Defense, 30), (Ppb.Intelligence, 30)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, 5),
                BonusSummary =
                    "+15 Loyalty and −10 Corruption; +30 Economy, Defense, and Intelligence; +5 Bluff and Trade.",
            },
        ];

        public static readonly IReadOnlyList<ReputationTier> Fear =
        [
            new()
            {
                Name = "Joke",
                MinRequired = int.MinValue,
                ThresholdLabel = "−150",
                BaronyBonus = Vec(
                    (Ppb.Stability, -15), (Ppb.Law, -15),
                    (Ppb.Production, -15), (Ppb.Food, -15), (Ppb.Defense, -15)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, -3),
                BonusSummary =
                    "−15 Stability and Law; −15 Production, Food, and Defense; −3 Intimidate and Inspire (Command).",
            },
            new()
            {
                Name = "Warm dumpling",
                MinRequired = -100,
                ThresholdLabel = "−100",
                BaronyBonus = Vec(
                    (Ppb.Stability, -8), (Ppb.Law, -8),
                    (Ppb.Production, -8), (Ppb.Food, -8), (Ppb.Defense, -8)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, -2),
                BonusSummary =
                    "−8 Stability and Law; −8 Production, Food, and Defense; −2 Intimidate and Inspire (Command).",
            },
            new()
            {
                Name = "Harmless",
                MinRequired = -50,
                ThresholdLabel = "−50",
                BaronyBonus = Vec(
                    (Ppb.Stability, -3), (Ppb.Law, -3),
                    (Ppb.Production, -3), (Ppb.Food, -3), (Ppb.Defense, -3)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, -1),
                BonusSummary =
                    "−3 Stability and Law; −3 Production, Food, and Defense; −1 Intimidate and Inspire (Command).",
            },
            new()
            {
                Name = "Undefined",
                MinRequired = 0,
                ThresholdLabel = "0",
                BonusSummary = "No bonuses or penalties.",
            },
            new()
            {
                Name = "Unsettling",
                MinRequired = 100,
                ThresholdLabel = "100",
                BaronyBonus = Vec(
                    (Ppb.Stability, 3), (Ppb.Law, 3),
                    (Ppb.Production, 3), (Ppb.Food, 3), (Ppb.Defense, 3)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, 1),
                BonusSummary =
                    "+3 Stability and Law; +3 Production, Food, and Defense; +1 Intimidate and Inspire (Command).",
            },
            new()
            {
                Name = "Dangerous",
                MinRequired = 300,
                ThresholdLabel = "300",
                BaronyBonus = Vec(
                    (Ppb.Stability, 5), (Ppb.Law, 5),
                    (Ppb.Production, 8), (Ppb.Food, 8), (Ppb.Defense, 8)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, 2),
                BonusSummary =
                    "+5 Stability and Law; +8 Production, Food, and Defense; +2 Intimidate and Inspire (Command).",
            },
            new()
            {
                Name = "Terror",
                MinRequired = 600,
                ThresholdLabel = "600",
                BaronyBonus = Vec(
                    (Ppb.Stability, 8), (Ppb.Law, 8),
                    (Ppb.Production, 15), (Ppb.Food, 15), (Ppb.Defense, 15)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, 3),
                BonusSummary =
                    "+8 Stability and Law; +15 Production, Food, and Defense; +3 Intimidate and Inspire (Command).",
            },
            new()
            {
                Name = "Walking terror",
                MinRequired = 1000,
                ThresholdLabel = "1000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 15), (Ppb.Law, 15),
                    (Ppb.Production, 30), (Ppb.Food, 30), (Ppb.Defense, 30)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, 5),
                BonusSummary =
                    "+15 Stability and Law; +30 Production, Food, and Defense; +5 Intimidate and Inspire (Command).",
            },
        ];

        public static ReputationTier Resolve(IReadOnlyList<ReputationTier> tiers, int score)
        {
            ReputationTier current = tiers[0];
            foreach (var tier in tiers)
            {
                if (score >= tier.MinRequired)
                    current = tier;
                else
                    break;
            }

            return current;
        }

        public static ReputationTier ResolvePrestige(int score) => Resolve(Prestige, score);
        public static ReputationTier ResolveHonor(int score) => Resolve(Honor, score);
        public static ReputationTier ResolveFear(int score) => Resolve(Fear, score);

        /// <summary>Combined barony PPB from all three ladders.</summary>
        public static PpbVector InfluenceFromScores(int prestige, int honor, int fear)
        {
            var total = new PpbVector();
            total.AddInPlace(ResolvePrestige(prestige).BaronyBonus);
            total.AddInPlace(ResolveHonor(honor).BaronyBonus);
            total.AddInPlace(ResolveFear(fear).BaronyBonus);
            return total;
        }

        /// <summary>Merged special-skill deltas from active Prestige, Honor, and Fear tiers.</summary>
        public static IReadOnlyDictionary<string, int> SkillBonusesFromScores(int prestige, int honor, int fear)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            AddSkills(map, ResolvePrestige(prestige).SkillBonuses);
            AddSkills(map, ResolveHonor(honor).SkillBonuses);
            AddSkills(map, ResolveFear(fear).SkillBonuses);
            return map;
        }

        public static string FormatBaronyBonus(PpbVector bonus)
        {
            var parts = new List<string>();
            foreach (var info in PpbCatalog.All)
            {
                if (info.Key == Ppb.Treasury)
                    continue;
                var v = bonus[info.Key];
                if (v == 0m)
                    continue;
                var sign = v > 0 ? "+" : "";
                parts.Add($"{sign}{v:0.##} {info.Name}");
            }

            return parts.Count == 0 ? Loc.T("No barony PPB bonuses.") : string.Join(", ", parts);
        }

        public static string DescribeActiveTiers(int prestige, int honor, int fear)
        {
            var p = ResolvePrestige(prestige);
            var h = ResolveHonor(honor);
            var f = ResolveFear(fear);
            var sb = new StringBuilder();
            sb.AppendLine(Loc.T("Prestige: {0} (score {1}) — {2}", p.Name, prestige, p.BonusSummary));
            sb.AppendLine(Loc.T("Honor: {0} (score {1}) — {2}", h.Name, honor, h.BonusSummary));
            sb.Append(Loc.T("Fear: {0} (score {1}) — {2}", f.Name, fear, f.BonusSummary));
            return sb.ToString();
        }

        private static void AddSkills(Dictionary<string, int> map, IReadOnlyList<(string Skill, int Value)> bonuses)
        {
            foreach (var (skill, value) in bonuses)
            {
                if (value == 0 || string.IsNullOrWhiteSpace(skill))
                    continue;
                map[skill] = map.TryGetValue(skill, out var existing) ? existing + value : value;
            }
        }

        private static IReadOnlyList<(string Skill, int Value)> SkillsPair(string a, string b, int value) =>
            [(a, value), (b, value)];

        private static PpbVector Vec(params (Ppb key, decimal value)[] entries)
        {
            var v = new PpbVector();
            foreach (var (key, value) in entries)
                v[key] = value;
            return v;
        }
    }
}
