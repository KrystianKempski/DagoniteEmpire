using Abp.Collections.Extensions;
using Castle.Core;
using DA_Common;
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

        public ICollection<SpecialSkillDTO> SkillsToDelete { get; set; } = new List<SpecialSkillDTO>();
        public IEnumerable<SpecialSkillDTO> GetAllSection(string baseSkillName)
        {
            var canonical = SD.BaseSkills.Canonical(baseSkillName);
            return Properties.Values.Where(u => SD.BaseSkills.Canonical(u.RelatedBaseSkillName) == canonical);
        }
        public override SpecialSkillDTO? Get(string key)
        {
            var skill = base.Get(key);
            if (skill is null)
            {
                var canonical = SD.SpecialSkills.Canonical(key);
                skill = base.Get(canonical);
                if (skill is null)
                {
                    skill = Properties.Values.FirstOrDefault(s =>
                        string.Equals(SD.SpecialSkills.Canonical(s.Name), canonical, StringComparison.Ordinal));
                }
            }
            if (skill is null) return null;
            FillRelatedProperties(skill);
            return skill;
        }

        public SpecialSkillDTO? Add(SpecialSkillDTO newSkill)
        {
            if (newSkill is null || newSkill.Name.IsNullOrEmpty())
                return null;
            FillRelatedProperties(newSkill);
            newSkill.Name = SD.SpecialSkills.Canonical(newSkill.Name);
            if (!string.IsNullOrEmpty(newSkill.RelatedBaseSkillName))
                newSkill.RelatedBaseSkillName = SD.BaseSkills.Canonical(newSkill.RelatedBaseSkillName);
            Properties.Add(newSkill.Name, newSkill);
            return newSkill;
        }

        public void Remove(string newSkillName)
        {
            if (newSkillName.IsNullOrEmpty())
                return;
            var temp = Properties[newSkillName];
            if(temp.Id>0)
                SkillsToDelete.Add(Properties[newSkillName]);
            Properties.Remove(newSkillName);
        }
        public void ReplaceNames(string newSkillName, string oldSkillName)
        {

            SpecialSkillDTO temp = Properties[oldSkillName];
            SpecialSkillDTO newSkill = new SpecialSkillDTO(temp);

            newSkill.Name = newSkillName;
            Add(newSkill);
            Properties.Remove(oldSkillName);
        }

        private void FillRelatedProperties(SpecialSkillDTO skill)
        {
            if (skill is null)
                return;

            if (skill.RelatedAttribute is null && skill.ChosenAttribute.IsNullOrEmpty() == false)
                skill.RelatedAttribute = _allParams.Attributes.Get(skill.ChosenAttribute);

            if (skill.RelatedBaseSkill is null && skill.RelatedBaseSkillName.IsNullOrEmpty() == false)
                skill.RelatedBaseSkill = _allParams.GetBaseSkill(skill.RelatedBaseSkillName);
        }

        // move to specialskillmodel
        public void AutoChooseRelatedAttribute()
        {
            foreach (var skill in _allParams.SpecialSkills.GetAllArray())
            {
                if (skill.RelatedAttribute1 != "")
                {
                    var attr1 = _allParams.Attributes.Get(skill.RelatedAttribute1);
                    var attr2 = _allParams.Attributes.Get(skill.RelatedAttribute2);
                    if (attr1 != null && attr2 != null)
                    {
                        if (attr1.SumBonus >= attr2.SumBonus)
                        {
                            skill.ChosenAttribute = attr1.Name;
                        }
                        else
                        {
                            skill.ChosenAttribute = attr2.Name;
                        }
                        ChangeSSRelatedAttribute(skill.ChosenAttribute, skill.Name);
                    }
                }
            }
        }
        public void ChangeSSRelatedAttribute(string attrName, string specSkillName)
        {
            if (attrName != null && attrName != "0" && specSkillName != null)
            {
                var obj = _allParams.SpecialSkills.Get(specSkillName);
                if (obj != null)
                {
                    obj.ChosenAttribute = attrName;
                    obj.AddPropertyListener(_allParams.Attributes.Get(attrName));
                }
                return;
            }
        }


        public void AddSSRelatedBaseSkill(SpecialSkillDTO obj)
        {
            if (obj != null && obj.RelatedBaseSkillName != null && obj.Name != null)
            {
                var baseSkill = _allParams.GetBaseSkill(obj.RelatedBaseSkillName);
                if (baseSkill == null) return;
                obj.AddPropertyListener(baseSkill);
                return;
            }
        }

        public void AddRelatedParametes()
        {
            foreach (var obj in _allParams.SpecialSkills.GetAllArray())
            {
                ChangeSSRelatedAttribute(obj.ChosenAttribute, obj.Name);
                AddSSRelatedBaseSkill(obj);
            }
        }

        public void FillPropertiesContainer(ICollection<SpecialSkillDTO> properties)
        {
            foreach (var property in properties)
            {
                property.Name = SD.SpecialSkills.Canonical(property.Name);
                if (!string.IsNullOrEmpty(property.RelatedBaseSkillName))
                    property.RelatedBaseSkillName = SD.BaseSkills.Canonical(property.RelatedBaseSkillName);
                Properties[property.Name] = property;
            }
        }
    }
}
