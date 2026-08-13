using DA_Common.Barony;
using DA_Models.BaronyModels;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Default Active army units seeded once when a barony is created (Bonefyre Oddziały card).
    /// Not re-applied via Ensure — existing baronies keep whatever units they already have.
    /// </summary>
    public static class StarterUnitsSeeder
    {
        public const string CityWatchName = "City Watch";
        public const string BaronsGuardName = "Baron's Guard";

        /// <summary>Builds the two starter Active units for a new barony.</summary>
        public static IReadOnlyList<BaronyUnitDTO> CreateDefaults(int baronyId)
            => new[]
            {
                BuildCityWatch(baronyId),
                BuildBaronsGuard(baronyId),
            };

        private static BaronyUnitDTO BuildCityWatch(int baronyId)
        {
            // Bonefyre Oddziały — STRAŻ MIEJSKA: short spears, light leather, wooden medium shield.
            return Finalize(new BaronyUnitDTO
            {
                BaronyId = baronyId,
                Name = CityWatchName,
                Status = UnitStatus.Active,
                TroopCount = UnitRules.DefaultTroopCount,
                RaceKey = UnitRaceKey.Human,
                Wage = 0,
                UpkeepFood = 0m,
                UpkeepDefense = 0,
                Build = 4,
                Agility = 3,
                Will = 3,
                Perception = 3,
                Discipline = 10,
                RemainingPd = 0,
                MaxBaseSkillAtGraduation = 0,
                Weapon1Key = "short-spears",
                ArmorKey = "light-leather",
                ShieldKey = "wooden-medium-shield",
                Weapon1Quality = UnitWeaponQuality.Normal,
                SkillBase = Skills(
                    (UnitSkillKey.Melee, 2),
                    (UnitSkillKey.Ranged, 1),
                    (UnitSkillKey.Swords, 1),
                    (UnitSkillKey.Bows, 1),
                    (UnitSkillKey.Spears, 2),
                    (UnitSkillKey.Shields, 2),
                    (UnitSkillKey.Athletics, 1),
                    (UnitSkillKey.AgilitySkill, 1),
                    (UnitSkillKey.Endurance, 1),
                    (UnitSkillKey.Dodges, 1),
                    (UnitSkillKey.ArmorSkill, 1),
                    (UnitSkillKey.Run, 1),
                    (UnitSkillKey.Urban, 1),
                    (UnitSkillKey.Scout, 1),
                    (UnitSkillKey.Vigilance, 1),
                    (UnitSkillKey.Wilderness, 1),
                    (UnitSkillKey.CityPatrol, 1)),
            });
        }

        private static BaronyUnitDTO BuildBaronsGuard(int baronyId)
        {
            // Bonefyre Oddziały — GWARDIA BARONA: 10/50 troops (casualty Loss), longswords + simple bows,
            // mail and gambeson, studded medium shield.
            return Finalize(new BaronyUnitDTO
            {
                BaronyId = baronyId,
                Name = BaronsGuardName,
                Status = UnitStatus.Active,
                TroopCount = 10,
                RaceKey = UnitRaceKey.Human,
                Wage = 0,
                UpkeepFood = 0m,
                UpkeepDefense = 0,
                Build = 4,
                Agility = 4,
                Will = 4,
                Perception = 4,
                Discipline = 10,
                RemainingPd = 0,
                MaxBaseSkillAtGraduation = 0,
                Weapon1Key = "longswords",
                Weapon2Key = "simple-bows",
                ArmorKey = "mail-and-gambeson",
                ShieldKey = "studded-medium-shield",
                Weapon1Quality = UnitWeaponQuality.Normal,
                SkillBase = Skills(
                    (UnitSkillKey.Melee, 3),
                    (UnitSkillKey.Ranged, 3),
                    (UnitSkillKey.Swords, 3),
                    (UnitSkillKey.Bows, 3),
                    (UnitSkillKey.Spears, 1),
                    (UnitSkillKey.Shields, 3),
                    (UnitSkillKey.Javelins, 1),
                    (UnitSkillKey.Athletics, 2),
                    (UnitSkillKey.AgilitySkill, 2),
                    (UnitSkillKey.Endurance, 1),
                    (UnitSkillKey.Climbing, 1),
                    (UnitSkillKey.Dodges, 2),
                    (UnitSkillKey.ArmorSkill, 2),
                    (UnitSkillKey.Run, 1),
                    (UnitSkillKey.Urban, 1),
                    (UnitSkillKey.Scout, 2),
                    (UnitSkillKey.CrowdFighting, 1),
                    (UnitSkillKey.Vigilance, 2),
                    (UnitSkillKey.CityOrientation, 1),
                    (UnitSkillKey.Wilderness, 1),
                    (UnitSkillKey.CityPatrol, 1)),
            });
        }

        private static Dictionary<string, int> Skills(params (string Key, int Value)[] levels)
        {
            var map = UnitSkillDefaults.CreateSkillBase();
            foreach (var (key, value) in levels)
                map[key] = value;
            return map;
        }

        private static BaronyUnitDTO Finalize(BaronyUnitDTO dto)
        {
            var armor = UnitArmorCatalog.Find(dto.ArmorKey);
            var shield = UnitArmorCatalog.Find(dto.ShieldKey);
            dto.AttrPenaltyAgility = (armor?.AgilityPenalty ?? 0) + (shield?.AgilityPenalty ?? 0);
            dto.SkillOther ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            dto.CombatOther ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            dto.SkillOtherSources ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            dto.AttrOtherSources ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);

            var combat = UnitStatHelper.Compute(dto);
            dto.CurrentHp = combat.MaxHp;
            return dto;
        }
    }
}
