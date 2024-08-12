using DA_DataAccess.CharacterClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class BattlePropertyDTO : FeatureDTO
    {
        public override string FeatureType { get; set; } = "BattleProperty";

    }
}
