using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class AttributeDTO
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        [Required]
        public  string Name { get; set; }
        [Range(6, 18, ErrorMessage = "Base bonus must be between 6 and 18")]
        public int BaseBonus { get; set; } = 0;
        [Range(-6, 6, ErrorMessage = "Race bonus must be between -6 and 6")]
        public int RaceBonus { get; set; } = 0;
        [Range(-6, 6, ErrorMessage = "Gear bonus must be between -6 and 6")]
        public int GearBonus { get; set; } = 0;
        [Range(-6, 6, ErrorMessage = "Other bonus must be between -6 and 6")]
        public int OtherBonuses { get; set; } = 0;
        public int TempBonuses { get; set; } = 0;

        public int HealthBonus { get; set; } = 0;
    }
}
