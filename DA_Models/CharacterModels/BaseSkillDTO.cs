using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class BaseSkillDTO
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        [Required]
        public string Name { get; set; } = "";

        [Range(0, 5, ErrorMessage = "Base bonus must be between 0 and 5")]
        public int BaseBonus { get; set; } = 0;
        [Range(0, 2, ErrorMessage = "Race bonus must be between 0 and 5")]
        public int RaceBonus { get; set; } = 0;
        [Range(0, 5, ErrorMessage = "Gear bonus must be between 0 and 5")]
        public int GearBonus { get; set; } = 0;
        [Range(0, 5, ErrorMessage = "Other bonus must be between 0 and 5")]
        public int OtherBonuses = 0;

        public int TempBonuses = 0;
    }
}
