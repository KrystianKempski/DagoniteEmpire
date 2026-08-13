using DA_Common.Barony;
using DA_Models.BaronyModels;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>Builds a live draft unit card from generator selections (Excel right-hand panel).</summary>
    public static class UnitDraftFactory
    {
        public static BaronyUnitDTO FromGenerator(
            string? name,
            string recruitKey,
            string trainingKey,
            string? weapon1Key,
            string? weapon2Key,
            string? armorKey,
            string? shieldKey,
            string? mountKey,
            string weaponQuality,
            UnitTrainingCostSummary costs)
        {
            var recruit = UnitRecruitSelectionCatalog.Find(recruitKey) ?? UnitRecruitSelectionCatalog.Volunteers;
            var armor = UnitArmorCatalog.Find(armorKey);
            var shield = UnitArmorCatalog.Find(shieldKey);
            var attr = costs.AttributeScore;

            var dto = new BaronyUnitDTO
            {
                Name = string.IsNullOrWhiteSpace(name) ? "Forming unit" : name.Trim(),
                Status = UnitStatus.Training,
                TroopCount = UnitRules.DefaultTroopCount,
                RecruitSelectionKey = recruit.Key,
                TrainingTypeKey = trainingKey,
                RaceKey = UnitRaceKey.Human,
                Wage = costs.Wage,
                UpkeepFood = UnitRules.DefaultUpkeepFood,
                UpkeepDefense = 0, // Defense upkeep is derived from gear Mkt (UnitUpkeepFormulas).
                Build = attr,
                Agility = attr,
                Will = attr,
                Perception = attr,
                AttrPenaltyAgility = (armor?.AgilityPenalty ?? 0) + (shield?.AgilityPenalty ?? 0),
                Weapon1Key = weapon1Key,
                Weapon2Key = weapon2Key,
                ArmorKey = armorKey,
                ShieldKey = shieldKey,
                MountKey = mountKey,
                Weapon1Quality = string.IsNullOrWhiteSpace(weaponQuality)
                    ? UnitWeaponQuality.Normal
                    : weaponQuality,
                DefenseSkillKey = UnitSkillKey.Dodges,
                RemainingPd = costs.Pd,
                Discipline = costs.StartingDiscipline,
                MaxBaseSkillAtGraduation = costs.MaxBaseSkill,
                FreeAttributePoints = costs.FreeAttributePoints,
                SkillBase = UnitSkillDefaults.CreateSkillBase(),
                SkillOther = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            };

            var combat = UnitStatHelper.Compute(dto);
            dto.CurrentHp = combat.MaxHp;
            return dto;
        }
    }
}
