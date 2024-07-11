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

        [ForeignKey(nameof(SpellId))]
        public int SpellId { get; set; }
        public Spell Spell { get; set; } = new();

        [ForeignKey(nameof(SpellCircle))]
        public int SpellCircleId { get; set; }
        public virtual SpellCircle? SpellCircle { get; set; }
    }
}
