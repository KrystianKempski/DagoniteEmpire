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
        public TraitProfessionDTO(bool isActiveSkill = true) { IsActiveSkill = isActiveSkill; }
        public TraitProfessionDTO(TraitDTO traitDTO, bool isActiveSkill = false,int profId = 0) 
        {
            foreach (var prop in traitDTO.GetType().GetProperties())
            {
                if (GetType().GetProperty(prop.Name).CanWrite)
                    GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            ProfessionId = profId;
            IsActiveSkill = isActiveSkill;
        }
        public override string TraitType { get; set; } = SD.TraitType_Profession;        
        public int ProfessionId { get; set; }
        public int Level { get; set; }
        public int DC { get; set; }
        public int Cost { get; set; }
        public string Range { get; set; } = "";
        //public bool IsApproved { get; set; } = false;
        public bool IsActiveSkill { get; set; } = true;
        public override string TraitLabel { get => "skill"; }
    }
}
