using DA_Common.Barony;
using DA_Models.BaronyModels;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>Shared unit combat/skill totals for UI and repository.</summary>
    public static class UnitStatHelper
    {
        public static Dictionary<string, int> BuildSkillTotals(BaronyUnitDTO dto)
        {
            var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var def in UnitSkillTree.All.Where(d => d.IsBase))
            {
                dto.SkillBase.TryGetValue(def.Key, out var bas);
                dto.SkillOther.TryGetValue(def.Key, out var oth);
                var attr = UnitCombatFormulas.AttrValue(
                    dto.EffectiveBuild, dto.EffectiveAgility, dto.EffectiveWill, dto.EffectivePerception,
                    def.LinkedAttr);
                totals[def.Key] = attr + bas + oth;
            }

            foreach (var def in UnitSkillTree.All.Where(d => !d.IsBase && d.ParentKey is not null))
            {
                var parentTotal = totals.GetValueOrDefault(def.ParentKey!);
                dto.SkillBase.TryGetValue(def.Key, out var bas);
                dto.SkillOther.TryGetValue(def.Key, out var oth);
                var attr = UnitCombatFormulas.AttrValue(
                    dto.EffectiveBuild, dto.EffectiveAgility, dto.EffectiveWill, dto.EffectivePerception,
                    def.LinkedAttr);
                totals[def.Key] = parentTotal + attr + bas + oth;
            }

            return totals;
        }

        public static UnitCombatTotals Compute(BaronyUnitDTO dto)
        {
            var skillTotals = BuildSkillTotals(dto);
            return UnitCombatFormulas.Compute(
                dto.EffectiveBuild, dto.EffectiveAgility, dto.EffectiveWill, dto.EffectivePerception,
                dto.Discipline,
                skillTotals,
                UnitWeaponCatalog.Find(dto.Weapon1Key),
                UnitArmorCatalog.Find(dto.ArmorKey),
                UnitArmorCatalog.Find(dto.ShieldKey),
                dto.Weapon1Quality,
                dto.DefenseSkillKey,
                dto.CommanderAttack, dto.CommanderDefense,
                dto.OtherAttack, dto.OtherDefense, dto.OtherDamage, dto.OtherMove, dto.OtherArmor, dto.OtherHp);
        }
    }
}
