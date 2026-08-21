using DA_Common;
using DA_Common.Barony;
using DA_Models.CharacterModels;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Starting / floor Commander XP (CX) from skills.
    /// Character sheets (baron, linked courtiers): (Inspire + Strategy and tactics) × 2.
    /// Court sheets: (Command + Strategy/tactics) × 4.
    /// </summary>
    public static class CommanderCxFormulas
    {
        public const int CharacterSkillMultiplier = 2;
        public const int CourtSheetMultiplier = 4;

        /// <summary>Permanent special-skill total (excludes wounds and temporary bonuses on the skill).</summary>
        public static int PermanentSpecialSkill(CharacterDTO? character, string skillName) =>
            AbsoluteSpecialSkill(character, skillName);

        /// <summary>Base skill Absolute total (no temp / wounds).</summary>
        public static int AbsoluteBaseSkill(CharacterDTO? character, string skillName)
        {
            if (character?.BaseSkills is null || string.IsNullOrWhiteSpace(skillName))
                return 0;

            CharacterSkillRelations.Wire(character);
            foreach (var s in character.BaseSkills)
            {
                if (!string.Equals(s.Name, skillName, StringComparison.OrdinalIgnoreCase))
                    continue;
                return Math.Max(0, s.SumAbsolute);
            }
            return 0;
        }

        /// <summary>Special skill Absolute total (no temp / wounds).</summary>
        public static int AbsoluteSpecialSkill(CharacterDTO? character, string skillName)
        {
            if (character?.SpecialSkills is null || string.IsNullOrWhiteSpace(skillName))
                return 0;

            CharacterSkillRelations.Wire(character);
            foreach (var s in character.SpecialSkills)
            {
                if (!string.Equals(s.Name, skillName, StringComparison.OrdinalIgnoreCase))
                    continue;
                return Math.Max(0, s.SumAbsolute);
            }
            return 0;
        }

        /// <summary>
        /// When the tier lists characterRequirements, evaluate them with Absolute skill totals.
        /// Returns null when the court-sheet gate should be used instead.
        /// </summary>
        public static bool? MeetsCharacterCommanderRequirements(
            CharacterDTO? character,
            CourtCommanderAbility ability)
        {
            if (character is null)
                return null;

            var tier = CourtCommanderCatalog.FindTierRequirement(ability.Branch, ability.Tier);
            if (tier is null || tier.CharacterRequirements.Count == 0)
                return null;

            CharacterSkillRelations.Wire(character);
            foreach (var req in tier.CharacterRequirements)
            {
                var value = ResolveCharacterRequirementValue(character, req);
                if (value < req.Min)
                    return false;
            }
            return true;
        }

        public static Func<CourtCommanderAbility, bool?>? CharacterSkillGate(CharacterDTO? character)
        {
            if (character is null)
                return null;
            return ability => MeetsCharacterCommanderRequirements(character, ability);
        }

        private static int ResolveCharacterRequirementValue(
            CharacterDTO character,
            CourtCommanderSkillRequirement req)
        {
            var key = req.SkillKey.Trim();
            if (string.Equals(req.Kind, "special", StringComparison.OrdinalIgnoreCase))
            {
                // JSON uses short names; map onto SD special-skill names.
                if (key.Equals("Riding", StringComparison.OrdinalIgnoreCase))
                    return AbsoluteSpecialSkill(character, SD.SpecialSkills.AnimalHandle.Riding);
                if (key.Equals("Armor", StringComparison.OrdinalIgnoreCase))
                    return AbsoluteSpecialSkill(character, SD.SpecialSkills.Athletics.Armor);
                return AbsoluteSpecialSkill(character, key);
            }

            // Base skills: Melee, Shooting, Acrobatics, Deceit, Perception, …
            return AbsoluteBaseSkill(character, key);
        }

        public static int BaseCxFromCharacter(CharacterDTO? character)
        {
            var inspire = PermanentSpecialSkill(character, UnitActionFormulas.CharacterCommandSkill);
            var strategy = PermanentSpecialSkill(character, UnitActionFormulas.CharacterStrategySkill);
            return (inspire + strategy) * CharacterSkillMultiplier;
        }

        public static int BaseCxFromCourtSheet(CourtCharacterSheet? sheet)
        {
            if (sheet is null)
                return 0;
            sheet.Normalize();
            var command = sheet.GetMain(CourtMainSkill.Command) + sheet.GetMainOtherSum(CourtMainSkill.Command);
            var strategy = sheet.GetSecondary(CourtSecondarySkill.StrategyTactics);
            return Math.Max(0, command + strategy) * CourtSheetMultiplier;
        }

        /// <summary>Raise the CX pool to at least <paramref name="baseCx"/> (keeps battle/MG surplus).</summary>
        public static bool EnsureMinimumPool(CourtCharacterSheet sheet, int baseCx)
        {
            sheet.Normalize();
            var floor = Math.Max(0, baseCx);
            if (sheet.CommanderXp >= floor)
                return false;
            sheet.CommanderXp = floor;
            return true;
        }

        /// <summary>
        /// Overlay character Inspire / Strategy onto court Command / Strategy for tree skill gates.
        /// Preserves CX pool and unlocked abilities.
        /// </summary>
        public static void ProjectCharacterSkillsOntoSheet(CourtCharacterSheet sheet, CharacterDTO? character)
        {
            sheet.Normalize();
            var inspire = PermanentSpecialSkill(character, UnitActionFormulas.CharacterCommandSkill);
            var strategy = PermanentSpecialSkill(character, UnitActionFormulas.CharacterStrategySkill);
            sheet.Main[CourtMainSkill.Command] = CourtSkillCatalog.ClampMain(CourtMainSkill.Command, inspire);
            SetSecondary(sheet, CourtSecondarySkill.StrategyTactics, strategy);
            sheet.Normalize();
        }

        /// <summary>Build / refresh a commander sheet for a full character (baron or linked courtier).</summary>
        public static CourtCharacterSheet BuildCharacterCommanderSheet(
            CourtCharacterSheet? existing,
            CharacterDTO? character)
        {
            var sheet = existing ?? CourtCharacterSheet.CreateDefault();
            sheet.Normalize();
            ProjectCharacterSkillsOntoSheet(sheet, character);
            EnsureMinimumPool(sheet, BaseCxFromCharacter(character));
            return sheet;
        }

        public static CourtCharacterSheet EnsureCourtSheetCx(CourtCharacterSheet sheet)
        {
            sheet.Normalize();
            EnsureMinimumPool(sheet, BaseCxFromCourtSheet(sheet));
            return sheet;
        }

        private static void SetSecondary(CourtCharacterSheet sheet, string key, int value)
        {
            var clamped = CourtSkillCatalog.ClampSecondary(value);
            var existing = sheet.Secondary.FirstOrDefault(s =>
                string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
                sheet.Secondary.Add(new CourtSecondaryEntry { Key = key, Value = clamped });
            else
                existing.Value = clamped;
        }
    }
}
