using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class Profession
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }    
        public string Description { get; set; }
        public string RelatedAttribute { get; set; }
        public int ClassLevel { get; set; }
        public int MaxFocusPoints { get; set; }
        public int CurrentCofusPoints { get; set; }
        public bool IsApproved { get; set; }

        public bool IsUniversal { get; set; }

        public ICollection<Character>? Characters { get; set; }

        [InverseProperty("ActiveProfession")]
        public virtual ICollection<ProfessionSkill>? ActiveSkills { get; set; }

        [InverseProperty("PassiveProfession")]
        public virtual ICollection<ProfessionSkill>? PassiveSkills { get; set; }

    }
}
