using DA_DataAccess.CharacterClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class EquipmentDTO
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; } = string.Empty;                    //for example "dwarf"
        public int Index { get; set; } = 0;
        public string Description { get; set; } = string.Empty;             // descritpion of item
        public string ShortDescr {  get; set; } = string.Empty;             // short item descr
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public int Count { get; set; } = 1;                                 // how many items in equipment
        public bool IsApproved { get; set; }                                // Equipment have to be approved by Game Master
        public string EquipmentType { get; set; } = "other";
        public ICollection<TraitEquipmentDTO>? Traits { get; set; } = new List<TraitEquipmentDTO>();
        public ICollection<CharacterDTO>? Characters { get; set; } = new List<CharacterDTO>();
    }
}
