using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class TraitAdv : Trait
    {
        public int? CharacterId { get; set; }

        [ForeignKey(nameof(CharacterId))]
        public Character? Character { get; set; }

    }
}
