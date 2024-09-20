using DA_Common;
using DA_DataAccess.CharacterClasses;
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
        public string Name { get; set; }
        public string Descr { get; set; }

        public string SummaryDescr { get; set; }
        public int Index { get; set; }
        public int TraitValue { get; set; }
        public bool TraitApproved { get; set; }
        public bool IsUnique { get; set; } = false;
        public virtual string TraitType { get; set; } = string.Empty;

        public ICollection<BonusDTO> Bonuses { get; set; }

    }
}
