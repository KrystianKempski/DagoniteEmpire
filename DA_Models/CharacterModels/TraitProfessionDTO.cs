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
                var targetProp = GetType().GetProperty(prop.Name);
                if (targetProp?.CanWrite == true)
                    targetProp.SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            ProfessionId = profId;
            IsActiveSkill = isActiveSkill;
        }
        public override string TraitType { get; set; } = SD.TraitType_Profession;        
        public int ProfessionId { get; set; }
        public int DC { get; set; }
        public int Cost { get; set; }
        public string Range { get; set; } = "";
        public bool IsActiveSkill { get; set; } = true;
        public bool IsInUse { get; set; } = false;
        /// <summary>Active uses of this skill in the current session (UI state).</summary>
        public int UseCount { get; set; } = 0;
        public override string TraitLabel { get => "skill"; }
    }
}
