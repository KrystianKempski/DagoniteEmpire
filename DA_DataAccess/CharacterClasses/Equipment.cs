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
    public class Equipment
    {
        [Key] public int Id { get; set; }

        public ICollection<TraitEquipment>? Traits { get; set; } = null;

        public ICollection<Character>? Characters { get; set; } = null;

        public string Name { get; set; }                        //for example "axe"
        public int Index { get; set; }

        public string Description { get; set; }                 // descritpion of Equipment

        public string ShortDescr { get; set; }                  // short item descr
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public int Count { get; set; }      = 1;                // how many items in equipment
        public bool IsApproved { get; set; }                    // Equipment have to be approved by Game Master

        public bool IsEquipped { get; set; } = false;
        public string EquipmentType { get; set; } = "other";

        //public ICollection<WeaponQuality>? WeaponQualities { get; set; } = null;

    }
}
