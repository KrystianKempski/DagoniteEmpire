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
        //public int DayOfInjury { get; set; } = 1;
        private int DateMonth { get; set; } = 1;
        private int DateDay { get; set; } = 1;
        private int DateYear { get; set; } = 1;
        public int CharacterId { get; set; }
        public bool IsCondition { get; set; } = false;

        public int HealTime { get => Value != 0 ? (int)((0.4 * Value) + 3.5) : 0; }
        public DateModel DateStart
        {
            get
            {
                return new DateModel(DateDay, DateMonth, DateYear);
            }
            set
            {
                DateMonth = value.Month;
                DateYear = value.Year;
                DateDay = value.Day;
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
        public virtual int Penalty { get
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


       
        public virtual ICollection<string> GetAttributeNamesFromLocation()
        {
            if (string.IsNullOrEmpty(Location))
                return new List<string>();
            ICollection<string>? res = GetAttributeNamesFromLocation(Location);
            if (res == null)
                return new List<string>();
            return res;
        }
        public static ICollection<string>? GetAttributeNamesFromLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                return null;
            ICollection<string> res = new List<string>();
            int i = 0;
            switch (location)
            {
                case SD.WoundLocation.Head: i = (int)SD.WoundLocationEnum.Head; break;
                case SD.WoundLocation.Neck: i = (int)SD.WoundLocationEnum.Neck; break;
                case SD.WoundLocation.MainHand: i = (int)SD.WoundLocationEnum.MainHand; break;
                case SD.WoundLocation.OffHand: i = (int)SD.WoundLocationEnum.OffHand; break;
                case SD.WoundLocation.MainArm: i = (int)SD.WoundLocationEnum.MainArm; break;
                case SD.WoundLocation.OffArm: i = (int)SD.WoundLocationEnum.OffArm; break;
                case SD.WoundLocation.Body: i = (int)SD.WoundLocationEnum.Body; break;
                case SD.WoundLocation.Back: i = (int)SD.WoundLocationEnum.Back; break;
                case SD.WoundLocation.LeftLeg: i = (int)SD.WoundLocationEnum.LeftLeg; break;
                case SD.WoundLocation.RightLeg: i = (int)SD.WoundLocationEnum.RightLeg; break;
                case SD.WoundLocation.Face: i = (int)SD.WoundLocationEnum.Face; break;
            }

            res.Add(SD.WoundAttributes[i, 0]);
            res.Add(SD.WoundAttributes[i, 1]);
            return res;
        }
    }
}
