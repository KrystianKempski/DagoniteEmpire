using Abp.Collections.Extensions;
using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_Models.ComponentModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class TraitDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";    
        public string Descr { get; set; } = "";

        public string SummaryDescr
        {
            get
            {
                string res = "";
                
                if(Descr.IsNullOrEmpty() == false)
                {
                    res = Descr + ". ";
                }
                if (Bonuses is null || Bonuses.Any() == false) return res;
                foreach (var bonus in Bonuses)
                {
                    if (bonus.Description != null && bonus.Description.Length > 0)
                    {
                        res += bonus.Description + ", ";
                    }
                    else
                    {
                        string val;
                        if (bonus.BonusValue > 0)
                            val = $"+{bonus.BonusValue}";
                        else
                            val = $"{bonus.BonusValue}";
                        res += $"{val} to {bonus.FeatureName}; ";
                    }
                }
                return res;
            }
        }
        public int Index { get; set; }
        public int TraitValue { get; set; }
        public bool TraitApproved { get; set; }
        public bool IsRemovable { get; set; } = true;
        public bool IsUnique { get; set; } = false;
        public int Level { get; set; }
        public virtual string TraitType { get; set; } = string.Empty;
        public virtual string TraitLabel { get =>"trait";} 
        public ICollection<BonusDTO> Bonuses { get; set; } = new List<BonusDTO>();

    }
}
