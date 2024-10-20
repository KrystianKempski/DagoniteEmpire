﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public abstract class Trait
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }            //for example "No leg"
        public int Index { get; set; }
        public string TraitType { get; set; } = string.Empty;      // for example "race" or "advantages"
        public int Level { get; set; }              // level for profession trait or defence/attack bonus
        public int TraitValue { get; set; }          // value for advantages and disadvantages or duration time
        public string Descr { get; set; } = "";         // short description of trait 
        public bool TraitApproved { get; set; }     // traits have to be approved by Game Master
        public bool IsRemovable { get; set; } = true;
        public bool IsUnique { get; set; }

        public ICollection<Bonus> Bonuses { get; set; } 

    }
}
