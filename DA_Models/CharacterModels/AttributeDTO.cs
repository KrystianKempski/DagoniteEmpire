using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class AttributeDTO
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        [Required]
        public  string Name { get; set; }
        [Range(6, 18, ErrorMessage = "Attribute base bonus must be between 6 and 18")]
        public int BaseBonus { get; set; } = 6;
        [Range(-6, 6, ErrorMessage = "Attribute race bonus must be between -6 and 6")]
        public int RaceBonus { get; set; } = 0;
        [Range(-6, 6, ErrorMessage = "Attribute gear bonus must be between -6 and 6")]
        public int GearBonus { get; set; } = 0;
        [Range(-6, 6, ErrorMessage = "Attribute other bonus must be between -6 and 6")]
        public int OtherBonuses { get; set; } = 0;
        public int TempBonuses { get; set; } = 0;

        public int HealthBonus { get; set; } = 0;

        public int SumBonus { get; set; } = 0;
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

        public void DecrRace() { if (RaceBonus >- 6) RaceBonus--; SumAll(); }
        public void IncrRace() { if (RaceBonus < 6) RaceBonus++; SumAll(); }

        public void DecrGear() { if (GearBonus > -6) GearBonus--; SumAll(); }
        public void IncrGear() { if (GearBonus < 6) GearBonus++;SumAll(); }
        public void DecrHeal() {  HealthBonus--; SumAll();}
        public void IncrHeal() {  HealthBonus++; SumAll();}

        public void DecrOther() { if (OtherBonuses > -6) OtherBonuses--; SumAll(); }
        public void IncrOther() { if (OtherBonuses < 6) OtherBonuses++;SumAll(); }
        public void DecrTemp() { TempBonuses--;SumAll(); }
        public void IncrTemp() { TempBonuses++; SumAll();}

        public int SumAll()
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
