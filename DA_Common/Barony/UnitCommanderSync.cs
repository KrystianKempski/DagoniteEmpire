namespace DA_Common.Barony;

/// <summary>Applies captain commander-tree passives onto unit combat Other / Cmd fields.</summary>
public static class UnitCommanderSync
{
    public const string CombatOtherLabel = "Commander";

    public static void ApplyCaptainBonuses(
        ref int commanderAttack,
        ref int commanderDefense,
        ref int otherAttack,
        ref int otherDefense,
        ref int otherDamage,
        ref int otherMove,
        ref int otherArmor,
        ref int otherHp,
        Dictionary<string, List<UnitCombatModifierEntry>> combatOther,
        CourtCharacterSheet? captainSheet,
        bool hasMount,
        bool hasShield)
    {
        combatOther ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
        ClearCommanderCombatOther(combatOther);

        if (captainSheet is null)
        {
            commanderAttack = 0;
            commanderDefense = 0;
            Resum(combatOther, out otherAttack, out otherDefense, out otherDamage, out otherMove, out otherArmor, out otherHp);
            return;
        }

        var bonuses = CourtCommanderFormulas.ComputeBonuses(captainSheet, hasMount, hasShield);
        commanderAttack = bonuses.CommanderAttack;
        commanderDefense = bonuses.CommanderDefense;

        void Put(string key, int value)
        {
            if (value == 0)
                return;
            if (!combatOther.TryGetValue(key, out var list) || list is null)
            {
                list = new List<UnitCombatModifierEntry>();
                combatOther[key] = list;
            }

            list.RemoveAll(e => string.Equals(e.Label, CombatOtherLabel, StringComparison.OrdinalIgnoreCase));
            list.Add(new UnitCombatModifierEntry { Label = CombatOtherLabel, Value = value });
        }

        Put(UnitCombatStatKey.Move, bonuses.OtherMove);
        Put(UnitCombatStatKey.Armor, bonuses.OtherArmor);
        Put(UnitCombatStatKey.Hp, bonuses.OtherHp);
        Put(UnitCombatStatKey.Damage, bonuses.OtherDamageMelee + bonuses.OtherDamageShooting);
        Resum(combatOther, out otherAttack, out otherDefense, out otherDamage, out otherMove, out otherArmor, out otherHp);
    }

    public static void ClearCaptainBonuses(
        ref int commanderAttack,
        ref int commanderDefense,
        ref int otherAttack,
        ref int otherDefense,
        ref int otherDamage,
        ref int otherMove,
        ref int otherArmor,
        ref int otherHp,
        Dictionary<string, List<UnitCombatModifierEntry>> combatOther)
        => ApplyCaptainBonuses(
            ref commanderAttack, ref commanderDefense,
            ref otherAttack, ref otherDefense, ref otherDamage, ref otherMove, ref otherArmor, ref otherHp,
            combatOther, null, false, false);

    private static void ClearCommanderCombatOther(Dictionary<string, List<UnitCombatModifierEntry>> combatOther)
    {
        foreach (var key in UnitCombatStatKey.All)
        {
            if (!combatOther.TryGetValue(key, out var list) || list is null)
                continue;
            list.RemoveAll(e => string.Equals(e.Label, CombatOtherLabel, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static void Resum(
        Dictionary<string, List<UnitCombatModifierEntry>> combatOther,
        out int otherAttack,
        out int otherDefense,
        out int otherDamage,
        out int otherMove,
        out int otherArmor,
        out int otherHp)
    {
        (otherAttack, otherDefense, otherDamage, otherMove, otherArmor, otherHp) =
            UnitCombatOtherFormulas.SumAll(combatOther);
    }
}
