using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ComponentModels
{
    public class EquipmentChosenModel
    {
        public List<EquipmentSlotDTO> EquipmentChosen { get; set; }
        public bool IsVisible { get; set; } = false;
    }
}
