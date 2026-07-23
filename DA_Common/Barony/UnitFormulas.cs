namespace DA_Common.Barony
{
    public sealed record UnitTrainingCostSummary(
        int Production,
        int GoldEquipment,
        int GoldTraining,
        int GoldTotal,
        int DefenseEquipment,
        int DefenseRecruit,
        int DefenseAccelerate,
        int DefenseTotal,
        int Turns,
        int Wage,
        int Pd,
        int StartingDiscipline,
        int MaxBaseSkill,
        int AttributeScore,
        int FreeAttributePoints);

    public static class UnitTrainingCostFormulas
    {
        public static UnitTrainingCostSummary Compute(
            UnitRecruitSelection recruit,
            UnitTrainingType training,
            UnitWeaponDef? weapon1,
            UnitWeaponDef? weapon2,
            UnitArmorDef? armor,
            UnitArmorDef? shield,
            bool payEquipmentAsDefense,
            int accelerateTurns)
        {
            var prod = (weapon1?.ProductionCost ?? 0)
                + (weapon2?.ProductionCost ?? 0)
                + (armor?.ProductionCost ?? 0)
                + (shield?.ProductionCost ?? 0);
            var goldEq = (weapon1?.GoldCost ?? 0)
                + (weapon2?.GoldCost ?? 0)
                + (armor?.GoldCost ?? 0)
                + (shield?.GoldCost ?? 0);
            var market = (weapon1?.MarketGold ?? 0)
                + (weapon2?.MarketGold ?? 0)
                + (armor?.MarketGold ?? 0)
                + (shield?.MarketGold ?? 0);

            var accel = Math.Clamp(accelerateTurns, 0, training.Turns);
            var turns = Math.Max(0, training.Turns - accel);
            var goldTrain = training.GoldPerTurn * turns;
            var defEq = payEquipmentAsDefense ? market * 2 : 0;
            var defRecruit = recruit.DefenseCost;
            var defAccel = accel * UnitRules.AccelerateDefensePerTurn;

            return new UnitTrainingCostSummary(
                Production: payEquipmentAsDefense ? 0 : prod,
                GoldEquipment: payEquipmentAsDefense ? 0 : goldEq,
                GoldTraining: goldTrain,
                GoldTotal: (payEquipmentAsDefense ? 0 : goldEq) + goldTrain,
                DefenseEquipment: defEq,
                DefenseRecruit: defRecruit,
                DefenseAccelerate: defAccel,
                DefenseTotal: defEq + defRecruit + defAccel,
                Turns: turns,
                Wage: recruit.Wage + training.Wage,
                Pd: training.Pd,
                StartingDiscipline: training.StartingDiscipline,
                MaxBaseSkill: Math.Min(recruit.MaxBaseSkill, training.MaxBaseSkill),
                AttributeScore: recruit.AttributeScore,
                FreeAttributePoints: training.FreeAttributePoints);
        }
    }

    public sealed record UnitCombatTotals(
        int Attack,
        int Defense,
        int Damage,
        int Move,
        int Armor,
        int MaxHp,
        int AttackSkill,
        int DefenseSkill,
        int WeaponAttackBonus,
        int WeaponDefenseBonus,
        int WeaponDamageBonus,
        int EquipmentMovePenalty);

    public static class UnitCombatFormulas
    {
        public static int SkillTotal(int attributeValue, int baseLevel, int other = 0) =>
            attributeValue + baseLevel + other;

        public static int AttrValue(
            int build, int agility, int will, int perception,
            string linkedAttr) => linkedAttr switch
        {
            UnitAttr.Build => build,
            UnitAttr.Agility => agility,
            UnitAttr.Will => will,
            UnitAttr.Perception => perception,
            _ => 0,
        };

        public static UnitCombatTotals Compute(
            int build,
            int agility,
            int will,
            int perception,
            int discipline,
            IReadOnlyDictionary<string, int> skillTotals,
            UnitWeaponDef? primaryWeapon,
            UnitArmorDef? armor,
            UnitArmorDef? shield,
            string? weaponQuality,
            string? defenseSkillKey,
            int commanderAttack = 0,
            int commanderDefense = 0,
            int otherAttack = 0,
            int otherDefense = 0,
            int otherDamage = 0,
            int otherMove = 0,
            int otherArmor = 0,
            int otherHp = 0)
        {
            var quality = UnitWeaponQuality.AttackDamageBonus(weaponQuality);
            var skillKey = UnitSkillTree.SkillKeyForWeaponType(primaryWeapon?.WeaponType);
            var attackSkill = skillKey is not null && skillTotals.TryGetValue(skillKey, out var asv) ? asv : 0;
            var weaponAt = (primaryWeapon?.Attack ?? 0) + quality;
            var attack = attackSkill + weaponAt + commanderAttack + otherAttack;

            var defKey = string.IsNullOrWhiteSpace(defenseSkillKey) ? UnitSkillKey.Shields : defenseSkillKey!;
            var defenseSkill = skillTotals.TryGetValue(defKey, out var dsv) ? dsv : 0;
            var gearDef = (primaryWeapon?.Defense ?? 0) + (armor?.Defense ?? 0) + (shield?.Defense ?? 0);
            var defense = defenseSkill + gearDef + commanderDefense + otherDefense;

            var damage = (primaryWeapon?.Damage ?? 0) + quality + otherDamage;

            var run = skillTotals.TryGetValue(UnitSkillKey.Run, out var runVal) ? runVal : 0;
            var baseMove = UnitRules.RaceMoveBonus + (int)Math.Floor((agility + run) / 2.0);
            var movePenalty = (primaryWeapon?.MovePenalty ?? 0)
                + (armor?.MovePenalty ?? 0)
                + (shield?.MovePenalty ?? 0)
                + (armor?.AgilityPenalty ?? 0); // Excel folds Ks into movement bar via T
            // Excel: T13 = weapon Kr + armor/shield Kr + armor Ks (agility penalty on move track)
            // Keep agility penalty separate from move: only Kr on move.
            movePenalty = (primaryWeapon?.MovePenalty ?? 0)
                + (armor?.MovePenalty ?? 0)
                + (shield?.MovePenalty ?? 0);
            var move = baseMove + movePenalty + otherMove;

            var armorRating = (armor?.ArmorValue ?? 0) + (shield?.ArmorValue ?? 0) + otherArmor;

            var endurance = skillTotals.TryGetValue(UnitSkillKey.Endurance, out var endVal) ? endVal : 0;
            var maxHp = build * 2 + will * 2 + endurance + discipline * 3 + otherHp;

            return new UnitCombatTotals(
                Attack: attack,
                Defense: defense,
                Damage: damage,
                Move: move,
                Armor: armorRating,
                MaxHp: maxHp,
                AttackSkill: attackSkill,
                DefenseSkill: defenseSkill,
                WeaponAttackBonus: weaponAt,
                WeaponDefenseBonus: gearDef,
                WeaponDamageBonus: (primaryWeapon?.Damage ?? 0) + quality,
                EquipmentMovePenalty: movePenalty);
        }
    }

    public static class UnitPdFormulas
    {
        public static bool CanRaiseAttribute(int current, int remainingPd, out int cost)
        {
            cost = UnitRules.AttributeRaiseCost(current + 1);
            return remainingPd >= cost;
        }

        public static bool CanRaiseBaseSkill(int current, int maxAtGraduation, int remainingPd, out int cost)
        {
            cost = UnitRules.BaseSkillRaiseCost(current + 1);
            if (current + 1 > maxAtGraduation && maxAtGraduation > 0)
                return false; // soft: after graduation Excel lifts the training cap — we allow after Active
            return remainingPd >= cost;
        }

        public static bool CanRaiseSpecialSkill(int current, int parentBase, int remainingPd, out int cost)
        {
            cost = UnitRules.SpecialSkillRaiseCost(current + 1);
            if (current + 1 > parentBase)
                return false;
            return remainingPd >= cost;
        }

        public static bool CanRaiseDiscipline(int current, int remainingPd, out int cost)
        {
            cost = UnitRules.DisciplineRaiseCost(current);
            if (current >= UnitRules.DisciplineMax)
                return false;
            return remainingPd >= cost;
        }
    }
}
