using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class Race
    {
        [Key] public int Id { get; set; }

        public ICollection<TraitRace>? Traits { get; set; }

        public int? CharacterId { get; set; }

        [ForeignKey(nameof(CharacterId))]
        public Character? Character { get; set; }

        public string Name { get; set; }            //for example "dwarf"
        public int Index { get; set; }

        public string Description { get; set; }            // descritpion of race
        public bool RaceApproved { get; set; }     // race have to be approved by Game Master



    }
}
