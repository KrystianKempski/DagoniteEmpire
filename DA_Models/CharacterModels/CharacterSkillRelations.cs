using System;
using System.Linq;

namespace DA_Models.CharacterModels
{
    /// <summary>
    /// Wires special-skill listeners so <see cref="SpecialSkillDTO.SumBonus"/> matches the character sheet
    /// (base skill + chosen attribute modifier).
    /// </summary>
    public static class CharacterSkillRelations
    {
        public static void Wire(CharacterDTO? character)
        {
            if (character?.SpecialSkills is null || character.SpecialSkills.Count == 0)
                return;

            var attrs = character.Attributes?.ToList() ?? new();
            var bases = character.BaseSkills?.ToList() ?? new();

            foreach (var skill in character.SpecialSkills)
            {
                if (string.IsNullOrWhiteSpace(skill.ChosenAttribute)
                    && !string.IsNullOrWhiteSpace(skill.RelatedAttribute1)
                    && !string.IsNullOrWhiteSpace(skill.RelatedAttribute2))
                {
                    var a1 = FindAttr(attrs, skill.RelatedAttribute1);
                    var a2 = FindAttr(attrs, skill.RelatedAttribute2);
                    if (a1 is not null && a2 is not null)
                        skill.ChosenAttribute = a1.SumBonus >= a2.SumBonus ? a1.Name : a2.Name;
                }

                if (!string.IsNullOrWhiteSpace(skill.ChosenAttribute))
                {
                    var attr = FindAttr(attrs, skill.ChosenAttribute);
                    if (attr is not null)
                        skill.AddPropertyListener(attr);
                }

                if (!string.IsNullOrWhiteSpace(skill.RelatedBaseSkillName))
                {
                    var baseSkill = bases.FirstOrDefault(b =>
                        string.Equals(b.Name, skill.RelatedBaseSkillName, StringComparison.OrdinalIgnoreCase));
                    if (baseSkill is not null)
                        skill.AddPropertyListener(baseSkill);
                }
            }
        }

        private static AttributeDTO? FindAttr(System.Collections.Generic.List<AttributeDTO> attrs, string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            return attrs.FirstOrDefault(a =>
                string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
