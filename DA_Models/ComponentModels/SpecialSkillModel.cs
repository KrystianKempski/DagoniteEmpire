using Abp.Collections.Extensions;
using Castle.Core;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using DA_Models.ComponentInterfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ComponentModels
{
    public class SpecialSkillModel : ParameterModel<SpecialSkillDTO>
    {
        public SpecialSkillModel(AllParamsModel allParams) : base(allParams)
        {

        }
        public IEnumerable<SpecialSkillDTO> GetAllSection(string baseSkillName)
        {

            return Properties.Values.Where(u=>u.RelatedBaseSkillName == baseSkillName);
        }
        public override SpecialSkillDTO? Get(string key)
        {
            var skill= base.Get(key);
            if (skill is null) return null;
            FillRelatedProperties(skill);
            return skill;
        }

        public void Init(ICollection<SpecialSkillDTO> properties)        {
            foreach (var skill in properties)
            {
                FillRelatedProperties(skill);
                Properties[skill.Name] = skill;

            }
        }
        public SpecialSkillDTO? Add(SpecialSkillDTO newSkill)
        {
            if (newSkill is null || newSkill.Name.IsNullOrEmpty())
                return null;
            FillRelatedProperties(newSkill);
            Properties.Add(newSkill.Name, newSkill);
            return newSkill;
        }

        private void FillRelatedProperties(SpecialSkillDTO skill)
        {
            if (skill is null)
                return;

            if (skill.RelatedAttribute is null && skill.ChosenAttribute.IsNullOrEmpty() == false)
                skill.RelatedAttribute = _allParams.Attributes[skill.ChosenAttribute];

            if (skill.RelatedBaseSkill is null && skill.RelatedBaseSkillName.IsNullOrEmpty() == false)
                skill.RelatedBaseSkill = _allParams.BaseSkills.FirstOrDefault(u => u.Name == skill.RelatedBaseSkillName);
        }
    }
}
