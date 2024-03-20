using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class Trait
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }            //for example "No leg"
        public int Index { get; set; }

        public string TraitType { get; set; }        // for example "race" or "advantages"
        public string Descr { get; set; }            // descritpion of all trait 

        public int CharacterId { get; set; }

        [ForeignKey(nameof(CharacterId))]
        public Character Character { get; set; }


    }
}
