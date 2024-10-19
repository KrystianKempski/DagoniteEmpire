using DA_Common;
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
        public string Description { get; set; } = string.Empty;
        public string RelatedAttributeName { get; set; } = string.Empty;
        public int ClassLevel { get; set; } = 1;
        public int CurrentFocusPoints { get; set; } = 0;
        public bool IsApproved { get; set; } = false;

        public bool IsUniversal { get; set; } = false;
        public SpellcasterType CasterType { get; set; } = SpellcasterType.None;

        public ICollection<Character>? Characters { get; set; }
        public virtual ICollection<SpellCircle>? SpellCircles { get; set; }

    }
}
