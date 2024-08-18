using DA_DataAccess.CharacterClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class EquipmentSlotDTO
    {
        public int Id { get; set; }
        public int Count { get; set; } = 1;                // how many items in equipment
        public int EquipmentID { get; set; } = 0;
        public Equipment Equipment { get; set; }
        public bool IsEquipped { get; set; }
        public string EquimpmentType { get; set; } = "";
    }
}
