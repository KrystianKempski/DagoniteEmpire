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

        /// <summary>Which skills can be chosen as primary defense skill.</summary>
        public static readonly string[] DefenseChoices = { Shields, Dodges, ArmorSkill };
    }

    public static class UnitRules
    {
        public const int DefaultTroopCount = 50;
        public const decimal DefaultUpkeepFood = 0.5m;
        public const int DefaultUpkeepDefense = 5;
        public const int RaceMoveBonus = 3; // Humans
        public const int DisciplineMin = 1;
        public const int DisciplineMax = 18;
        public const int AccelerateDefensePerTurn = 50;

        public static int AttributeRaiseCost(int targetLevel) => Math.Max(1, targetLevel) * 10;
        public static int BaseSkillRaiseCost(int targetLevel) => Math.Max(1, targetLevel) * 3;
        public static int SpecialSkillRaiseCost(int targetLevel) => Math.Max(1, targetLevel);
        public static int DisciplineRaiseCost(int currentLevel) => Math.Max(1, currentLevel);
    }
}
