using DA_Common;
using DA_Models.ComponentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class ConditionDTO : WoundDTO
    {
        public ConditionDTO(string name) 
        {
            Location = name;
            Description = Location + "condition";
        }
        public ConditionDTO(WoundDTO wound)
        {
            Location = wound.Location;
            Description = wound.Description;
            Value = wound.Value;
            Id = wound.Id;
            CharacterId = wound.CharacterId;
        }
        public override int Penalty { get
            {
                return -Value;
            }
        }
        public override ICollection<string> GetAttributeNamesFromLocation()
        {
            ICollection<string> res = new List<string>();
            if (string.IsNullOrEmpty(Location))
                return res;
            switch (Location)
            {
                case SD.Condition.Nutrition:
                    res.Add(SD.Attributes.Strength);
                    res.Add(SD.Attributes.Endurance);
                    res.Add(SD.Attributes.Dexterity);
                    break;
                case SD.Condition.Cleanliness:
                    res.Add(SD.Attributes.Charisma);
                    break;
                case SD.Condition.Wellbeing:
                    res.Add(SD.Attributes.Intelligence);
                    res.Add(SD.Attributes.Instinct);
                    res.Add(SD.Attributes.Willpower);
                    break;
                case SD.Condition.Rest:
                case SD.Condition.GeneralHealth:
                    foreach(var attr in SD.Attributes.All)
                        res.Add(attr);
                    break;
            }
            return res;
        }
    }
}
