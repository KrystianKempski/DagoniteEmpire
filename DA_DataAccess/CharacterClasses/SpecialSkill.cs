﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class SpecialSkill : Feature
    {

        public string RelatedAttribute1 { get; set; }
        public string RelatedAttribute2 { get; set; }
        public string? RelatedBaseSkillName { get; set; }
        public string? ChosenAttribute { get; set; }
        public bool Editable { get; set; }

    }
}
