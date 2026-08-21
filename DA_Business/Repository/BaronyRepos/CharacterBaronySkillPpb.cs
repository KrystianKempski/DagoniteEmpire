using DA_Common.Barony;
using DA_Models.CharacterModels;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Domain skill units from a full character sheet (same formulas as the baron).
    /// Uses Absolute skill totals (no temp / wounds).
    /// </summary>
    public static class CharacterBaronySkillPpb
    {
        public static PpbVector FromCharacter(CharacterDTO? character)
        {
            if (character is null)
                return new PpbVector();

            CharacterSkillRelations.Wire(character);

            decimal Special(string name)
            {
                if (character.SpecialSkills is null)
                    return 0m;
                foreach (var s in character.SpecialSkills)
                {
                    if (string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase))
                        return s.SumAbsolute;
                }
                return 0m;
            }

            decimal Base(string name)
            {
                if (character.BaseSkills is null)
                    return 0m;
                foreach (var s in character.BaseSkills)
                {
                    if (string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase))
                        return s.SumAbsolute;
                }
                return 0m;
            }

            decimal Attr(string name)
            {
                if (character.Attributes is null)
                    return 0m;
                foreach (var a in character.Attributes)
                {
                    if (string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase))
                        return a.ModifierAbsolute;
                }
                return 0m;
            }

            return BaronSkillPpbFormulas.Compute(Special, Base, Attr);
        }
    }
}
