using DA_Common;
using DA_DataAccess.CharacterClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class ProfessionDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RelatedAttributeName { get; set; } = string.Empty;
        public int ClassLevel { get; set; } = 1;

        public int MaxFocusPoints { get; set; } = 0;
        public int CurrentCofusPoints { get; set; } = 0;

        public ICollection<CharacterDTO>? Characters { get; set; } = new List<CharacterDTO>();
        //public ICollection<TraitProfessionDTO>? ActiveSkills { get; set; }
        //public ICollection<TraitProfessionDTO>? PassiveSkills { get; set;

        public ICollection<TraitProfessionDTO>? Traits { get; set; }

        public SpellcasterType CasterType { get; set; } = SpellcasterType.None;

        public ICollection<SpellCircleDTO>? SpellCircles { get;set; }


        public bool IsApproved { get; set; } = false;

        public bool IsUniversal { get; set; } = false;
        // dto
        public int ProfessionSkillRoll { get; set; } = 0;


        public AttributeDTO? RelatedAttribute { get; set; }

        public ProfessionDTO()
        {
            Traits = new List<TraitProfessionDTO>()
            {
                //active
                new TraitProfessionDTO(){ Level = 1, Index=0},
                new TraitProfessionDTO(){ Level = 1, Index=1},
                new TraitProfessionDTO(){ Level = 2, Index=2},
                new TraitProfessionDTO(){ Level = 2, Index=3},
                new TraitProfessionDTO(){ Level = 3, Index=4},
                new TraitProfessionDTO(){ Level = 3, Index=5},
                new TraitProfessionDTO(){ Level = 4, Index=6},
                new TraitProfessionDTO(){ Level = 4, Index=7},
                new TraitProfessionDTO(){ Level = 5, Index=8},
                new TraitProfessionDTO(){ Level = 5, Index=9},
                new TraitProfessionDTO(){ Level = 6, Index=10},
                new TraitProfessionDTO(){ Level = 6, Index=11},
                new TraitProfessionDTO(){ Level = 7, Index=12},
                new TraitProfessionDTO(){ Level = 7, Index=13},
                // passive
                new TraitProfessionDTO(){ Level = 1, Index=0, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 2, Index=1, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 3, Index=2, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 4, Index=3, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 5, Index=4, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 6, Index=5, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 7, Index=6, IsActiveSkill = false},
            };
            Description = string.Empty;
        }
        public void CalculateClassParams(IDictionary<string, AttributeDTO> attributes)
        {
            if (string.IsNullOrEmpty(RelatedAttributeName))
                return;
            var attr = attributes[RelatedAttributeName];
            if (attr is null)
                return;
            //attr.SumAll();
            RelatedAttribute = attr;
            ProfessionSkillRoll = attr.ModifierAbsolute  + ClassLevel;
            MaxFocusPoints = ProfessionSkillRoll * 4 + 4;
            if (MaxFocusPoints < 4)
                MaxFocusPoints = 4;
        }

        public void AddPropertyListener(AttributeDTO attr)
        {
            if (attr == null) return;
            RelatedAttribute = attr;
            //RelatedAttribute.ModifierChanged += A_PropertyChanged;
        }

        private void A_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(RelatedAttribute.Modifier)) return;
            if (RelatedAttribute == null) return;

            ProfessionSkillRoll = RelatedAttribute.ModifierAbsolute + ClassLevel;
            MaxFocusPoints = ProfessionSkillRoll * 4 + 4;
            if (MaxFocusPoints < 4)
                MaxFocusPoints = 4;

        }
    }
}
