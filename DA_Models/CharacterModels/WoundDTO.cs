using DA_Common;
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
        public int Value { get; set; } = 0;
        public bool IsIgnored { get; set; } = false;
        public bool IsTended { get; set; } = false ;
        public bool IsMagicHealed { get; set; } = false ;
        public int DateMonth { get; set; }
        public int DateDay { get; set; }
        public int HealTime { get; set; }
        public string Severity 
        { 
            get 
            {
                if (Value > 0 && Value < 3)
                    return SD.Wound.Light;
                else if (Value >= 3 && Value < 9)
                    return SD.Wound.Moderate;
                else if (Value >= 9 && Value < 18)
                    return SD.Wound.Heavy;
                else if (Value >= 18 && Value < 25)
                    return SD.Wound.Critical;
                else if (Value >= 25)
                    return SD.Wound.Deadly;
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
    }
}
