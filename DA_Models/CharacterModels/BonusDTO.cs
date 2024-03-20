using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class BonusDTO
    {
        public int Id { get; set; }
        public string FeatureType{ get; set; }
        public string? FeatureName { get; set; } = null;
        public int Index { get; set; }
        //public string BonusType { get; set; }
        public int TraitId { get; set; }
        public int? BonusValue { get; set; }
        public string Description { get; set; }
    }
}
