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
        public static (int Prod, int Gold, int Def) SliceWeapon(UnitWeaponDef? w, string mode)
        {
            if (w is null) return (0, 0, 0);
            return UnitEquipmentAcquireMode.Normalize(mode) switch
            {
                UnitEquipmentAcquireMode.Buy => (0, w.MarketGold, 0),
                UnitEquipmentAcquireMode.Defense => (0, 0, w.MarketGold * 2),
                _ => (w.ProductionCost, w.GoldCost, 0),
            };
        }

        public static (int Prod, int Gold, int Def) SliceArmor(UnitArmorDef? a, string mode)
        {
            if (a is null) return (0, 0, 0);
            return UnitEquipmentAcquireMode.Normalize(mode) switch
            {
                UnitEquipmentAcquireMode.Buy => (0, a.MarketGold, 0),
                UnitEquipmentAcquireMode.Defense => (0, 0, a.MarketGold * 2),
                _ => (a.ProductionCost, a.GoldCost, 0),
            };
        }

        public static (int Prod, int Gold, int Def) SumGear(
            UnitWeaponDef? weapon1,
            UnitWeaponDef? weapon2,
            UnitArmorDef? armor,
            UnitArmorDef? shield,
            UnitEquipmentPayModes payModes)
        {
            var w1 = SliceWeapon(weapon1, payModes.Weapon1);
            var w2 = SliceWeapon(weapon2, payModes.Weapon2);
            var ar = SliceArmor(armor, payModes.Armor);
            var sh = SliceArmor(shield, payModes.Shield);
            return (
                w1.Prod + w2.Prod + ar.Prod + sh.Prod,
                w1.Gold + w2.Gold + ar.Gold + sh.Gold,
                w1.Def + w2.Def + ar.Def + sh.Def);
        }

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
            var (prod, goldEq, defEq) = SumGear(weapon1, weapon2, armor, shield, payModes);

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

    public sealed record UnitReinforceCostSummary(
        int TroopCount,
        int MissingTroops,
        int Production,
        int GoldPeople,
        int GoldGear,
        int GoldTotal,
        int DefensePeople,
        int DefenseGear,
        int DefenseTotal,
        int Turns,
        int FullGearProduction,
        int FullGearGold,
        int FullGearDefense);

    /// <summary>
    /// Reinforce understrength units: Selected volunteers + Standard people cost scaled by N/50,
    /// plus current gear at 50% salvage then × N/50 (i.e. full gear × N/100). Floor rounding.
    /// </summary>
    public static class UnitReinforceCostFormulas
    {
        public static UnitReinforceCostSummary Compute(
            int currentTroops,
            UnitWeaponDef? weapon1,
            UnitWeaponDef? weapon2,
            UnitArmorDef? armor,
            UnitArmorDef? shield,
            UnitEquipmentPayModes payModes,
            int? reinforceTroops = null)
        {
            var full = UnitRules.DefaultTroopCount;
            var current = Math.Clamp(currentTroops, 0, full);
            var missing = full - current;
            var n = reinforceTroops is int asked
                ? Math.Clamp(asked, 1, Math.Max(1, missing))
                : Math.Max(1, missing);
            if (missing <= 0)
                n = 0;

            var recruit = UnitRecruitSelectionCatalog.SelectedVolunteers;
            var training = UnitTrainingTypeCatalog.Standard;
            var (fullProd, fullGold, fullDef) = UnitTrainingCostFormulas.SumGear(
                weapon1, weapon2, armor, shield, payModes);

            // People: Selected volunteers Defense + Standard gold, × N/50 (floor).
            var goldPeople = n <= 0 ? 0 : training.GoldCost * n / full;
            var defPeople = n <= 0 ? 0 : recruit.DefenseCost * n / full;

            // Gear: salvage% of full gear, then × N/50 → full × N × salvage / (full×100) (floor).
            var prod = n <= 0 ? 0 : fullProd * n * UnitRules.ReinforceGearSalvagePercent / (full * 100);
            var goldGear = n <= 0 ? 0 : fullGold * n * UnitRules.ReinforceGearSalvagePercent / (full * 100);
            var defGear = n <= 0 ? 0 : fullDef * n * UnitRules.ReinforceGearSalvagePercent / (full * 100);

            var turns = n <= 0 ? 0 : Math.Max(1, training.Turns * n / full);

            return new UnitReinforceCostSummary(
                TroopCount: n,
                MissingTroops: missing,
                Production: prod,
                GoldPeople: goldPeople,
                GoldGear: goldGear,
                GoldTotal: goldPeople + goldGear,
                DefensePeople: defPeople,
                DefenseGear: defGear,
                DefenseTotal: defPeople + defGear,
                Turns: turns,
                FullGearProduction: fullProd,
                FullGearGold: fullGold,
                FullGearDefense: fullDef);
        }
    }

    /// <summary>
    /// Per-turn unit maintenance (Active units / Domain Panel Army).
    /// Gold = base wage + floor(Σ equipment Mkt / 100) × 2.
    /// Defense = floor(Σ equipment Mkt / 100) × 5 (replaces the old flat 5).
    /// Food = stored UpkeepFood. Starter units with wage/food/defense all 0 are exempt.
    /// </summary>
    public sealed record UnitUpkeepTotals(
        int GearMarketGold,
        int GearBlocks,
        int BaseWage,
        int GearGold,
        int Gold,
        decimal Food,
        int Defense,
        bool MaintenanceExempt);

    public static class UnitUpkeepFormulas
    {
        public static int EquipmentMarketGold(
            string? weapon1Key,
            string? weapon2Key,
            string? armorKey,
            string? shieldKey)
        {
            var sum = 0;
            if (UnitWeaponCatalog.Find(weapon1Key) is { } w1) sum += w1.MarketGold;
            if (UnitWeaponCatalog.Find(weapon2Key) is { } w2) sum += w2.MarketGold;
            if (UnitArmorCatalog.Find(armorKey) is { } ar) sum += ar.MarketGold;
            if (UnitArmorCatalog.Find(shieldKey) is { } sh) sum += sh.MarketGold;
            return Math.Max(0, sum);
        }

        /// <summary>
        /// Seeded free companies (City Watch / Baron's Guard): wage, food, and stored defense all 0.
        /// </summary>
        public static bool IsMaintenanceExempt(int wage, decimal upkeepFood, int upkeepDefense) =>
            wage == 0 && upkeepFood == 0m && upkeepDefense == 0;

        public static UnitUpkeepTotals Compute(
            int baseWage,
            decimal upkeepFood,
            int storedUpkeepDefense,
            string? weapon1Key,
            string? weapon2Key,
            string? armorKey,
            string? shieldKey)
        {
            var mkt = EquipmentMarketGold(weapon1Key, weapon2Key, armorKey, shieldKey);
            var blocks = mkt / UnitRules.GearUpkeepMarketGoldPerBlock; // floor for non-negative
            var gearGold = blocks * UnitRules.GearUpkeepGoldPerBlock;
            var gearDef = blocks * UnitRules.GearUpkeepDefensePerBlock;

            if (IsMaintenanceExempt(baseWage, upkeepFood, storedUpkeepDefense))
            {
                return new UnitUpkeepTotals(
                    GearMarketGold: mkt,
                    GearBlocks: blocks,
                    BaseWage: 0,
                    GearGold: 0,
                    Gold: 0,
                    Food: 0m,
                    Defense: 0,
                    MaintenanceExempt: true);
            }

            return new UnitUpkeepTotals(
                GearMarketGold: mkt,
                GearBlocks: blocks,
                BaseWage: Math.Max(0, baseWage),
                GearGold: gearGold,
                Gold: Math.Max(0, baseWage) + gearGold,
                Food: upkeepFood,
                Defense: gearDef,
                MaintenanceExempt: false);
        }

        public static string Explain(UnitUpkeepTotals u)
        {
            if (u.MaintenanceExempt)
                return "Maintenance paid elsewhere (no gold, food, or Defense upkeep).";

            return $"Gold / turn = base wage {u.BaseWage} + gear ({u.GearBlocks} × {UnitRules.GearUpkeepGoldPerBlock} "
                + $"from {u.GearMarketGold} Mkt) = {u.Gold}. "
                + $"Defense / turn = {u.GearBlocks} × {UnitRules.GearUpkeepDefensePerBlock} = {u.Defense} "
                + $"(floor of equipment market gold / {UnitRules.GearUpkeepMarketGoldPerBlock}). "
                + $"Food / turn = {u.Food.ToString("0.#")}.";
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
        int LossAttack = 0,
        int LossDefense = 0,
        int LossHp = 0,
        int CasualtySteps = 0,
        int TroopCount = UnitRules.DefaultTroopCount,
        int FullTroopCount = UnitRules.DefaultTroopCount,
        int BaseMove = 0,
        int RaceMove = 0,
        int AgilityRunMove = 0,
        int ArmorFromGear = 0,
        string? AttackSkillKey = null,
        string? DefenseSkillKeyUsed = null);

    /// <summary>
    /// Combat penalties from missing troops vs nominal full strength (default 50).
    /// −1 Attack, −1 Defense, −4 Max HP per each full 10% of strength lost.
    /// Example: 10/50 → 80% lost → 8 steps → −8 Atk/Def, −32 HP.
    /// </summary>
    public static class UnitCasualtyFormulas
    {
        public sealed record Penalties(int Steps, int Attack, int Defense, int Hp);

        public static int Steps(int troopCount, int fullStrength = UnitRules.DefaultTroopCount)
        {
            var full = Math.Max(1, fullStrength);
            var current = Math.Max(0, troopCount);
            if (current >= full)
                return 0;
            var lost = full - current;
            // Each 10% of full strength = one step (50 → step size 5).
            return lost * 10 / full;
        }

        public static Penalties Compute(int troopCount, int fullStrength = UnitRules.DefaultTroopCount)
        {
            var steps = Steps(troopCount, fullStrength);
            return new Penalties(
                Steps: steps,
                Attack: -steps * UnitRules.CasualtyAttackPerStep,
                Defense: -steps * UnitRules.CasualtyDefensePerStep,
                Hp: -steps * UnitRules.CasualtyHpPerStep);
        }

        /// <summary>
        /// Restores up to <see cref="UnitRules.TroopRegenPerTurn"/> troops toward full strength.
        /// Returns the new troop count (unchanged when already full).
        /// </summary>
        public static int Regenerate(
            int troopCount,
            int fullStrength = UnitRules.DefaultTroopCount,
            int perTurn = UnitRules.TroopRegenPerTurn)
        {
            var full = Math.Max(1, fullStrength);
            var current = Math.Max(0, troopCount);
            if (current >= full)
                return full;
            return Math.Min(full, current + Math.Max(0, perTurn));
        }

        public static string Explain(int troopCount, int fullStrength = UnitRules.DefaultTroopCount)
        {
            var p = Compute(troopCount, fullStrength);
            var lostPct = Math.Max(0, fullStrength - Math.Max(0, troopCount)) * 100 / Math.Max(1, fullStrength);
            var regenNote = Math.Max(0, troopCount) < fullStrength
                ? $" Regenerates +{UnitRules.TroopRegenPerTurn} troops per turn until full."
                : string.Empty;
            return $"Troops {Math.Max(0, troopCount)}/{fullStrength} ({lostPct}% lost). "
                + $"Each 10% lost → −{UnitRules.CasualtyAttackPerStep} Atk/Def, −{UnitRules.CasualtyHpPerStep} HP. "
                + $"Steps {p.Steps}: Atk {p.Attack}, Def {p.Defense}, HP {p.Hp}. "
                + $"Floors while depleted: Atk/Def ≥ {UnitRules.CasualtyMinAttack}, Max HP ≥ {UnitRules.CasualtyMinMaxHp}."
                + regenNote;
        }
    }

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
            int raceMoveBonus = UnitRules.RaceMoveBonus,
            int troopCount = UnitRules.DefaultTroopCount,
            int fullTroopCount = UnitRules.DefaultTroopCount)
        {
            var quality = UnitWeaponQuality.AttackDamageBonus(weaponQuality);
            var skillKey = UnitSkillTree.SkillKeyForWeaponType(primaryWeapon?.WeaponType);
            var attackSkill = skillKey is not null && skillTotals.TryGetValue(skillKey, out var asv) ? asv : 0;
            var weaponAt = (primaryWeapon?.Attack ?? 0) + quality;

            var defKey = ResolveDefenseSkillKey(skillTotals, hasShield: shield is not null, hasArmor: armor is not null);
            var defenseSkill = skillTotals.TryGetValue(defKey, out var dsv) ? dsv : 0;
            var gearDef = (primaryWeapon?.Defense ?? 0) + (armor?.Defense ?? 0) + (shield?.Defense ?? 0);

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
            var loss = UnitCasualtyFormulas.Compute(troopCount, fullTroopCount);

            var attack = attackSkill + weaponAt + commanderAttack + otherAttack + loss.Attack;
            var defense = defenseSkill + gearDef + commanderDefense + otherDefense + loss.Defense;
            var maxHp = build * 2 + will * 2 + endurance + discipline * 3 + otherHp + loss.Hp;

            if (loss.Steps > 0)
            {
                attack = Math.Max(UnitRules.CasualtyMinAttack, attack);
                defense = Math.Max(UnitRules.CasualtyMinDefense, defense);
                maxHp = Math.Max(UnitRules.CasualtyMinMaxHp, maxHp);
            }

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
                LossAttack: loss.Attack,
                LossDefense: loss.Defense,
                LossHp: loss.Hp,
                CasualtySteps: loss.Steps,
                TroopCount: Math.Max(0, troopCount),
                FullTroopCount: Math.Max(1, fullTroopCount),
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
            // Cap only while Training (pass int.MaxValue or 0 after Active graduation).
            if (maxAtGraduation > 0 && maxAtGraduation < int.MaxValue && current + 1 > maxAtGraduation)
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
