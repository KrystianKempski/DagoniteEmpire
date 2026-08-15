using System.Text;
using static DA_Common.Barony.BaronSkillPpbFormulas;

namespace DA_Common.Barony
{
    /// <summary>One Prestige / Honor / Fear reputation tier and its barony PPB + character skill bonuses.</summary>
    public sealed class ReputationTier
    {
        public required string Name { get; init; }

        /// <summary>Minimum score to reach this tier (inclusive). Highest matching tier wins.</summary>
        public int MinRequired { get; init; }

        /// <summary>Human-readable threshold for the table (e.g. “less than 300”, “1000”).</summary>
        public required string ThresholdLabel { get; init; }

        public PpbVector BaronyBonus { get; init; } = new();

        /// <summary>Special-skill deltas applied on the baron’s character sheet (Temp column).</summary>
        public IReadOnlyList<(string Skill, int Value)> SkillBonuses { get; init; } = [];

        /// <summary>Full English bonus blurb for tooltips / detail panels.</summary>
        public required string BonusSummary { get; init; }

        public string? SkillBonusText
        {
            get
            {
                if (SkillBonuses.Count == 0)
                    return null;
                return string.Join(", ", SkillBonuses.Select(s =>
                {
                    var sign = s.Value > 0 ? "+" : "";
                    return $"{sign}{s.Value} {s.Skill}";
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
                Name = "Nikt",
                MinRequired = int.MinValue,
                ThresholdLabel = "mniej niż 300",
                BonusSummary = "Bez premii ani kar.",
            },
            new()
            {
                Name = "Znany lokalnie",
                MinRequired = 300,
                ThresholdLabel = "300",
                BaronyBonus = Vec(
                    (Ppb.Stability, 3), (Ppb.Loyalty, 3),
                    (Ppb.Culture, 5), (Ppb.Science, 5), (Ppb.Magic, 5)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 1),
                BonusSummary =
                    "+3 Stabilność i Lojalność; +5 Kultura, Nauka i Magia; +1 Dyplomacja i Perswazja.",
            },
            new()
            {
                Name = "Popularny",
                MinRequired = 1000,
                ThresholdLabel = "1000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 5), (Ppb.Loyalty, 5),
                    (Ppb.Culture, 8), (Ppb.Science, 8), (Ppb.Magic, 8)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 2),
                BonusSummary =
                    "+5 Stabilność i Lojalność; +8 Kultura, Nauka i Magia; +2 Dyplomacja i Perswazja.",
            },
            new()
            {
                Name = "Sławny",
                MinRequired = 3000,
                ThresholdLabel = "3000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 8), (Ppb.Loyalty, 8),
                    (Ppb.Culture, 15), (Ppb.Science, 15), (Ppb.Magic, 15)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 3),
                BonusSummary =
                    "+8 Stabilność i Lojalność; +15 Kultura, Nauka i Magia; +3 Dyplomacja i Perswazja.",
            },
            new()
            {
                Name = "Znany wszystkim",
                MinRequired = 5000,
                ThresholdLabel = "5000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 12), (Ppb.Loyalty, 12),
                    (Ppb.Culture, 30), (Ppb.Science, 30), (Ppb.Magic, 30)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 4),
                BonusSummary =
                    "+12 Stabilność i Lojalność; +30 Kultura, Nauka i Magia; +4 Dyplomacja i Perswazja.",
            },
            new()
            {
                Name = "Żywa legenda",
                MinRequired = 10000,
                ThresholdLabel = "10000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 15), (Ppb.Loyalty, 15),
                    (Ppb.Culture, 50), (Ppb.Science, 50), (Ppb.Magic, 50)),
                SkillBonuses = SkillsPair(Skills.Diplomacy, Skills.Persuasion, 5),
                BonusSummary =
                    "+15 Stabilność i Lojalność; +50 Kultura, Nauka i Magia; +5 Dyplomacja i Perswazja.",
            },
        ];

        public static readonly IReadOnlyList<ReputationTier> Honor =
        [
            new()
            {
                Name = "Zdrajca",
                MinRequired = int.MinValue,
                ThresholdLabel = "−150",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, -15), (Ppb.Corruption, 5),
                    (Ppb.Economy, -15), (Ppb.Defense, -15), (Ppb.Intelligence, -15)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, -3),
                BonusSummary =
                    "−15 Lojalność i +5 Korupcja; −15 Ekonomia, Obrona i Wywiad; −3 Blef i Handel.",
            },
            new()
            {
                Name = "Bez cienia honoru",
                MinRequired = -100,
                ThresholdLabel = "−100",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, -8), (Ppb.Corruption, 3),
                    (Ppb.Economy, -8), (Ppb.Defense, -8), (Ppb.Intelligence, -8)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, -2),
                BonusSummary =
                    "−8 Lojalność i +3 Korupcja; −8 Ekonomia, Obrona i Wywiad; −2 Blef i Handel.",
            },
            new()
            {
                Name = "Wątpliwej natury",
                MinRequired = -50,
                ThresholdLabel = "−50",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, -3), (Ppb.Corruption, 1),
                    (Ppb.Economy, -3), (Ppb.Defense, -3), (Ppb.Intelligence, -3)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, -1),
                BonusSummary =
                    "−3 Lojalność i +1 Korupcja; −3 Ekonomia, Obrona i Wywiad; −1 Blef i Handel.",
            },
            new()
            {
                Name = "Nieokreślony",
                MinRequired = 0,
                ThresholdLabel = "0",
                BonusSummary = "Bez premii ani kar.",
            },
            new()
            {
                Name = "Poważany",
                MinRequired = 100,
                ThresholdLabel = "100",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, 3), (Ppb.Corruption, -1),
                    (Ppb.Economy, 3), (Ppb.Defense, 3), (Ppb.Intelligence, 3)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, 1),
                BonusSummary =
                    "+3 Lojalność i −1 Korupcja; +3 Ekonomia, Obrona i Wywiad; +1 Blef i Handel.",
            },
            new()
            {
                Name = "Uczciwy",
                MinRequired = 300,
                ThresholdLabel = "300",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, 5), (Ppb.Corruption, -3),
                    (Ppb.Economy, 8), (Ppb.Defense, 8), (Ppb.Intelligence, 8)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, 2),
                BonusSummary =
                    "+5 Lojalność i −3 Korupcja; +8 Ekonomia, Obrona i Wywiad; +2 Blef i Handel.",
            },
            new()
            {
                Name = "Honorowy",
                MinRequired = 600,
                ThresholdLabel = "600",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, 8), (Ppb.Corruption, -5),
                    (Ppb.Economy, 15), (Ppb.Defense, 15), (Ppb.Intelligence, 15)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, 3),
                BonusSummary =
                    "+8 Lojalność i −5 Korupcja; +15 Ekonomia, Obrona i Wywiad; +3 Blef i Handel.",
            },
            new()
            {
                Name = "Nieskazitelny",
                MinRequired = 1000,
                ThresholdLabel = "1000",
                BaronyBonus = Vec(
                    (Ppb.Loyalty, 15), (Ppb.Corruption, -10),
                    (Ppb.Economy, 30), (Ppb.Defense, 30), (Ppb.Intelligence, 30)),
                SkillBonuses = SkillsPair(Skills.Bluff, Skills.Trade, 5),
                BonusSummary =
                    "+15 Lojalność i −10 Korupcja; +30 Ekonomia, Obrona i Wywiad; +5 Blef i Handel.",
            },
        ];

        public static readonly IReadOnlyList<ReputationTier> Fear =
        [
            new()
            {
                Name = "Żart",
                MinRequired = int.MinValue,
                ThresholdLabel = "−150",
                BaronyBonus = Vec(
                    (Ppb.Stability, -15), (Ppb.Law, -15),
                    (Ppb.Production, -15), (Ppb.Food, -15), (Ppb.Defense, -15)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, -3),
                BonusSummary =
                    "−15 Stabilność i Prawo; −15 Produkcja, Wyżywienie i Obrona; −3 Zastraszanie i Inspiracja (Dowodzenie).",
            },
            new()
            {
                Name = "Ciepła klucha",
                MinRequired = -100,
                ThresholdLabel = "−100",
                BaronyBonus = Vec(
                    (Ppb.Stability, -8), (Ppb.Law, -8),
                    (Ppb.Production, -8), (Ppb.Food, -8), (Ppb.Defense, -8)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, -2),
                BonusSummary =
                    "−8 Stabilność i Prawo; −8 Produkcja, Wyżywienie i Obrona; −2 Zastraszanie i Inspiracja (Dowodzenie).",
            },
            new()
            {
                Name = "Nieszkodliwy",
                MinRequired = -50,
                ThresholdLabel = "−50",
                BaronyBonus = Vec(
                    (Ppb.Stability, -3), (Ppb.Law, -3),
                    (Ppb.Production, -3), (Ppb.Food, -3), (Ppb.Defense, -3)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, -1),
                BonusSummary =
                    "−3 Stabilność i Prawo; −3 Produkcja, Wyżywienie i Obrona; −1 Zastraszanie i Inspiracja (Dowodzenie).",
            },
            new()
            {
                Name = "Nieokreślony",
                MinRequired = 0,
                ThresholdLabel = "0",
                BonusSummary = "Bez premii ani kar.",
            },
            new()
            {
                Name = "Niepokojący",
                MinRequired = 100,
                ThresholdLabel = "100",
                BaronyBonus = Vec(
                    (Ppb.Stability, 3), (Ppb.Law, 3),
                    (Ppb.Production, 3), (Ppb.Food, 3), (Ppb.Defense, 3)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, 1),
                BonusSummary =
                    "+3 Stabilność i Prawo; +3 Produkcja, Wyżywienie i Obrona; +1 Zastraszanie i Inspiracja (Dowodzenie).",
            },
            new()
            {
                Name = "Niebezpieczny",
                MinRequired = 300,
                ThresholdLabel = "300",
                BaronyBonus = Vec(
                    (Ppb.Stability, 5), (Ppb.Law, 5),
                    (Ppb.Production, 8), (Ppb.Food, 8), (Ppb.Defense, 8)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, 2),
                BonusSummary =
                    "+5 Stabilność i Prawo; +8 Produkcja, Wyżywienie i Obrona; +2 Zastraszanie i Inspiracja (Dowodzenie).",
            },
            new()
            {
                Name = "Postrach",
                MinRequired = 600,
                ThresholdLabel = "600",
                BaronyBonus = Vec(
                    (Ppb.Stability, 8), (Ppb.Law, 8),
                    (Ppb.Production, 15), (Ppb.Food, 15), (Ppb.Defense, 15)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, 3),
                BonusSummary =
                    "+8 Stabilność i Prawo; +15 Produkcja, Wyżywienie i Obrona; +3 Zastraszanie i Inspiracja (Dowodzenie).",
            },
            new()
            {
                Name = "Chodzący postrach",
                MinRequired = 1000,
                ThresholdLabel = "1000",
                BaronyBonus = Vec(
                    (Ppb.Stability, 15), (Ppb.Law, 15),
                    (Ppb.Production, 30), (Ppb.Food, 30), (Ppb.Defense, 30)),
                SkillBonuses = SkillsPair(Skills.Intimidate, CommandSkill, 5),
                BonusSummary =
                    "+15 Stabilność i Prawo; +30 Produkcja, Wyżywienie i Obrona; +5 Zastraszanie i Inspiracja (Dowodzenie).",
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
                parts.Add($"{sign}{v:0.##} {info.NameEn}");
            }

            return parts.Count == 0 ? "No barony PPB bonuses." : string.Join(", ", parts);
        }

        public static string DescribeActiveTiers(int prestige, int honor, int fear)
        {
            var p = ResolvePrestige(prestige);
            var h = ResolveHonor(honor);
            var f = ResolveFear(fear);
            var sb = new StringBuilder();
            sb.AppendLine($"Prestige: {p.Name} (score {prestige}) — {p.BonusSummary}");
            sb.AppendLine($"Honor: {h.Name} (score {honor}) — {h.BonusSummary}");
            sb.Append($"Fear: {f.Name} (score {fear}) — {f.BonusSummary}");
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
