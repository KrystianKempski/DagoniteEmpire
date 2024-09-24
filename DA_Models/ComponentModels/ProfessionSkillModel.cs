using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ComponentModels
{
    public class ProfessionSkillModel
    {
        public TraitProfessionDTO TraitDTO { get; set; } = new TraitProfessionDTO();

        public bool IsVisible { get; set; } = false;
        public bool IsEditMode { get; set; } = false;
        public bool IsInfoMode { get; set; } = false;

    }
}
