using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class SpecialSkill : Skill
    {
        [Key]
        public int Id { get; set; }
        //public int BaseSkillId { get; set; } = 0;
        //[ForeignKey(nameof(BaseSkillId))]
        //public BaseSkill BaseSkill { get; set; }
        public string? Type { get; set; }
       // public int AtributeId { get; set; }

        //[ForeignKey(nameof(AtributeId))]
        //public Attribute RelatedAttribute { get; set; }
       

    }
}
