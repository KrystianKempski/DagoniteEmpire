using DA_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class TraitProfessionDTO : TraitDTO
    {
        public TraitProfessionDTO() { }
        public override string TraitType { get; set; } = SD.TraitType_Profession;        
        public int ProfessionId { get; set; }
        public int Level { get; set; }
        public int DC { get; set; }
        public int Cost { get; set; }
        public string Range { get; set; } = "";
        public bool IsApproved { get; set; } = false;
        public bool IsActiveSkill { get; set; } = true;
    }
}
