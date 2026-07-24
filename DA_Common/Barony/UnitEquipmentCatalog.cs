namespace DA_Common.Barony
{
    public sealed record UnitWeaponDef(
        string Key,
        string Name,
        string Category, // simple | military | powder
        string Kind, // melee | ranged
        string WeaponType,
        int Attack,
        int Damage,
        int Defense,
        int Pierce,
        int Range,
        int MovePenalty,
        bool OneHanded,
        int RequiredBuild,
        int RequiredAgility,
        int ProductionCost,
        int GoldCost,
        int MarketGold);

    public static class UnitWeaponCatalog
    {
        public static readonly IReadOnlyList<UnitWeaponDef> All = new[]
        {
            // Simple melee
            W("short-spears", "Short spears", "simple", "melee", "Spears & lances", 0, 0, 1, 1, 0, 0, true, 1, 1, 20, 5, 20),
            W("simple-axes", "Simple axes", "simple", "melee", "Heavy weapons", 0, 1, 0, 2, 0, 0, true, 2, 1, 30, 7, 30),
            W("studded-clubs", "Studded clubs", "simple", "melee", "Heavy weapons", 0, 2, 0, 0, 0, 0, true, 2, 1, 15, 3, 15),
            // Simple ranged
            W("simple-bows", "Simple bows", "simple", "ranged", "Bows", 0, 0, 0, 0, 3, 0, false, 1, 3, 40, 10, 50),
            W("slings", "Slings", "simple", "ranged", "Slings", 0, 2, 0, 0, 2, 0, false, 2, 3, 15, 5, 20),
            W("simple-javelins", "Simple javelins", "simple", "ranged", "Javelins", 0, 4, 0, 1, 1, 0, true, 3, 2, 30, 9, 40),

            // Military melee
            W("longswords", "Longswords", "military", "melee", "Swords", 2, 0, 2, 0, 0, 0, true, 2, 2, 50, 20, 80),
            W("battle-axes", "Battle axes", "military", "melee", "Heavy weapons", 0, 3, 0, 3, 0, 0, true, 3, 2, 40, 15, 60),
            W("one-handed-maces", "One-handed maces", "military", "melee", "Heavy weapons", 0, 5, 1, 0, 0, 0, true, 3, 2, 40, 12, 50),
            W("halberds", "Halberds", "military", "melee", "Heavy weapons", 1, 4, 3, 2, 0, 0, false, 4, 2, 60, 25, 90),
            W("zweihanders", "Zweihanders", "military", "melee", "Swords", 4, 2, 1, 1, 0, 0, false, 4, 2, 100, 50, 230),
            W("long-spears", "Long spears", "military", "melee", "Spears & lances", 1, 2, 5, 2, 0, -1, false, 4, 2, 35, 10, 50),
            W("war-hammers", "War hammers", "military", "melee", "Heavy weapons", 1, 8, 1, 2, 0, -1, false, 5, 2, 70, 35, 150),
            // Military ranged
            W("war-bows", "War bows", "military", "ranged", "Bows", 1, 3, 0, 1, 4, 0, false, 3, 3, 80, 20, 80),
            W("longbows", "Longbows", "military", "ranged", "Bows", 2, 5, 0, 2, 5, 0, false, 5, 3, 120, 30, 140),
            W("light-crossbow", "Light crossbow", "military", "ranged", "Crossbows", 2, 0, 0, 2, 2, 0, false, 2, 2, 90, 30, 150),
            W("medium-crossbow", "Medium crossbow", "military", "ranged", "Crossbows", 3, 1, 0, 3, 2, 0, false, 3, 2, 120, 30, 180),
            W("siege-crossbow", "Siege crossbow", "military", "ranged", "Crossbows", 4, 2, 0, 4, 3, -1, false, 4, 2, 160, 45, 200),
            W("pilum", "Pilum", "military", "ranged", "Javelins", 0, 6, 0, 2, 1, 0, true, 4, 2, 60, 25, 100),

            // Powder
            W("muskets", "Muskets", "powder", "ranged", "Firearms", 1, 3, 0, 1, 4, 0, false, 3, 3, 150, 50, 220),
            W("arquebuses", "Arquebuses", "powder", "ranged", "Firearms", 2, 5, 0, 2, 5, 0, false, 5, 3, 180, 70, 250),
            W("hand-bombs", "Hand bombs", "powder", "ranged", "Grenades", 2, 0, 0, 2, 2, -1, false, 2, 2, 160, 50, 240),
        };

        private static UnitWeaponDef W(
            string key, string name, string category, string kind, string type,
            int at, int dm, int ob, int prz, int za, int kr, bool oneHanded,
            int bw, int sw, int p, int z, int r) =>
            new(key, name, category, kind, type, at, dm, ob, prz, za, kr, oneHanded, bw, sw, p, z, r);

        public static UnitWeaponDef? Find(string? key) =>
            All.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<UnitWeaponDef> ByCategory(string category) =>
            All.Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));

        public static string CategoryLabel(string category) => category switch
        {
            "simple" => "Simple weapons",
            "military" => "Military weapons",
            "powder" => "Powder weapons",
            _ => category,
        };
    }

    public sealed record UnitArmorDef(
        string Key,
        string Name,
        string ArmorClass, // light | medium | heavy | shields
        bool IsShield,
        int Defense,
        int ArmorValue,
        int AgilityPenalty,
        int MovePenalty,
        int RequiredBuild,
        int RequiredArmorSkill,
        int ProductionCost,
        int GoldCost,
        int MarketGold);

    public static class UnitArmorCatalog
    {
        public static readonly IReadOnlyList<UnitArmorDef> All = new[]
        {
            // Simple shields
            A("wooden-buckler", "Wooden buckler", "shields", true, 2, 0, 0, 0, 1, 1, 15, 5, 15),
            A("wooden-medium-shield", "Wooden medium shield", "shields", true, 3, 1, 0, -1, 2, 3, 25, 9, 30),
            A("wooden-large-shield", "Wooden large shield", "shields", true, 4, 2, -1, -2, 3, 5, 40, 12, 50),
            // Simple armor
            A("light-leather", "Light leather armor", "light", false, 2, 2, 0, 0, 2, 3, 50, 20, 60),
            A("heavy-leather", "Heavy leather armor", "medium", false, 3, 3, -1, 0, 3, 5, 80, 40, 100),

            // Medium shields / armor
            A("studded-buckler", "Studded buckler", "shields", true, 2, 1, 0, 0, 2, 1, 25, 8, 30),
            A("studded-medium-shield", "Studded medium shield", "shields", true, 3, 2, 0, -1, 3, 3, 40, 15, 50),
            A("studded-large-shield", "Studded large shield", "shields", true, 5, 2, -1, -2, 3, 5, 60, 25, 80),
            A("mail-and-gambeson", "Mail and gambeson", "medium", false, 4, 4, 0, 0, 3, 5, 80, 35, 100),
            A("mail-and-cuirass", "Mail and cuirass", "medium", false, 6, 6, -1, -1, 3, 6, 120, 50, 150),

            // Heavy
            A("metal-buckler", "Metal buckler", "shields", true, 2, 1, 0, 0, 2, 1, 40, 20, 60),
            A("metal-medium-shield", "Metal medium shield", "shields", true, 3, 2, 0, -1, 4, 3, 65, 35, 90),
            A("metal-large-shield", "Metal large shield", "shields", true, 5, 2, -1, -2, 4, 5, 85, 40, 120),
            A("lamellar", "Lamellar armor", "medium", false, 7, 5, -1, -2, 3, 7, 130, 60, 160),
            A("half-plate", "Half plate", "heavy", false, 9, 8, -2, -3, 4, 9, 200, 80, 250),
            A("full-plate", "Full plate", "heavy", false, 11, 10, -3, -4, 5, 11, 400, 110, 500),
        };

        private static UnitArmorDef A(
            string key, string name, string cls, bool shield,
            int ob, int pc, int ks, int kr, int bw, int pw, int p, int z, int r) =>
            new(key, name, cls, shield, ob, pc, ks, kr, bw, pw, p, z, r);

        public static UnitArmorDef? Find(string? key) =>
            All.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<UnitArmorDef> Shields => All.Where(x => x.IsShield);
        public static IEnumerable<UnitArmorDef> BodyArmor => All.Where(x => !x.IsShield);

        /// <summary>Excel “PANCERZE PROSTE / ŚREDNIE / CIĘŻKIE” groupings (shields + body).</summary>
        public static readonly IReadOnlyList<(string Title, string[] Keys)> ExcelTiers = new[]
        {
            ("Simple armor", new[]
            {
                "wooden-buckler", "wooden-medium-shield", "wooden-large-shield",
                "light-leather", "heavy-leather",
            }),
            ("Medium armor", new[]
            {
                "studded-buckler", "studded-medium-shield", "studded-large-shield",
                "mail-and-gambeson", "mail-and-cuirass",
            }),
            ("Heavy armor", new[]
            {
                "metal-buckler", "metal-medium-shield", "metal-large-shield",
                "lamellar", "half-plate", "full-plate",
            }),
        };

        public static IEnumerable<UnitArmorDef> TierItems(string[] keys) =>
            keys.Select(Find).Where(x => x is not null).Cast<UnitArmorDef>();
    }

    /// <summary>Build / Agility / Armor-skill gates for unit gear (Excel Bld / Agi / Ask).</summary>
    public static class UnitEquipmentRequirements
    {
        public static bool MeetsWeapon(UnitWeaponDef w, int build, int agility, out string reason)
        {
            if (build < w.RequiredBuild)
            {
                reason = $"{w.Name}: need Build {w.RequiredBuild} (have {build}).";
                return false;
            }
            if (agility < w.RequiredAgility)
            {
                reason = $"{w.Name}: need Agility {w.RequiredAgility} (have {agility}).";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public static bool MeetsArmor(UnitArmorDef a, int build, int armorSkill, out string reason)
        {
            if (build < a.RequiredBuild)
            {
                reason = $"{a.Name}: need Build {a.RequiredBuild} (have {build}).";
                return false;
            }
            if (armorSkill < a.RequiredArmorSkill)
            {
                reason = $"{a.Name}: need Armor skill {a.RequiredArmorSkill} (have {armorSkill}).";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public static string? FirstEligibleWeaponKey(int build, int agility) =>
            UnitWeaponCatalog.All
                .FirstOrDefault(w => MeetsWeapon(w, build, agility, out _))
                ?.Key;
    }
}
