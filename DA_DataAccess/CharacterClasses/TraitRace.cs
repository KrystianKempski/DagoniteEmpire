using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class TraitRace: Trait
    {
        public int? RaceId { get; set; }
        [ForeignKey(nameof(RaceId))]
        public Race? Race { get; set; }
    }
}
