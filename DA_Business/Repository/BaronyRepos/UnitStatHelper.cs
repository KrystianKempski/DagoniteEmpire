using DA_Common.Barony;
using DA_Common.Barony.Battle;
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

            // Riding sits alone in Excel and totals as: attr + base + other (no hidden parent).
            var riding = UnitSkillTree.Find(UnitSkillKey.Riding);
            if (riding is not null)
            {
                dto.SkillBase.TryGetValue(riding.Key, out var bas);
                dto.SkillOther.TryGetValue(riding.Key, out var oth);
                var attr = UnitCombatFormulas.AttrValue(
                    dto.EffectiveBuild, dto.EffectiveAgility, dto.EffectiveWill, dto.EffectivePerception,
                    riding.LinkedAttr);
                totals[riding.Key] = attr + bas + oth;
            }

            return totals;
        }

        /// <summary>Base skill total (Base + Other). Caps specialization base rank when spending XP.</summary>
        public static int BaseSkillTotal(BaronyUnitDTO dto, string baseSkillKey)
        {
            dto.SkillBase.TryGetValue(baseSkillKey, out var bas);
            dto.SkillOther.TryGetValue(baseSkillKey, out var oth);
            return bas + oth;
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

        public static UnitCombatTotals Compute(BaronyUnitDTO dto, int activeWeaponSlot = BattleWeaponSwap.MinSlot)
        {
            var skillTotals = BuildSkillTotals(dto);
            var primaryKey = BattleWeaponSwap.ActiveWeaponKey(dto.Weapon1Key, dto.Weapon2Key, activeWeaponSlot);
            var quality = BattleWeaponSwap.ActiveWeaponQuality(dto.Weapon1Quality, dto.Weapon2Quality, activeWeaponSlot);
            var shieldKey = BattleWeaponSwap.ActiveShieldKey(dto.Weapon1Key, dto.Weapon2Key, dto.ShieldKey, activeWeaponSlot);
            var combat = UnitCombatFormulas.Compute(
                dto.EffectiveBuild, dto.EffectiveAgility, dto.EffectiveWill, dto.EffectivePerception,
                dto.Discipline,
                skillTotals,
                UnitWeaponCatalog.Find(primaryKey),
                UnitArmorCatalog.Find(dto.ArmorKey),
                UnitArmorCatalog.Find(shieldKey),
                quality,
                dto.CommanderAttack, dto.CommanderDefense,
                dto.OtherAttack, dto.OtherDefense, dto.OtherDamage, dto.OtherMove, dto.OtherArmor, dto.OtherHp,
                UnitRaceCatalog.Find(dto.RaceKey).MoveBonus,
                dto.TroopCount,
                dto.FullTroopCount,
                UnitMountCatalog.Find(dto.MountKey));
            if (!string.IsNullOrWhiteSpace(combat.DefenseSkillKeyUsed))
                dto.DefenseSkillKey = combat.DefenseSkillKeyUsed;
            return combat;
        }

        public static UnitUpkeepTotals ComputeUpkeep(BaronyUnitDTO dto, bool battleSuppressesUnitActions = false) =>
            UnitActionFormulas.ApplyUpkeepModifier(
                UnitUpkeepFormulas.Compute(
                    dto.Wage, dto.UpkeepFood, dto.UpkeepDefense,
                    dto.Weapon1Key, dto.Weapon2Key, dto.ArmorKey, dto.ShieldKey, dto.MountKey),
                dto.CurrentAction,
                battleSuppressesUnitActions);
    }
}
