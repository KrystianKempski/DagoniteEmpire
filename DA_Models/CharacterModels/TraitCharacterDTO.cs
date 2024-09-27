using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class TraitCharacterDTO : TraitDTO
    {
        public TraitCharacterDTO(bool isTemporary = false) { IsTemporary = isTemporary; }
        public TraitCharacterDTO(TraitDTO traitDTO, CharacterDTO characterDTO, bool isTemporary=false) 
        {
            foreach (var prop in traitDTO.GetType().GetProperties())
            {
                if(this.GetType().GetProperty(prop.Name).CanWrite)
                    this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            if(this.Characters is not null)
            {

                var race = this.Characters.FirstOrDefault(r => r.Id == characterDTO.Id);
                if (race is null)
                    this.Characters.Add(characterDTO);
                else
                    race = characterDTO;
            }
            IsTemporary = isTemporary;
        }
        public ICollection<CharacterDTO>? Characters { get; set; }
        public bool IsTemporary { get; set; } = false;
    }
}
