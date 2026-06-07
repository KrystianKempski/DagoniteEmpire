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
        public TraitCharacterDTO(bool isTemporary = false)
        {
            IsTemporary = isTemporary;
            if (IsTemporary) TraitValue = 1;
        }
        public TraitCharacterDTO(TraitCharacter trait)
        {
            foreach (var prop in typeof(TraitCharacter).GetProperties())
            {
                if (prop.Name == nameof(Bonuses))
                    continue;

                var targetProp = GetType().GetProperty(prop.Name);
                if (targetProp?.CanWrite == true)
                    targetProp.SetValue(this, prop.GetValue(trait, null), null);
            }

            foreach (var bonus in trait.Bonuses)
            {
                Bonuses.Add(new BonusDTO
                {
                    Id = bonus.Id,
                    FeatureType = bonus.FeatureType,
                    FeatureName = bonus.FeatureName,
                    BonusValue = bonus.BonusValue ?? 0,
                    Description = bonus.Description,
                    Index = bonus.Index,
                    TraitId = bonus.TraitId,
                });
            }
        }

        public TraitCharacterDTO(TraitDTO traitDTO, int characterId=0, bool isTemporary=false) 
        {
            foreach (var prop in typeof(TraitDTO).GetProperties())
            {
                var targetProp = GetType().GetProperty(prop.Name);
                if (targetProp?.CanWrite == true)
                    targetProp.SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            Id = 0;
            foreach (var bonus in Bonuses)
            {
                bonus.Id = 0;
                bonus.TraitId = Id;
            }
            TraitApproved = false;
            CharacterId = characterId;
            IsTemporary = isTemporary;
            
            TraitType = isTemporary ? SD.TraitType_Temporary : SD.TraitType_Character;
        }
        public TraitCharacterDTO(TraitDTO traitDTO, bool isTemporary = false)
        {
            
            foreach (var prop in typeof(TraitDTO).GetProperties())
            {
                var targetProp = GetType().GetProperty(prop.Name);
                if (targetProp?.CanWrite == true)
                    targetProp.SetValue(this, prop.GetValue(traitDTO, null), null);   
            }
            Id = 0;
            foreach (var bonus in Bonuses)
            {
                bonus.Id = 0;
                bonus.TraitId = Id;
            }
            TraitApproved = false;
            IsTemporary = isTemporary;
            TraitType = isTemporary ? SD.TraitType_Temporary : SD.TraitType_Character;
        }
        public int CharacterId { get; set; }
        public bool IsTemporary { get; set; } = false;
        public override string TraitLabel { get => IsTemporary ? "state" : "trait"; }
        public override string TraitType { get => IsTemporary ? SD.TraitType_Temporary : SD.TraitType_Character; }
    }
}
