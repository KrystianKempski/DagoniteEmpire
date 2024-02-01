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
        [Key] public int Id { get; set; }
        [Required]
        public  string Name { get; set; }

        public int BaseBonus { get; set; } = 0;
        public int RaceBonus { get; set; } = 0;
        public int GearBonus { get; set; } = 0;
        public Dictionary<string, int> OtherBonuses = new Dictionary<string, int>();
        public int HealthBonus { get; set; } = 0;
        [Required]
        public int CharacterId { get; set; }
    }
}
