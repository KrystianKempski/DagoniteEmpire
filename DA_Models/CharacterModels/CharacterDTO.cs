using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA_DataAccess.CharacterClasses;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Models.CharacterModels
{
    public class CharacterDTO
    {
        [Key] public int ID { get; set; }

        public string? NPCName { get; set; }
        public string? Class { get; set; }
        public string? Race { get; set; }
        public int Age { get; set; }
        public int UserId { get; set; }

        public ICollection<Attribute>? Attributes { get; set; }
        public ICollection<BaseSkill>? BaseSkills { get; set; }
        public ICollection<SpecialSkill>? SpecialSkills { get; set; }
    }
}
