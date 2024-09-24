using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class ProfessionSkill
    {
        [Key]
        public int Id { get; set; }

        public int Level { get; set; }
        public int? DC { get; set; }
        public int? Cost { get; set; }
        public string? Range { get; set; }
        public bool IsApproved { get; set; } = false;
        public uint Index { get; set; } = 0;
        public int? ActiveProfessionId { get; set; } =null;
        [ForeignKey(nameof(ActiveProfessionId)), Column(Order = 0)]
        public virtual Profession? ActiveProfession { get; set; }
        public int? PassiveProfessionId { get; set; } = null;
        [ForeignKey(nameof(PassiveProfessionId)), Column(Order = 1)]
        public virtual Profession? PassiveProfession { get; set; }
    }
}
