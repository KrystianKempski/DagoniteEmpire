using DA_Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class EquipmentSlot
    {
        [Key] public int Id { get; set; }
        public int Count { get; set; } = 1;                // how many items in equipment
        public int EquipmentID { get; set; } = 0;
        [ForeignKey(nameof(EquipmentID))]
        public Equipment Equipment { get; set; } = new();
        public bool IsEquipped { get; set; } = false;

        public string SlotType { get; set; } = "other";

        public int CharacterID { get; set; } = 0;
        [ForeignKey(nameof(CharacterID))]
        public Character Character { get; set; }
    }
}
