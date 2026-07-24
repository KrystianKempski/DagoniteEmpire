namespace DA_Common.Barony
{
    public sealed record UnitTrainingCostSummary(
        int Production,
        int GoldEquipment,
        int GoldTraining,
        int GoldRecruit,
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

    /// <summary>How a gear item is paid for when forming a unit.</summary>
    public static class UnitEquipmentAcquireMode
    {
        /// <summary>Workshop: Production + catalog GoldCost.</summary>
        public const string Craft = "craft";
        /// <summary>Buy on the market: MarketGold only (no Production).</summary>
        public const string Buy = "buy";
        /// <summary>Requisition: 2× MarketGold as Defense (no Production/Gold).</summary>
        public const string Defense = "defense";

        public static readonly string[] All = { Craft, Buy, Defense };

        public static string Normalize(string? mode) =>
            string.Equals(mode, Buy, StringComparison.OrdinalIgnoreCase) ? Buy
            : string.Equals(mode, Defense, StringComparison.OrdinalIgnoreCase) ? Defense
            : Craft;

        public static string Label(string? mode) => Normalize(mode) switch
        {
            Buy => "Buy",
            Defense => "Defense",
            _ => "Craft",
        };
    }

    public sealed record UnitEquipmentPayModes(
        string Weapon1 = UnitEquipmentAcquireMode.Craft,
        string Weapon2 = UnitEquipmentAcquireMode.Craft,
        string Armor = UnitEquipmentAcquireMode.Craft,
        string Shield = UnitEquipmentAcquireMode.Craft);

    public static class UnitTrainingCostFormulas
    {
        public static UnitTrainingCostSummary Compute(
            UnitRecruitSelection recruit,
            UnitTrainingType training,
            UnitWeaponDef? weapon1,
            UnitWeaponDef? weapon2,
            UnitArmorDef? armor,
            UnitArmorDef? shield,
            UnitEquipmentPayModes payModes,
            int accelerateTurns)
        {
            static (int Prod, int Gold, int Def) SliceWeapon(UnitWeaponDef? w, string mode)
            {
                if (w is null) return (0, 0, 0);
                return UnitEquipmentAcquireMode.Normalize(mode) switch
                {
                    UnitEquipmentAcquireMode.Buy => (0, w.MarketGold, 0),
                    UnitEquipmentAcquireMode.Defense => (0, 0, w.MarketGold * 2),
                    _ => (w.ProductionCost, w.GoldCost, 0),
                };
            }

            static (int Prod, int Gold, int Def) SliceArmor(UnitArmorDef? a, string mode)
            {
                if (a is null) return (0, 0, 0);
                return UnitEquipmentAcquireMode.Normalize(mode) switch
                {
                    UnitEquipmentAcquireMode.Buy => (0, a.MarketGold, 0),
                    UnitEquipmentAcquireMode.Defense => (0, 0, a.MarketGold * 2),
                    _ => (a.ProductionCost, a.GoldCost, 0),
                };
            }

            var w1 = SliceWeapon(weapon1, payModes.Weapon1);
            var w2 = SliceWeapon(weapon2, payModes.Weapon2);
            var ar = SliceArmor(armor, payModes.Armor);
            var sh = SliceArmor(shield, payModes.Shield);

            var prod = w1.Prod + w2.Prod + ar.Prod + sh.Prod;
            var goldEq = w1.Gold + w2.Gold + ar.Gold + sh.Gold;
            var defEq = w1.Def + w2.Def + ar.Def + sh.Def;

            var accel = Math.Clamp(accelerateTurns, 0, training.Turns);
            var turns = Math.Max(0, training.Turns - accel);
            var goldTrain = training.GoldCost;
            var goldRecruit = Math.Max(0, recruit.GoldCost);
            var defRecruit = recruit.DefenseCost;
            var defAccel = accel * UnitRules.AccelerateDefensePerTurn;

            return new UnitTrainingCostSummary(
                Production: prod,
                GoldEquipment: goldEq,
                GoldTraining: goldTrain,
                GoldRecruit: goldRecruit,
                GoldTotal: goldEq + goldTrain + goldRecruit,
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
        int EquipmentMovePenalty,
        int CommanderAttack = 0,
        int CommanderDefense = 0,
        int OtherAttack = 0,
        int OtherDefense = 0,
        int OtherDamage = 0,
        int OtherMove = 0,
        int OtherArmor = 0,
        int OtherHp = 0,
        int BaseMove = 0,
        int RaceMove = 0,
        int AgilityRunMove = 0,
        int ArmorFromGear = 0,
        string? AttackSkillKey = null,
        string? DefenseSkillKeyUsed = null);

    public static class UnitCombatFormulas
    {
        public static int SkillTotal(int attributeValue, int baseLevel, int other = 0) =>
            attributeValue + baseLevel + other;

        /// <summary>
        /// Eligible defense skills: Dodges always; Shields only with a shield; Armor only with armor.
        /// Uses the highest skill total among eligible options (stable tie-break: Shields, Armor, Dodges).
        /// </summary>
        public static string ResolveDefenseSkillKey(
            IReadOnlyDictionary<string, int> skillTotals,
            bool hasShield,
            bool hasArmor)
        {
            static int Total(IReadOnlyDictionary<string, int> totals, string key) =>
                totals.TryGetValue(key, out var v) ? v : 0;

            string? bestKey = null;
            var bestValue = int.MinValue;
            // Tie-break order matches DefenseChoices.
            foreach (var key in UnitSkillKey.DefenseChoices)
            {
                if (key == UnitSkillKey.Shields && !hasShield) continue;
                if (key == UnitSkillKey.ArmorSkill && !hasArmor) continue;
                var value = Total(skillTotals, key);
                if (bestKey is null || value > bestValue)
                {
                    bestKey = key;
                    bestValue = value;
                }
            }

            return bestKey ?? UnitSkillKey.Dodges;
        }

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
            int commanderAttack = 0,
            int commanderDefense = 0,
            int otherAttack = 0,
            int otherDefense = 0,
            int otherDamage = 0,
            int otherMove = 0,
            int otherArmor = 0,
            int otherHp = 0,
            int raceMoveBonus = UnitRules.RaceMoveBonus)
        {
            var quality = UnitWeaponQuality.AttackDamageBonus(weaponQuality);
            var skillKey = UnitSkillTree.SkillKeyForWeaponType(primaryWeapon?.WeaponType);
            var attackSkill = skillKey is not null && skillTotals.TryGetValue(skillKey, out var asv) ? asv : 0;
            var weaponAt = (primaryWeapon?.Attack ?? 0) + quality;
            var attack = attackSkill + weaponAt + commanderAttack + otherAttack;

            var defKey = ResolveDefenseSkillKey(skillTotals, hasShield: shield is not null, hasArmor: armor is not null);
            var defenseSkill = skillTotals.TryGetValue(defKey, out var dsv) ? dsv : 0;
            var gearDef = (primaryWeapon?.Defense ?? 0) + (armor?.Defense ?? 0) + (shield?.Defense ?? 0);
            var defense = defenseSkill + gearDef + commanderDefense + otherDefense;

            var weaponDmg = (primaryWeapon?.Damage ?? 0) + quality;
            var damage = weaponDmg + otherDamage;

            var run = skillTotals.TryGetValue(UnitSkillKey.Run, out var runVal) ? runVal : 0;
            var raceMove = raceMoveBonus;
            var agilityRunMove = (int)Math.Floor((agility + run) / 2.0);
            var baseMove = raceMove + agilityRunMove;
            var movePenalty = (primaryWeapon?.MovePenalty ?? 0)
                + (armor?.MovePenalty ?? 0)
                + (shield?.MovePenalty ?? 0);
            var move = baseMove + movePenalty + otherMove;

            var armorFromGear = (armor?.ArmorValue ?? 0) + (shield?.ArmorValue ?? 0);
            var armorRating = armorFromGear + otherArmor;

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
                WeaponDamageBonus: weaponDmg,
                EquipmentMovePenalty: movePenalty,
                CommanderAttack: commanderAttack,
                CommanderDefense: commanderDefense,
                OtherAttack: otherAttack,
                OtherDefense: otherDefense,
                OtherDamage: otherDamage,
                OtherMove: otherMove,
                OtherArmor: otherArmor,
                OtherHp: otherHp,
                BaseMove: baseMove,
                RaceMove: raceMove,
                AgilityRunMove: agilityRunMove,
                ArmorFromGear: armorFromGear,
                AttackSkillKey: skillKey,
                DefenseSkillKeyUsed: defKey);
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
            // Cap applies until graduation (Training / draft). Pass int.MaxValue after Active.
            if (maxAtGraduation > 0 && current + 1 > maxAtGraduation)
                return false;
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
