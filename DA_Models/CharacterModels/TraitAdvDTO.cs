using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class TraitAdvDTO : TraitDTO
    {
        public TraitAdvDTO() { }
        public TraitAdvDTO(TraitDTO traitDTO, int id) 
        {
            foreach (var prop in traitDTO.GetType().GetProperties())
            {
                this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            this.CharacterId = id;
        }
        public int? CharacterId { get; set; }
    }
}
