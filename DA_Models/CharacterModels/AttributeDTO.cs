using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class AttributeDTO : FeatureDTO
    {
        public override string FeatureType { get; set; } = "Attribute";

        public override int BaseBonus { get; set; } = 6;
        private int _modifier = 0;
        public int Modifier { 
            get => _modifier;
            set
            {
                if(_modifier == value) return;
                _modifier = value;
                OnModifierChanged(nameof(Modifier));
            }
         }

        public event PropertyChangedEventHandler? ModifierChanged=null;

        protected virtual void OnModifierChanged(string propertyName)
        {
            ModifierChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override int SumAll()
        {
            SumBonus = BaseBonus + RaceBonus + GearBonus + TempBonuses + HealthBonus+OtherBonuses;
            GetModifier();
            return SumBonus;
        }
        public int GetModifier()
        {
            Modifier = (int)Math.Floor((SumBonus - 10) / 2.0);
            return Modifier;
        }
    }
}
