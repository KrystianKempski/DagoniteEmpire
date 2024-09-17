using DA_Common;
using DA_Models.ComponentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class WoundDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Value { get; set; }
        public bool IsIgnored { get; set; } = false;
        public bool IsTended { get; set; } = false;
        public bool IsMagicHealed { get; set; } = false;
        public int DayOfInjury { get; set; }
        public int DateMonth { get; set; } = 1;
        public int DateDay { get; set; } = 1;
        public int DateYear { get; set; } = SD.Calendar.StartYear;

        public int HealTime { get => Value != 0 ? (int)((0.4 * Value) + 3.5) : 0; }
        public DateModel DateStart
        {
            get
            {
                return new DateModel(DateDay, DateMonth, DateYear);
            }
        }
        public DateModel DateReduce { get; set; } = new(1,1);
        public string Severity
        {
            get
            {
                if (Value > 0 && Value < 3)
                    return SD.WoundSeverity.Light;
                else if (Value >= 3 && Value < 9)
                    return SD.WoundSeverity.Moderate;
                else if (Value >= 9 && Value < 18)
                    return SD.WoundSeverity.Heavy;
                else if (Value >= 18 && Value < 25)
                    return SD.WoundSeverity.Critical;
                else if (Value >= 25)
                    return SD.WoundSeverity.Deadly;
                else
                    return "";
            }
        }
        public List<AttributeDTO> RelatedAttributes { get; set; } = new List<AttributeDTO> { };
        public int Penalty { get
            {
                if (Value > 0 && Value < 3)
                    return IsIgnored || IsIgnored ? 0 : 1;
                else if (Value >= 3 && Value < 9)
                    return IsIgnored || IsIgnored ? 1 : 3;
                else if (Value >= 9 && Value < 18)
                    return IsIgnored || IsIgnored ? 3 : 7;
                else if (Value >= 18 && Value < 25)
                    return IsIgnored || IsIgnored ? 5 : 12;
                else if (Value >= 25)
                    return IsIgnored || IsIgnored ? 20 : 20;
                else
                    return 0;
            }
        }

    public int CharacterId { get; set; }

        public int GetValueFromSeverity(string severity)
        {
            switch (severity)
            {
                case SD.WoundSeverity.Light: return 1;
                case SD.WoundSeverity.Moderate: return 3;
                case SD.WoundSeverity.Heavy: return 9;
                case SD.WoundSeverity.Critical: return 18;
                case SD.WoundSeverity.Deadly: return 25;
                default: return 0;
            }
        }
    }
}
