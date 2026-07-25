namespace DA_Common.Barony
{
    public readonly struct UnitStatus
    {
        public const string Training = "Training";
        public const string Active = "Active";
        public const string Disbanded = "Disbanded";

        public static readonly string[] All = { Training, Active, Disbanded };
    }

    public readonly struct UnitAttr
    {
        public const string Build = "build";
        public const string Agility = "agility";
        public const string Will = "will";
        public const string Perception = "perception";

        public static readonly string[] All = { Build, Agility, Will, Perception };

        public static string Label(string key) => key switch
        {
            Build => "Build",
            Agility => "Agility",
            Will => "Will",
            Perception => "Perception",
            _ => key,
        };
    }

    public readonly struct UnitWeaponQuality
    {
        public const string Normal = "Normal";
        public const string Good = "Good";
        public const string Poor = "Poor";

        public static readonly string[] All = { Normal, Good, Poor };

        public static int AttackDamageBonus(string? quality) => quality switch
        {
            Good => 1,
            Poor => -1,
            _ => 0,
        };
    }

    /// <summary>Skill keys used on unit cards (English keys, Excel trees).</summary>
    public static class UnitSkillKey
    {
        public const string Melee = "melee";
        public const string Swords = "swords";
        public const string HeavyWeapons = "heavy-weapons";
        public const string Spears = "spears";
        public const string Shields = "shields";
        public const string LightWeapons = "light-weapons";
        public const string Exotic = "exotic";

        public const string Ranged = "ranged";
        public const string Bows = "bows";
        public const string Crossbows = "crossbows";
        public const string Slings = "slings";
        public const string Javelins = "javelins";
        public const string Firearms = "firearms";
        public const string Grenades = "grenades";

        public const string Athletics = "athletics";
        public const string Endurance = "endurance";
        public const string Lifting = "lifting";
        public const string ArmorSkill = "armor";
        public const string Wrestling = "wrestling";

        public const string AgilitySkill = "agility-skill";
        public const string Climbing = "climbing";
        public const string Dodges = "dodges";
        public const string Run = "run";
        public const string Stealth = "stealth";

        public const string Urban = "urban";
        public const string CrowdFighting = "crowd-fighting";
        public const string CityOrientation = "city-orientation";
        public const string Fortification = "fortification";
        public const string CityPatrol = "city-patrol";
        public const string TreatWounded = "treat-wounded";

        public const string Scout = "scout";
        public const string Vigilance = "vigilance";
        public const string Tracking = "tracking";
        public const string Wilderness = "wilderness";
        public const string Traps = "traps";
        public const string Camouflage = "camouflage";

        public const string Riding = "riding";

        /// <summary>Defense skill candidates (auto-picked by highest total; Shields needs a shield, Armor needs armor).</summary>
        public static readonly string[] DefenseChoices = { Shields, Dodges, ArmorSkill };
    }

    public readonly struct UnitRaceKey
    {
        public const string Human = "human";
    }

    /// <summary>Army unit race (Excel race move / racial starting skills).</summary>
    public sealed class UnitRaceDef
    {
        public string Key { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        /// <summary>Added into Move base: race + floor((Agility + Run)/2).</summary>
        public int MoveBonus { get; init; }
        /// <summary>How many base skills get a racial Other bonus the player may assign.</summary>
        public int BonusBaseSkillPicks { get; init; }
        /// <summary>Value of each racial base-skill Other bonus.</summary>
        public int BonusBaseSkillAmount { get; init; }
        /// <summary>Tooltip / catalog blurb (move + base skill defaults).</summary>
        public string Description { get; init; } = string.Empty;
    }

    public static class UnitRaceCatalog
    {
        public static readonly UnitRaceDef Human = new()
        {
            Key = UnitRaceKey.Human,
            Name = "Human",
            MoveBonus = 3,
            BonusBaseSkillPicks = 2,
            BonusBaseSkillAmount = 1,
            Description =
                "Move +3. Base skills start at 0. "
                + "+1 Other to two base skills (player picks).",
        };

        public static readonly IReadOnlyList<UnitRaceDef> All = new[] { Human };

        public static UnitRaceDef Default => Human;

        public static UnitRaceDef Find(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Default;
            return All.FirstOrDefault(r =>
                       string.Equals(r.Key, key.Trim(), StringComparison.OrdinalIgnoreCase))
                   ?? Default;
        }
    }

    public static class UnitRules
    {
        public const int DefaultTroopCount = 50;
        public const decimal DefaultUpkeepFood = 0.5m;
        /// <summary>Legacy flat Defense upkeep — replaced by gear Mkt blocks (<see cref="UnitUpkeepFormulas"/>).</summary>
        public const int DefaultUpkeepDefense = 5;
        /// <summary>Each full 100 market-gold of equipped gear is one upkeep block.</summary>
        public const int GearUpkeepMarketGoldPerBlock = 100;
        /// <summary>Gold added to wage per gear Mkt block (floor).</summary>
        public const int GearUpkeepGoldPerBlock = 2;
        /// <summary>Defense upkeep per gear Mkt block (floor) — replaces flat DefaultUpkeepDefense.</summary>
        public const int GearUpkeepDefensePerBlock = 5;

        /// <summary>
        /// Reinforce people cost uses Selected volunteers + Standard training, scaled by troops/50.
        /// Gear costs use half catalog price (salvage) then the same troop scale → effective × troops/100.
        /// </summary>
        public const int ReinforceGearSalvagePercent = 50;
        /// <summary>Legacy alias — prefer <see cref="UnitRaceCatalog.Human"/>.MoveBonus.</summary>
        public const int RaceMoveBonus = 3; // Humans
        public const int DisciplineMin = 1;
        public const int DisciplineMax = 18;
        public const int AccelerateDefensePerTurn = 50;

        /// <summary>Per full 10% of nominal strength lost: −1 Attack, −1 Defense, −4 Max HP.</summary>
        public const int CasualtyAttackPerStep = 1;
        public const int CasualtyDefensePerStep = 1;
        public const int CasualtyHpPerStep = 4;
        /// <summary>Troops restored per turn while below full strength.</summary>
        public const int TroopRegenPerTurn = 5;
        /// <summary>Floors applied when the unit has casualty steps (loss &gt; 0).</summary>
        public const int CasualtyMinAttack = 1;
        public const int CasualtyMinDefense = 1;
        public const int CasualtyMinMaxHp = 10;

        public static int AttributeRaiseCost(int targetLevel) => Math.Max(1, targetLevel) * 10;
        public static int BaseSkillRaiseCost(int targetLevel) => Math.Max(1, targetLevel) * 3;
        public static int SpecialSkillRaiseCost(int targetLevel) => Math.Max(1, targetLevel);
        public static int DisciplineRaiseCost(int currentLevel) => Math.Max(1, currentLevel);
    }

    /// <summary>
    /// Starting <c>Bazowo</c> for base skills (Humans: all at 0) until XP / MG changes them.
    /// Racial bonus is only via <see cref="UnitRaceSkillBonus"/> (+1 Other × picks).
    /// </summary>
    public static class UnitSkillDefaults
    {
        public static readonly IReadOnlyDictionary<string, int> BaseSkillLevels =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                [UnitSkillKey.Melee] = 0,
                [UnitSkillKey.Ranged] = 0,
                [UnitSkillKey.Athletics] = 0,
                [UnitSkillKey.AgilitySkill] = 0,
                [UnitSkillKey.Urban] = 0,
                [UnitSkillKey.Scout] = 0,
            };

        public static Dictionary<string, int> CreateSkillBase(
            IReadOnlyDictionary<string, int>? overlay = null)
        {
            var map = new Dictionary<string, int>(BaseSkillLevels, StringComparer.OrdinalIgnoreCase);
            if (overlay is null) return map;
            foreach (var (key, value) in overlay)
                map[key] = value;
            return map;
        }
    }

    /// <summary>
    /// Racial +N Other on chosen base skills (Humans: +1 to two base skills).
    /// Stored as named SkillOtherSources entries with <see cref="OtherLabel"/>.
    /// </summary>
    public static class UnitRaceSkillBonus
    {
        public const string OtherLabel = "Race";

        public static IReadOnlyList<UnitSkillDef> EligibleBaseSkills { get; } =
            UnitSkillTree.All
                .Where(d => d.IsBase && d.Key != UnitSkillKey.Riding)
                .ToList();

        public static bool IsRaceEntry(UnitCombatModifierEntry e) =>
            string.Equals(e.Label?.Trim(), OtherLabel, StringComparison.OrdinalIgnoreCase);

        /// <summary>Remove all Race Other entries, then apply +amount to each distinct pick.</summary>
        public static void ApplyPicks(
            Dictionary<string, List<UnitCombatModifierEntry>> skillOtherSources,
            Dictionary<string, int> skillOther,
            UnitRaceDef race,
            params string?[] picks)
        {
            ClearRaceEntries(skillOtherSources, skillOther);

            if (race.BonusBaseSkillPicks <= 0 || race.BonusBaseSkillAmount == 0)
                return;

            var chosen = picks
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(k => EligibleBaseSkills.Any(d =>
                    string.Equals(d.Key, k, StringComparison.OrdinalIgnoreCase)))
                .Take(race.BonusBaseSkillPicks)
                .ToList();

            foreach (var key in chosen)
            {
                if (!skillOtherSources.TryGetValue(key, out var list) || list is null)
                {
                    list = new List<UnitCombatModifierEntry>();
                    skillOtherSources[key] = list;
                }

                list.Add(new UnitCombatModifierEntry
                {
                    Label = OtherLabel,
                    Value = race.BonusBaseSkillAmount,
                });
                skillOther[key] = UnitCombatOtherFormulas.Sum(list);
            }
        }

        public static void ClearRaceEntries(
            Dictionary<string, List<UnitCombatModifierEntry>> skillOtherSources,
            Dictionary<string, int> skillOther)
        {
            foreach (var key in skillOtherSources.Keys.ToList())
            {
                var list = skillOtherSources[key];
                if (list is null) continue;
                list.RemoveAll(IsRaceEntry);
                if (list.Count == 0)
                {
                    skillOtherSources.Remove(key);
                    skillOther.Remove(key);
                }
                else
                {
                    skillOther[key] = UnitCombatOtherFormulas.Sum(list);
                }
            }
        }
    }
}
