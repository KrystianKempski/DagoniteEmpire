using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class SpecialSkillDTO
    {
        public int Id { get; set; }

        //[Required]
        //public int BaseSkillId { get; set; }

        //public Attribute RelatedAttribute { get; set; }
        [Required]
        public string Name { get; set; }
        public int BaseBonus { get; set; } = 0;
        public int RaceBonus { get; set; } = 0;
        public int GearBonus { get; set; } = 0;
        public Dictionary<string, int> OtherBonuses = new();

        public Dictionary<string, int> TempBonuses = new();

    }
}
