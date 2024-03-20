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
        public override string FeatureType { get; set; } = "BaseSkill";


        public string RelatedAttribute1 { get; set; }
        public string RelatedAttribute2 { get; set; }


        private int _sumBonus = 0;
        public override int SumBonus
        {
            get => _sumBonus;
            set
            {
                if (_sumBonus == value) return;
                _sumBonus = value;
                OnSumChanged(nameof(SumBonus));
            }
        }

        public event PropertyChangedEventHandler? SumChanged = null;

        protected virtual void OnSumChanged(string propertyName)
        {
            SumChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
