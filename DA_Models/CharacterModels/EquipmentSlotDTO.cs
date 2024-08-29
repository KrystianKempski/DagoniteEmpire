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
        public decimal Weight { get; set; } = decimal.Zero;
        public decimal Price { get; set; } = decimal.Zero;
        public int EquipmentID { get; set; } = 0;
        public EquipmentDTO Equipment { get; set; } = new();
        public bool IsEquipped { get; set; } = false;
        public string SlotType { get; set; } = "other";

        public int CharacterID { get; set; } = 0;
        public CharacterDTO Character { get; set; }

        public EquipmentSlotDTO() { }
        public EquipmentSlotDTO(EquipmentDTO eq) 
        {
            Equipment = eq;
        }

    }
}
