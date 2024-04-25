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
        public string RelatedAttribute { get; set; } = string.Empty;
        public int ClassLevel { get; set; } = 1;

        public int MaxFocusPoints { get; set; } = 0;
        public int CurrentCofusPoints { get; set; } = 0;

        public ICollection<CharacterDTO>? Characters { get; set; } = new List<CharacterDTO>();

        public ICollection<ProfessionSkill>? ActiveSkills { get; set; } 

        public ICollection<ProfessionSkill>? PassiveSkills { get; set; } 

        public bool IsApproved { get; set; } = false;

        public bool IsUniversal { get; set; } = false;
        // dto
        public int ProfessionSkillRoll { get; set; } = 0;


        public AttributeDTO? RelatedAttributeD { get; set; }

        public ProfessionDTO()
        {
            ActiveSkills = new List<ProfessionSkill>()
            {
                new ProfessionSkill(){ Level = 1, Index=0},
                new ProfessionSkill(){ Level = 1, Index=1},
                new ProfessionSkill(){ Level = 2, Index=2},
                new ProfessionSkill(){ Level = 2, Index=3},
                new ProfessionSkill(){ Level = 3, Index=4},
                new ProfessionSkill(){ Level = 3, Index=5},
                new ProfessionSkill(){ Level = 4, Index=6},
                new ProfessionSkill(){ Level = 4, Index=7},
                new ProfessionSkill(){ Level = 5, Index=8},
                new ProfessionSkill(){ Level = 5, Index=9},
                new ProfessionSkill(){ Level = 6, Index=10},
                new ProfessionSkill(){ Level = 6, Index=11},
                new ProfessionSkill(){ Level = 7, Index=12},
                new ProfessionSkill(){ Level = 7, Index=13},
            };
            PassiveSkills = new List<ProfessionSkill>()
            {
                new ProfessionSkill(){ Level = 1, Index=0},
                new ProfessionSkill(){ Level = 2, Index=1},
                new ProfessionSkill(){ Level = 3, Index=2},
                new ProfessionSkill(){ Level = 4, Index=3},
                new ProfessionSkill(){ Level = 5, Index=4},
                new ProfessionSkill(){ Level = 6, Index=5},
                new ProfessionSkill(){ Level = 7, Index=6},
            };
        }
        public void CalculateClassParams(IEnumerable<AttributeDTO> attributes)
        {
            if (string.IsNullOrEmpty(RelatedAttribute))
                return;
            var attr = attributes.FirstOrDefault(u => u.Name == RelatedAttribute);
            if (attr is null)
                return;
            ProfessionSkillRoll = attr.Modifier + ClassLevel;
            MaxFocusPoints = ProfessionSkillRoll * 4 + 4;
            if (MaxFocusPoints < 4)
                MaxFocusPoints = 4;
        }

        public void AddPropertyListener(AttributeDTO attr)
        {
            RelatedAttributeD = attr;
            if (RelatedAttributeD == null) return;
            RelatedAttributeD.ModifierChanged += A_PropertyChanged;
        }

        private void A_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(RelatedAttributeD.Modifier)) return;
            if (RelatedAttributeD == null) return;

            ProfessionSkillRoll = RelatedAttributeD.Modifier + ClassLevel;
            MaxFocusPoints = ProfessionSkillRoll * 4 + 4;
            if (MaxFocusPoints < 4)
                MaxFocusPoints = 4;

        }
    }
}
