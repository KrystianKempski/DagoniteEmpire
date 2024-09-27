using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class TraitCharacter : Trait
    {
        public ICollection<Character>? Characters { get; set; }
        public bool IsTemporary { get; set; } = false;

    }
}
