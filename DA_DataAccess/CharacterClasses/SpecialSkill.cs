using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class SpecialSkill : Feature
    {
        //[Key]
        //public int Id { get; set; }
        //public int CharacterId { get; set; }

        //public int Index { get; set; }

        //[ForeignKey(nameof(CharacterId))]
        //public Character Character { get; set; }

        //public new string FeatureType { get; set; } = "SpecialSkill";

        public string? RelatedBaseSkillName { get; set; }
        public string? ChosenAttribute { get; set; }
        public bool Editable { get; set; }

    }
}
