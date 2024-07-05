using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class SpellCircle
    {
        [Key] public int Id { get; set; }
        public int Level { get; set; }
        public int KnownSpells { get; set; }
        public int PerDay { get; set; }
        public IEnumerable<SpellSlot> SpellSlots { get; set; }
        public int ProfessionId { get; set; }
        [ForeignKey(nameof(ProfessionId))]
        public Profession Profession { get; set; }
    }
}
