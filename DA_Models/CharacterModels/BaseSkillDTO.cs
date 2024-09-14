using DA_Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class BaseSkillDTO :FeatureDTO
    {
        public override string FeatureType { get; set; } = SD.FeatureBaseSkill;
        public string RelatedAttribute1 { get; set; }
        public string RelatedAttribute2 { get; set; }
        public override int SumBonus
        {
            get
            {
                if (_sumBonus == base.SumBonus) return _sumBonus;
                _sumBonus = base.SumBonus;
                return _sumBonus;
            }
        }
    }
}
