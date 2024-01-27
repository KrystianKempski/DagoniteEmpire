using DA_DataAccess.CharacterClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        }

        public class Skill
        {
            public string? Name { get; set; }
            public int BaseBonus { get; set; } = 0;
            public int RaceBonus { get; set; } = 0;
            public int GearBonus { get; set; } = 0;
            public Dictionary<string, int> OtherBonuses = new();

            public Dictionary<string, int> TempBonuses = new();

        public int SumOfSkill()
            {
                return BaseBonus + RaceBonus + GearBonus + OtherBonuses.Values.AsEnumerable().Sum() + TempBonuses.Values.AsEnumerable().Sum();
            }
        }  
}


//public required Skill Melee;
//public required Skill Projectiles;
//public required Skill Acrobatics;
//public required Skill SleightOfHands;
//public required Skill Athletics;
//public required Skill Talk;
//public required Skill Deceit;
//public required Skill Knowledge;
//public required Skill Craft;
//public required Skill Survival;
//public required Skill AnimalHandle;
//public required Skill Medicine;
//public required Skill SpecialSkill;