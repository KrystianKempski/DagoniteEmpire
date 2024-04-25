using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class RaceDTO
    {
        public int Id { get; set; }
        public ICollection<CharacterDTO>? Characters { get; set; } = new List<CharacterDTO>();

        public ICollection<TraitRaceDTO>? Traits { get; set; } = new List<TraitRaceDTO>();
        public string Name { get; set; } = string.Empty;         //for example "dwarf"
        public int Index { get; set; } = 0;

        public string Description { get; set; } = string.Empty;          // descritpion of race
        public bool RaceApproved { get; set; } = false;   // race have to be approved by Game Master

    }
}
