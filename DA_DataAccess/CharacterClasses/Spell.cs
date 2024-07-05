using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class Spell
    {
        [Key] public int Id { get; set; }
        public bool IsApproved { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public IEnumerable<SpellSlot>? SpellSlots { get; set; }

    }
}
