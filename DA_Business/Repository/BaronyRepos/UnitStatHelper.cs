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

            // Excel base skills (Melee, Ranged, …): Razem = Bazowo + Inne — no attribute.
            foreach (var def in UnitSkillTree.All.Where(d => d.IsBase && d.Key != UnitSkillKey.Riding))
            {
                dto.SkillBase.TryGetValue(def.Key, out var bas);
                dto.SkillOther.TryGetValue(def.Key, out var oth);
                totals[def.Key] = bas + oth;
            }

            // Specializations: parent total + linked attr + base + other.
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

            // Riding sits alone in Excel but totals like a Melee specialization (parent = Melee Razem).
            var riding = UnitSkillTree.Find(UnitSkillKey.Riding);
            if (riding is not null)
            {
                dto.SkillBase.TryGetValue(riding.Key, out var bas);
                dto.SkillOther.TryGetValue(riding.Key, out var oth);
                var attr = UnitCombatFormulas.AttrValue(
                    dto.EffectiveBuild, dto.EffectiveAgility, dto.EffectiveWill, dto.EffectivePerception,
                    riding.LinkedAttr);
                var parentTotal = totals.GetValueOrDefault(UnitSkillKey.Melee);
                totals[riding.Key] = parentTotal + attr + bas + oth;
            }

            return totals;
        }

        /// <summary>Linked-attribute contribution shown in the Attr column (0 for pure base skills).</summary>
        public static int SkillAttrContribution(BaronyUnitDTO dto, UnitSkillDef def)
        {
            if (def.IsBase && def.Key != UnitSkillKey.Riding)
                return 0;
            return UnitCombatFormulas.AttrValue(
                dto.EffectiveBuild, dto.EffectiveAgility, dto.EffectiveWill, dto.EffectivePerception,
                def.LinkedAttr);
        }

        public static UnitCombatTotals Compute(BaronyUnitDTO dto)
        {
            var skillTotals = BuildSkillTotals(dto);
            var combat = UnitCombatFormulas.Compute(
                dto.EffectiveBuild, dto.EffectiveAgility, dto.EffectiveWill, dto.EffectivePerception,
                dto.Discipline,
                skillTotals,
                UnitWeaponCatalog.Find(dto.Weapon1Key),
                UnitArmorCatalog.Find(dto.ArmorKey),
                UnitArmorCatalog.Find(dto.ShieldKey),
                dto.Weapon1Quality,
                dto.CommanderAttack, dto.CommanderDefense,
                dto.OtherAttack, dto.OtherDefense, dto.OtherDamage, dto.OtherMove, dto.OtherArmor, dto.OtherHp,
                UnitRaceCatalog.Find(dto.RaceKey).MoveBonus,
                dto.TroopCount);
            if (!string.IsNullOrWhiteSpace(combat.DefenseSkillKeyUsed))
                dto.DefenseSkillKey = combat.DefenseSkillKeyUsed;
            return combat;
        }
    }
}
