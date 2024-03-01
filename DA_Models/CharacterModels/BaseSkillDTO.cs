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
    public class BaseSkillDTO
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        [Required]
        public string Name { get; set; } = "";

        [Range(0, 5, ErrorMessage = "Base skill base bonus must be between 0 and 7")]
        public int BaseBonus { get; set; } = 0;
        [Range(0, 5, ErrorMessage = "Base skill race bonus must be between 0 and 7")]
        public int RaceBonus { get; set; } = 0;
        [Range(0, 5, ErrorMessage = "Base skill gear bonus must be between 0 and 7")]
        public int GearBonus { get; set; } = 0;
        [Range(0, 5, ErrorMessage = "Base skill other bonus must be between 0 and 7")]
        public int OtherBonuses = 0;

        public int TempBonuses = 0;

        public string RelatedAttribute1 { get; set; }
        public string RelatedAttribute2 { get; set; }


        private int _sumBonus = 0;
        public int SumBonus
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

        public void DecrRace() { if (RaceBonus > -6) RaceBonus--; SumAll(); }
        public void IncrRace() { if (RaceBonus < 6) RaceBonus++; SumAll(); }

        public void DecrGear() { if (GearBonus > -6) GearBonus--; SumAll(); }
        public void IncrGear() { if (GearBonus < 6) GearBonus++; SumAll(); }

        public void DecrOther() { if (OtherBonuses > -6) OtherBonuses--; SumAll(); }
        public void IncrOther() { if (OtherBonuses < 6) OtherBonuses++; SumAll(); }
        public void DecrTemp() { TempBonuses--; SumAll(); }
        public void IncrTemp() { TempBonuses++; SumAll(); }

        public int SumAll()
        {
            SumBonus = BaseBonus + RaceBonus + GearBonus + TempBonuses + OtherBonuses;
            return SumBonus;
        }
    }
}
