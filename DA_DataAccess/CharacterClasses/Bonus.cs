using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class Bonus
    {
        [Key]
        public int Id { get; set; }

        public string FeatureType { get; set; }  // for example Attribute
        public string? FeatureName { get; set; } // for example Dexterity
        public int? BonusValue { get; set; }
        public string? Description { get; set; }  // for examlpe "lets you see in darkness", only when Feature type is "other"
        public int Index { get; set; }

        [ForeignKey(nameof(Trait))]
        public int TraitId { get; set; }

        public virtual Trait Trait { get; set; }

    }
}
