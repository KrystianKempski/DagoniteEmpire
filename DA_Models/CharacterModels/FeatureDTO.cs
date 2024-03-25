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

        public virtual int SumBonus { get; set; } = 0;

        public virtual void DecrRace() { if (RaceBonus > -6) RaceBonus--; SumAll(); }
        public virtual void IncrRace() { if (RaceBonus < 6) RaceBonus++; SumAll(); }

        public virtual void DecrGear() { if (GearBonus > -6) GearBonus--; SumAll(); }
        public  virtual void IncrGear() { if (GearBonus < 6) GearBonus++;SumAll(); }
        public virtual void ChangeTrait(int val) { TraitBonus+=val; SumAll(); }
        //public virtual void CalcTrait()
        //{
        //    foreach (var bonus in TraitBonusesRelated)
        //    {
        //        if (bonus.FeatureName == Name)
        //            TraitBonus += bonus.BonusValue;
        //    }
        //}
        public virtual void DecrHeal() {  HealthBonus--; SumAll();}
        public virtual void IncrHeal() {  HealthBonus++; SumAll();}

        public virtual void DecrOther() { if (OtherBonuses > -6) OtherBonuses--; SumAll(); }
        public virtual void IncrOther() { if (OtherBonuses < 6) OtherBonuses++;SumAll(); }
        public virtual void DecrTemp() { TempBonuses--;SumAll(); }
        public virtual void IncrTemp() { TempBonuses++; SumAll();}

        public virtual int SumAll()
        {
            SumBonus = BaseBonus + RaceBonus + GearBonus + TraitBonus + TempBonuses + HealthBonus + OtherBonuses;
            return SumBonus;
        }
    }
}
