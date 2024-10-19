using DA_Common;
using DA_DataAccess.CharacterClasses;
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
        public TraitCharacterDTO(TraitCharacter trait)
        {
            foreach (var prop in typeof(TraitCharacter).GetProperties())
            {
                if (GetType().GetProperty(prop.Name).CanWrite)
                    GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(trait, null), null);
            }
        }

        public TraitCharacterDTO(TraitDTO traitDTO, int characterId=0, bool isTemporary=false) 
        {
            foreach (var prop in typeof(TraitDTO).GetProperties())
            {
                if(GetType().GetProperty(prop.Name).CanWrite)
                    GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            Id = 0;
            foreach (var bonus in Bonuses)
            {
                bonus.Id = 0;
                bonus.TraitId = Id;
            }
            CharacterId = characterId;
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
        public int CharacterId { get; set; }
        public bool IsTemporary { get; set; } = false;
        public override string TraitLabel { get => IsTemporary ? "state" : "trait"; }
        public override string TraitType { get => IsTemporary ? SD.TraitType_Temporary : SD.TraitType_Character; }
    }
}
