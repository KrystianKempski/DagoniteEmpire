﻿using DA_DataAccess.CharacterClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{

        public class BaseSkill : Skill
        {
            [Key] 
            public int Id { get; set; }
            public string? Type { get; set; }
            public int CharacterId { get; set; }

            [ForeignKey(nameof(CharacterId))]
            public Character Character { get; set; }

            //public ICollection<SpecialSkill>? SpecialSkills { get; set; }
    }

        public class Skill
        {
            public string? Name { get; set; }
            public int BaseBonus { get; set; } = 0;
            public int RaceBonus { get; set; } = 0;
            public int GearBonus { get; set; } = 0;
            public Dictionary<string, int> OtherBonuses = new();

            public Dictionary<string, int> TempBonuses = new();
            

        }  
}

