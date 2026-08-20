using DA_Common.Localization;

namespace DA_Common.Barony
{
    /// <summary>Per-turn orders for an active army unit.</summary>
    public static class UnitActionKind
    {
        public const string None = "";
        public const string Patrol = "patrol";
        public const string Scout = "scout";
        public const string Training = "training";
        public const string Work = "work";
        public const string PartialDemobilization = "partial-demobilization";

        public static readonly string[] All =
        {
            Patrol,
            Scout,
            Training,
            Work,
            PartialDemobilization,
        };

        public static string Normalize(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return None;
            foreach (var k in All)
            {
                if (string.Equals(k, key.Trim(), StringComparison.OrdinalIgnoreCase))
                    return k;
            }
            return None;
        }

        public static string DisplayName(string? key) => Normalize(key) switch
        {
            Patrol => Loc.T("Patrol"),
            Scout => Loc.T("Reconnaissance"),
            Training => Loc.T("Training"),
            Work => Loc.T("Labour"),
            PartialDemobilization => Loc.T("Partial demobilization"),
            _ => Loc.T("No action"),
        };

        public static string Description(string? key) => Normalize(key) switch
        {
            Patrol => Loc.T("Stability = City patrol ÷ 2 (floor); Law = City patrol ÷ 3 (floor)."),
            Scout => Loc.T("Intelligence = Wilderness orientation."),
            Training => Loc.T("Grants unit XP from the captain’s Command and Strategy / tactics each Resolve Turn."),
            Work => Loc.T("Production = Fortification building."),
            PartialDemobilization => Loc.T("Halves gold, food and Defense upkeep for this turn."),
            _ => Loc.T("No peacetime order — no action bonuses."),
        };

        public static bool GrantsDomainBonus(string? key)
        {
            var n = Normalize(key);
            return n is Patrol or Scout or Work;
        }

        public static bool GrantsTrainingXp(string? key)
            => Normalize(key) == Training;

        public static bool HalvesUpkeep(string? key)
            => Normalize(key) == PartialDemobilization;

        public static bool IsPartialDemobilization(string? key)
            => Normalize(key) == PartialDemobilization;
    }

    /// <summary>Who leads the unit for Training XP formulas.</summary>
    public enum UnitCaptainKind
    {
        None = 0,
        /// <summary>Simplified court sheet (no linked CharacterId).</summary>
        CourtSheet = 1,
        /// <summary>Full character sheet attached as courtier.</summary>
        LinkedCharacter = 2,
        /// <summary>The baron leads the unit.</summary>
        Baron = 3,
    }

    /// <summary>Domain / XP effects of unit peacetime actions.</summary>
    public static class UnitActionFormulas
    {
        /// <summary>
        /// Domain Skill bonuses from the unit’s current action.
        /// Suppressed entirely when a battle is in progress this turn.
        /// </summary>
        public static PpbVector DomainBonus(
            string? action,
            IReadOnlyDictionary<string, int>? skillTotals,
            bool battleSuppresses)
        {
            var result = new PpbVector();
            if (battleSuppresses)
                return result;

            var kind = UnitActionKind.Normalize(action);
            if (!UnitActionKind.GrantsDomainBonus(kind) || skillTotals is null)
                return result;

            int Skill(string key) =>
                skillTotals.TryGetValue(key, out var v) ? Math.Max(0, v) : 0;

            switch (kind)
            {
                case UnitActionKind.Patrol:
                {
                    var patrol = Skill(UnitSkillKey.CityPatrol);
                    result[Ppb.Stability] = Math.Floor(patrol / 2m);
                    result[Ppb.Law] = Math.Floor(patrol / 3m);
                    break;
                }
                case UnitActionKind.Scout:
                    result[Ppb.Intelligence] = Skill(UnitSkillKey.Wilderness);
                    break;
                case UnitActionKind.Work:
                    result[Ppb.Production] = Skill(UnitSkillKey.Fortification);
                    break;
            }

            return result;
        }

        public static string? DomainBonusFormula(
            string? action,
            IReadOnlyDictionary<string, int>? skillTotals)
        {
            var kind = UnitActionKind.Normalize(action);
            if (!UnitActionKind.GrantsDomainBonus(kind))
                return null;

            int Skill(string key) =>
                skillTotals is not null && skillTotals.TryGetValue(key, out var v) ? Math.Max(0, v) : 0;

            return kind switch
            {
                UnitActionKind.Patrol =>
                    Loc.T("Patrol: Stability ⌊{0}/2⌋, Law ⌊{0}/3⌋", Skill(UnitSkillKey.CityPatrol)),
                UnitActionKind.Scout =>
                    Loc.T("Reconnaissance: Intelligence = Wilderness orientation ({0})", Skill(UnitSkillKey.Wilderness)),
                UnitActionKind.Work =>
                    Loc.T("Labour: Production = Fortification building ({0})", Skill(UnitSkillKey.Fortification)),
                _ => null,
            };
        }

        /// <summary>
        /// XP (PD) gained this turn from Training.
        /// Court sheet: Command + Strategy/tactics.
        /// Linked character / baron: ⌊(Command + Strategy)/2⌋; baron also × JC/100.
        /// </summary>
        public static int TrainingXp(
            UnitCaptainKind captainKind,
            int command,
            int strategy,
            int trainingJc,
            bool battleSuppresses)
        {
            if (battleSuppresses || captainKind == UnitCaptainKind.None)
                return 0;

            command = Math.Max(0, command);
            strategy = Math.Max(0, strategy);

            return captainKind switch
            {
                UnitCaptainKind.CourtSheet => command + strategy,
                UnitCaptainKind.LinkedCharacter => (command + strategy) / 2,
                UnitCaptainKind.Baron =>
                    (int)Math.Floor((command + strategy) / 2m * ClampJc(trainingJc) / 100m),
                _ => 0,
            };
        }

        public static int ClampJc(int jc) => Math.Clamp(jc, 0, 100);

        /// <summary>
        /// Partial demobilization: half gold / food / Defense upkeep (floored).
        /// Suppressed while a battle is in progress.
        /// </summary>
        public static UnitUpkeepTotals ApplyUpkeepModifier(
            UnitUpkeepTotals upkeep,
            string? action,
            bool battleSuppresses)
        {
            if (battleSuppresses || !UnitActionKind.HalvesUpkeep(action) || upkeep.MaintenanceExempt)
                return upkeep;

            return upkeep with
            {
                Gold = FloorHalf(upkeep.Gold),
                Food = FloorHalf(upkeep.Food),
                Defense = FloorHalf(upkeep.Defense),
                BaseWage = FloorHalf(upkeep.BaseWage),
                GearGold = FloorHalf(upkeep.GearGold),
            };
        }

        public static string? UpkeepModifierFormula(string? action, bool battleSuppresses)
        {
            if (!UnitActionKind.HalvesUpkeep(action))
                return null;
            if (battleSuppresses)
                return Loc.T("Action bonuses suppressed (battle in progress).");
            return Loc.T("Partial demobilization: upkeep × ½ (gold, food, Defense).");
        }

        private static int FloorHalf(int value) => (int)Math.Floor(value / 2m);
        private static decimal FloorHalf(decimal value) => Math.Floor(value / 2m);

        /// <summary>Character-sheet skill used as “Command” (Dowodzenie) for Training XP.</summary>
        public const string CharacterCommandSkill = "Inspire";

        /// <summary>Character-sheet skill used as Strategy for Training XP.</summary>
        public const string CharacterStrategySkill = "Strategy and tactics";
    }
}
