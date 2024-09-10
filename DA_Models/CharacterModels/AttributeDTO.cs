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
    public class AttributeDTO : FeatureDTO
    {
        public override string FeatureType { get; set; } = "Attribute";

        public override int BaseBonus { get; set; } = 6;
        private int _modifier = 0;
        public int Modifier
        {
            get
            {
                int mod = (int)Math.Floor((SumBonus - 10) / 2.0);
                if (_modifier == mod) return _modifier;
                _modifier = mod;
                //OnModifierChanged(nameof(Modifier));
                return mod;
            }
        }

        private int _modifierAbsolute = 0;
        public int ModifierAbsolute
        {
            get
            {
                int mod = (int)Math.Floor((SumAbsolute - 10) / 2.0);
                if (_modifierAbsolute == mod) return _modifierAbsolute;
                _modifierAbsolute = mod;
                // OnModifierChanged(nameof(Modifier));
                return mod;
            }
        }

        //public event PropertyChangedEventHandler? ModifierChanged = null;

        //protected virtual void OnModifierChanged(string propertyName)
        //{
        //    ModifierChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}
