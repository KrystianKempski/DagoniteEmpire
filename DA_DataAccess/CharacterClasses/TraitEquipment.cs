﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{

    public class TraitEquipment : Trait
    {
        public ICollection<Equipment>? Equipment { get; set; }

    }
}
