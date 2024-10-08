using DA_Common;
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
        public TraitCharacterDTO(TraitDTO traitDTO, CharacterDTO? characterDTO, bool isTemporary=false) 
        {
            foreach (var prop in typeof(TraitDTO).GetProperties())
            {
                if(GetType().GetProperty(prop.Name).CanWrite)
                    GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            if(Characters is not null && characterDTO is not null)
            {

                var charac = Characters.FirstOrDefault(r => r.Id == characterDTO.Id);
                if (charac is null)
                    Characters.Add(characterDTO);
                else
                    charac = characterDTO;
            }
            IsTemporary = isTemporary;
            TraitType = isTemporary ? SD.TraitType_Temporary : SD.TraitType_Character;
        }
        public TraitCharacterDTO(TraitDTO traitDTO, bool isTemporary = false)
        {
            
            foreach (var prop in typeof(TraitDTO).GetProperties())
            {
                if (GetType().GetProperty(prop.Name).CanWrite)
                    GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);   
            }
            Id = 0;
            foreach (var bonus in Bonuses)
            {
                bonus.Id = 0;
                bonus.TraitId = Id;
            }            
            IsTemporary = isTemporary;
            TraitType = isTemporary ? SD.TraitType_Temporary : SD.TraitType_Character;
        }
        public ICollection<CharacterDTO>? Characters { get; set; }
        public bool IsTemporary { get; set; } = false;
        public override string TraitLabel { get => IsTemporary ? "state" : "trait"; }
        public override string TraitType { get => IsTemporary ? SD.TraitType_Temporary : SD.TraitType_Character; }
    }
}
