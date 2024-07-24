using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class SpellSlot
    {
        [Key] public int Id { get; set; }

        public bool InSpellbook { get; set; }
        public int Prepared { get; set; }

        //public ICollection<Spell>? Spell { get; set; }

        public int? SpellId { get; set; }
        [ForeignKey(nameof(SpellId))]
        public virtual Spell? Spell { get; set; }

        public int SpellCircleId { get; set; }
        [ForeignKey(nameof(SpellCircleId))]
        public virtual SpellCircle? SpellCircle { get; set; }
    }
}
