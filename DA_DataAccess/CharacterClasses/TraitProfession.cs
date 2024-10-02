using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class TraitProfession : Trait
    {
        public int ProfessionId { get; set; }
        public int Level { get; set; }
        public int? DC { get; set; }
        public int? Cost { get; set; }
        public string? Range { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsActiveSkill { get; set; } = false;
    }
}
