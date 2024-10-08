using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public abstract class FeatureDTO
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }

        public virtual string FeatureType { get; set; } = string.Empty;

        public int Index { get; set; }
        [Required]
        public  string Name { get; set; } = string.Empty;
        public virtual int BaseBonus { get; set; } = 0;
        public int RaceBonus { get; set; } = 0;
        public int GearBonus { get; set; } = 0;
        public int TraitBonus { get; set; } = 0;
        public int OtherBonuses { get; set; } = 0;
        public int TempBonuses { get; set; } = 0;

        public int HealthBonus { get; set; } = 0;

        protected int _sumBonus;
        public virtual int SumBonus { 
            get => _sumBonus = BaseBonus + RaceBonus + GearBonus + TraitBonus + TempBonuses + HealthBonus + OtherBonuses;
        }
        private int _sumAbsolute;
        public virtual int SumAbsolute
        {
            get => _sumAbsolute = BaseBonus + RaceBonus + GearBonus + TraitBonus + OtherBonuses;
        }

        public virtual void DecrRace() { if (RaceBonus > -6) RaceBonus--;  }
        public virtual void IncrRace() { if (RaceBonus < 6) RaceBonus++;  }

        public virtual void DecrGear() { if (GearBonus > -6) GearBonus--;  }
        public  virtual void IncrGear() { if (GearBonus < 6) GearBonus++; }
        public virtual void ChangeTrait(int val) { TraitBonus+=val; }
        public virtual void DecrHeal() {  HealthBonus--; }
        public virtual void IncrHeal() {  HealthBonus++; }

        public virtual void DecrOther() { if (OtherBonuses > -6) OtherBonuses--;  }
        public virtual void IncrOther() { if (OtherBonuses < 6) OtherBonuses++; }
        public virtual void DecrTemp() { TempBonuses--; }
        public virtual void IncrTemp() { TempBonuses++; }

    }
}
