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

        // move to specialskillmodel
        public void AutoChooseRelatedAttribute()
        {
            foreach (var skill in _allParams.SpecialSkills.GetAllArray())
            {
                if (skill.RelatedAttribute1 != "")
                {
                    var attr1 = _allParams.Attributes[skill.RelatedAttribute1];
                    var attr2 = _allParams.Attributes[skill.RelatedAttribute2];
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
                    }
                }
            }
        }


        public void ChangeSSRelatedAttribute(string attrName, string specSkillName)
        {

            try
            {
                if (attrName != null && attrName != "0" && specSkillName != null)
                {
                    var obj = _allParams.SpecialSkills.Get(specSkillName);
                    if (obj != null)
                    {
                        obj.ChosenAttribute = attrName;
                        var attr = _allParams.Attributes[attrName];
                        if (attr == null) return;
                        //obj.AttributeBonus = attr.Modifier;
                        obj.AddPropertyListener(attr);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }


        public void AddSSRelatedBaseSkill(SpecialSkillDTO obj)
        {
            try
            {
                if (obj != null && obj.RelatedBaseSkillName != null && obj.Name != null)
                {
                    var baseSkill = _allParams.BaseSkills.FirstOrDefault(p => p.Name == obj.RelatedBaseSkillName);
                    if (baseSkill == null) return;
                    obj.AddPropertyListener(baseSkill);
                    return;
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        public void InitSpecialSkills()
        {

            foreach (var obj in _allParams.SpecialSkills.GetAllArray())
            {
                ChangeSSRelatedAttribute(obj.ChosenAttribute, obj.Name);
                AddSSRelatedBaseSkill(obj);
            }
        }
    }
}
