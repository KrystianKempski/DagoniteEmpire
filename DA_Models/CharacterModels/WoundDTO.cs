using DA_Common;
using DA_DataAccess.CharacterClasses;
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

        public int DateNumber { get; set; } = 0;
        ////public int DayOfInjury { get; set; } = 1;
        //private int DateMonth { get; set; } = 1;
        //private int DateDay { get; set; } = 1;
        //private int DateYear { get; set; } = 1;
        public int CharacterId { get; set; }
        public bool IsCondition { get; set; } = false;

        public int HealTime { get => Value != 0 ? (int)((0.4 * Value) + 3.5) : 0; }
        public DateModel DateStart
        {
            get
            {
                return new DateModel(DateNumber);
            }
            set
            {
                DateNumber = value.AllDays;
            }
        }
        public DateModel DateReduce { get; set; } = new(1,1);
        public string Severity
        {
            get
            {
                if (Value > 0 && Value < 3)
                    return Wounds.Severity.Light;
                else if (Value >= 3 && Value < 9)
                    return Wounds.Severity.Moderate;
                else if (Value >= 9 && Value < 18)
                    return Wounds.Severity.Heavy;
                else if (Value >= 18 && Value < 25)
                    return Wounds.Severity.Critical;
                else if (Value >= 25)
                    return Wounds.Severity.Deadly;
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
                case Wounds.Location.Head: i = (int)Wounds.LocationEnum.Head; break;
                case Wounds.Location.Neck: i = (int)Wounds.LocationEnum.Neck; break;
                case Wounds.Location.MainHand: i = (int)Wounds.LocationEnum.MainHand; break;
                case Wounds.Location.OffHand: i = (int)Wounds.LocationEnum.OffHand; break;
                case Wounds.Location.MainArm: i = (int)Wounds.LocationEnum.MainArm; break;
                case Wounds.Location.OffArm: i = (int)Wounds.LocationEnum.OffArm; break;
                default:
                case Wounds.Location.Body: i = (int)Wounds.LocationEnum.Body; break;
                case Wounds.Location.Back: i = (int)Wounds.LocationEnum.Back; break;
                case Wounds.Location.LeftLeg: i = (int)Wounds.LocationEnum.LeftLeg; break;
                case Wounds.Location.RightLeg: i = (int)Wounds.LocationEnum.RightLeg; break;
                case Wounds.Location.Face: i = (int)Wounds.LocationEnum.Face; break;
            }

            res.Add(Wounds.Attributes[i, 0]);
            res.Add(Wounds.Attributes[i, 1]);
            return res;
        }
    }
}
