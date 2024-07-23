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
        public string RelatedBaseSkillName { get; set; }
        public override string FeatureType { get; set; } = SD.FeatureSpecialSkill;

        public override int SumBonus
        {
            get => _sumBonus = base.SumBonus + AttributeBonus + BaseSkillBonus;
        }
        public int AttributeBonus { get; set; } = 0;
        public int BaseSkillBonus { get; set; } = 0;

        public bool Editable { get; set; } = false;

        public string RelatedAttribute1 { get; set; } = "";
        public string RelatedAttribute2 { get; set; } = "";

        public string? ChosenAttribute { get; set; } = null;


        // Property changes events and property listeners for Attribute and Base skill related to Special Skill

        public AttributeDTO? RelatedAttribute;
        public BaseSkillDTO? RelatedBaseSkill;

        public void AddPropertyListener(AttributeDTO attr)
        {
            RelatedAttribute = attr;
            if (RelatedAttribute == null) return;
            RelatedAttribute.ModifierChanged += A_PropertyChanged;
        }
        public void AddPropertyListener(BaseSkillDTO baseSkill)
        {
            RelatedBaseSkill = baseSkill;
            if (RelatedBaseSkill == null) return;
            RelatedBaseSkill.SumChanged += BS_PropertyChanged;
        }
        private void A_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(RelatedAttribute.Modifier)) return;
            if (RelatedAttribute == null) return;

            AttributeBonus = RelatedAttribute.Modifier;
        }

        private void BS_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(RelatedBaseSkill.SumBonus)) return;
            if (RelatedBaseSkill == null) return;

            BaseSkillBonus = RelatedBaseSkill.SumBonus;
        }


    }
}
