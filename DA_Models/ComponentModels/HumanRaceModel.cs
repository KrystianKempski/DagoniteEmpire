using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ComponentModels
{
    public class HumanRaceModel
    {
        public string AttributeNameBonus { get; set; } = string.Empty;
        public string FirstSkillNameBonus { get; set; } = string.Empty;
        public string SecondSkillNameBonus { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = false;
    }
}
