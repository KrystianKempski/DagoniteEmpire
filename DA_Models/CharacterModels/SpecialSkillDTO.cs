using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DA_Models.CharacterModels
{
    public class SpecialSkillDTO
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }

        public int Index { get; set; }
        public string RelatedBaseSkillName { get; set; } 
        [Required]
        public string Name { get; set; }
        [Range(0, 5, ErrorMessage = "Base bonus must be between 0 and 5")]
        public int BaseBonus { get; set; } = 0;
        [Range(0, 2, ErrorMessage = "Race bonus must be between 0 and 5")]
        public int RaceBonus { get; set; } = 0;
        [Range(0, 5, ErrorMessage = "Gear bonus must be between 0 and 5")]
        public int GearBonus { get; set; } = 0;
        [Range(0, 5, ErrorMessage = "Other bonus must be between 0 and 5")]
        public int OtherBonuses { get; set; } = 0;

        public int TempBonuses { get; set; } = 0;

        public int AttributeBonus { get; set; } = 0;
        public int BaseSkillBonus { get; set; } = 0;

        public bool Editable { get; set; } = false;

        public string RelatedAttribute1 { get; set; } = "";
        public string RelatedAttribute2 { get; set; } = "";

        public string? ChosenAttribute { get; set; } = null;

        public int SumBonus { get; set; } = 0;
       

        public void DecrRace() { if (RaceBonus > -6) RaceBonus--; SumAll(); }
        public void IncrRace() { if (RaceBonus < 6) RaceBonus++; SumAll(); }

        public void DecrGear() { if (GearBonus > -6) GearBonus--; SumAll(); }
        public void IncrGear() { if (GearBonus < 6) GearBonus++; SumAll(); }

        public void DecrOther() { if (OtherBonuses > -6) OtherBonuses--; SumAll(); }
        public void IncrOther() { if (OtherBonuses < 6) OtherBonuses++; SumAll(); }
        public void DecrTemp() { TempBonuses--; SumAll(); }
        public void IncrTemp() { TempBonuses++; SumAll(); }

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
            SumAll();
        }

        private void BS_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(RelatedBaseSkill.SumBonus)) return;
            if (RelatedBaseSkill == null) return;

            BaseSkillBonus = RelatedBaseSkill.SumBonus;
            SumAll();
        }

        public int SumAll()
        {
            SumBonus = BaseBonus + AttributeBonus + BaseSkillBonus + RaceBonus + GearBonus + TempBonuses + OtherBonuses;
            return SumBonus;
        }

    }
}
