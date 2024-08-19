using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ComponentModels
{
   
    public class CustomEquipmentModel
    {
        public EquipmentSlotDTO EquipmentSlotDTO { get; set; } = new EquipmentSlotDTO();

        public ICollection<TraitDTO>? Traits { get; set; } = new List<TraitDTO>();
        public bool IsVisible { get; set; } = false;
        public bool IsEditMode { get; set; } = false;
    }
}
