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
        public TraitProfessionDTO(TraitDTO traitDTO, int professionSkillID)
        {
            foreach (var prop in traitDTO.GetType().GetProperties())
            {
                this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);
            }

            ProfessionSkillId = professionSkillID;
        }
        public override string TraitType { get; set; } = SD.TraitType_Profession;
        
        public int ProfessionSkillId { get; set; }
    }
}
