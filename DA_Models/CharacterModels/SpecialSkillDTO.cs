using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using DA_Common;

namespace DA_Models.CharacterModels
{
    public class SpecialSkillDTO : FeatureDTO
    {
        public SpecialSkillDTO() { }
        public SpecialSkillDTO(SpecialSkillDTO copy)
        {
            this.RelatedBaseSkill = copy.RelatedBaseSkill;
            this.Name = copy.Name;
            this.Index = copy.Index;
            this.OtherBonuses = copy.OtherBonuses;
            this.RaceBonus = copy.RaceBonus;
            this.RelatedAttribute = copy.RelatedAttribute;
            this.RelatedAttribute1 = copy.RelatedAttribute1;
            this.RelatedAttribute2  = copy.RelatedAttribute2;
            this.RelatedBaseSkillName = copy.RelatedBaseSkillName;
            this.TempBonuses = copy.TempBonuses;
            this.HealthBonus = copy.HealthBonus;
            this.Id = copy.Id;
            this.Editable = copy.Editable;
            this.GearBonus = copy.GearBonus;
            this.FeatureType = copy.FeatureType;
            this.BaseBonus = copy.BaseBonus;    
            this.CharacterId = copy.CharacterId;
            this.TraitBonus = copy.TraitBonus;
            this._sumBonus = copy._sumBonus;
        }
        public string RelatedBaseSkillName { get; set; }
        public override string FeatureType { get; set; } = SD.FeatureSpecialSkill;

        public override int SumBonus
        {
            get => _sumBonus = base.SumBonus + AttributeBonus + BaseSkillBonus;
        }
        public int AttributeBonus { get => RelatedAttribute is not null ? RelatedAttribute.Modifier : 0 ; }  
        public int BaseSkillBonus { get => RelatedBaseSkill is not null ? RelatedBaseSkill.SumBonus : 0; }

        public bool Editable { get; set; } = false;

        public string RelatedAttribute1 { get; set; } = "";
        public string RelatedAttribute2 { get; set; } = "";

        public string? ChosenAttribute { get; set; } = null;


        // Property changes events and property listeners for Attribute and Base skill related to Special Skill

        public AttributeDTO? RelatedAttribute;
        public BaseSkillDTO? RelatedBaseSkill;

        public void AddPropertyListener(AttributeDTO attr)
        {
            if(attr is not null)
                RelatedAttribute = attr;
        }
        public void AddPropertyListener(BaseSkillDTO baseSkill)
        {
            if (baseSkill is not null)
                RelatedBaseSkill = baseSkill;
        }

    }
}
