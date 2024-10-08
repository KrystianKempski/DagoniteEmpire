using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class BonusDTO
    {
        public BonusDTO() { }
        public BonusDTO(BonusDTO cpy,int traitId=0)
        {
            Id = 0;
            FeatureType = cpy.FeatureType;
            FeatureName = cpy.FeatureName;
            Index = cpy.Index;
            TraitId = traitId;
            BonusValue = cpy.BonusValue;
            Description = cpy.Description;
        }
        public int Id { get; set; } = 0;
        public string FeatureType{ get; set; }
        public string? FeatureName { get; set; } = null;
        public int Index { get; set; } = 0;
        public int TraitId { get; set; } = 0;
        public int BonusValue { get; set; } = 0;
        public string Description { get; set; } = string.Empty;

    }
}
