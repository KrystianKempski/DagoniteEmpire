using DA_Models.CharacterModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models
{
    public class CharacterSeeder
    {
        static public IEnumerable<AttributeDTO> GetAttributes()
        {
            var attributes = new List<AttributeDTO>()
            {
                new AttributeDTO()
                {
                    Name = "Strength",
                },
                new AttributeDTO()
                {
                    Name = "Dexterity",
                },
                new AttributeDTO()
                {
                    Name = "Endurance",
                    TempBonuses = 0,
                },
                new AttributeDTO()
                {
                    Name = "Intelligence",
                },
                new AttributeDTO()
                {
                    Name = "Instinct",
                },
                new AttributeDTO()
                {
                    Name = "Willpower",
                },
                new AttributeDTO()
                {
                    Name = "Charisma",
                },
            };
            return attributes;
        }

        static public IEnumerable<BaseSkillDTO> GetBaseSkills()
        {
            var baseSkills = new List<BaseSkillDTO>()
            {
                new BaseSkillDTO()
                {
                    Name = "Melee",
                    RelatedAttribute1 = "Strength",
                    RelatedAttribute2 = "Dexterity"
                },
                new BaseSkillDTO()
                {
                    Name = "Shooting",
                    RelatedAttribute1 = "Instinct",
                    RelatedAttribute2 = "Dexterity"
                },
                new BaseSkillDTO()
                {
                    Name = "Acrobatics",
                    RelatedAttribute1 = "Endurance", 
                    RelatedAttribute2 = "Dexterity"
                },
                new BaseSkillDTO()
                {
                    Name = "Sleight of hands",
                    RelatedAttribute1 = "Instinct", 
                    RelatedAttribute2 = "Dexterity"
                },
                new BaseSkillDTO()
                {
                    Name = "Athletics",
                    RelatedAttribute1 = "Endurance",
                    RelatedAttribute2 = "Strength"
                },
                new BaseSkillDTO()
                {
                    Name = "Talk",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Instinct"
                },
                new BaseSkillDTO()
                {
                    Name = "Deceit",
                    RelatedAttribute1 = "Instinct",
                    RelatedAttribute2 = "Charisma"
                },
                 new BaseSkillDTO()
                {
                    Name = "Perception",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Instinct"
                },
                new BaseSkillDTO()
                {
                    Name = "Knowledge",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Willpower"
                },
                new BaseSkillDTO()
                {
                    Name = "Craft",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Endurance"
                },
                new BaseSkillDTO()
                {
                    Name = "Survival",
                     RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Dexterity"
                },
                new BaseSkillDTO()
                {
                    Name = "Animal handle",
                     RelatedAttribute1 = "Instinct",
                    RelatedAttribute2 = "Willpower"
                },
                new BaseSkillDTO()
                {
                    Name = "Medicine",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Willpower"
                },
                
            };
            return baseSkills;
        }
        static public IEnumerable<SpecialSkillDTO> GetSpecialSkills()
        {
            var SpecialSkills = new List<SpecialSkillDTO>()
            {
                // MELEE
                new SpecialSkillDTO()
                {
                    Name="Heavy weapons",
                    ChosenAttribute = "Strength",
                    RelatedBaseSkillName = "Melee"
                },
                new SpecialSkillDTO()
                {
                    Name="Swords and sabres",
                    RelatedAttribute1="Strength",
                    RelatedAttribute2="Dexterity", 
                    RelatedBaseSkillName = "Melee"
                },
                new SpecialSkillDTO()
                {
                    Name="Fencing weapons",
                    RelatedAttribute1="Strength",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Melee"
                },
                new SpecialSkillDTO()
                {
                    Name="Light weapons",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Melee"
                },
                new SpecialSkillDTO()
                {
                    Name="Shields",
                    RelatedAttribute1="Strength",
                    RelatedAttribute2 = "Endurance", 
                    RelatedBaseSkillName = "Melee"
                },
                new SpecialSkillDTO()
                {
                    Name="Polearms",
                    RelatedAttribute1="Strength",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Melee"
                },
                new SpecialSkillDTO()
                {
                    Name="Exotic weapons",
                    RelatedBaseSkillName = "Melee",
                    Editable = true,
                },

                // SHOOTING
                new SpecialSkillDTO()
                {
                    Name="Bows",
                    RelatedAttribute1= "Strength",
                    RelatedAttribute2 =  "Dexterity",
                    RelatedBaseSkillName = "Shooting"
                },

                new SpecialSkillDTO()
                {
                    Name="Crossbows",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Shooting"
                },
                new SpecialSkillDTO()
                {
                    Name="Throwing weapons",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 ="Strength" ,
                    RelatedBaseSkillName = "Shooting"
                },
                new SpecialSkillDTO()
                {
                    Name="Slingshots",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Strength" ,
                    RelatedBaseSkillName = "Shooting"
                },
                new SpecialSkillDTO()
                {
                    Name="Javelins",
                    ChosenAttribute = "Strength",
                    RelatedBaseSkillName = "Shooting"
                },
                new SpecialSkillDTO()
                {
                    Name="Firearms",
                    RelatedAttribute1 = "Instinct",
                    RelatedAttribute2 = "Intelligence",
                    RelatedBaseSkillName = "Shooting"
                },
                new SpecialSkillDTO()
                {
                    Name="Grenades",
                    RelatedAttribute1 =  "Instinct",
                    RelatedAttribute2 = "Strength",
                    RelatedBaseSkillName = "Shooting"
                },

                // ACROBATICS

                new SpecialSkillDTO()
                {
                    Name="Jumping",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Acrobatics"
                },
                new SpecialSkillDTO()
                {
                    Name="Climbing",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Strength" ,
                    RelatedBaseSkillName = "Acrobatics"
                },
                new SpecialSkillDTO()
                {
                    Name="Balance",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Acrobatics"
                },
                new SpecialSkillDTO()
                {
                    Name="Running",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Endurance",
                    RelatedBaseSkillName = "Acrobatics"
                },
                new SpecialSkillDTO()
                {
                    Name="Dodge",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Acrobatics"
                },

                // SLEIGHT OF HANDS

                new SpecialSkillDTO()
                {
                    Name="Pickpocketing",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Sleight of hands"
                },
                new SpecialSkillDTO()
                {
                    Name="Lockpicking",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Intelligence" ,
                    RelatedBaseSkillName = "Sleight of hands"
                },
                new SpecialSkillDTO()
                {
                    Name="Disarming traps",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Sleight of hands"
                },
                new SpecialSkillDTO()
                {
                    Name="Tricks",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Sleight of hands"
                },
                new SpecialSkillDTO()
                {
                    Name="Handcraft",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Sleight of hands"
                },

                // ATHLETICS Athletics

                new SpecialSkillDTO()
                {
                    Name="Wrestling",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct",
                    RelatedBaseSkillName = "Athletics"
                },
                new SpecialSkillDTO()
                {
                    Name="Lifting",
                    ChosenAttribute = "Strength",
                    RelatedBaseSkillName = "Athletics"
                },
                new SpecialSkillDTO()
                {
                    Name="Threatening",
                    ChosenAttribute = "Strength",
                    RelatedBaseSkillName = "Athletics"
                },
                new SpecialSkillDTO()
                {
                    Name="Armor",
                    ChosenAttribute = "Endurance",
                    RelatedBaseSkillName = "Athletics"
                },
                new SpecialSkillDTO()
                {
                    Name="Pain Resistance",
                    RelatedAttribute1 = "Endurance",
                    RelatedAttribute2 = "Willpower",
                    RelatedBaseSkillName = "Athletics"
                },
                new SpecialSkillDTO()
                {
                    Name="Swimming",
                    RelatedAttribute1 = "Endurance",
                    RelatedAttribute2 = "Strength" ,
                    RelatedBaseSkillName = "Athletics"
                },

                // TALK 

                new SpecialSkillDTO()
                {
                    Name="Persuasion",
                    RelatedAttribute1 =  "Charisma",
                    RelatedAttribute2 = "Intelligence",
                    RelatedBaseSkillName = "Talk"
                },
                new SpecialSkillDTO()
                {
                    Name="Bluff",
                    RelatedAttribute1 =  "Charisma",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Talk"
                },
                new SpecialSkillDTO()
                {
                    Name="Acting",
                    ChosenAttribute = "Charisma",
                    RelatedBaseSkillName = "Talk"
                },
                new SpecialSkillDTO()
                {
                    Name="Public speech",
                    RelatedAttribute1 =  "Charisma",
                    RelatedAttribute2 = "Willpower" ,
                    RelatedBaseSkillName = "Talk"
                },
                new SpecialSkillDTO()
                {
                    Name="Inspire",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Willpower" ,
                    RelatedBaseSkillName = "Talk"
                },
                new SpecialSkillDTO()
                {
                    Name="Diplomacy",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Talk"
                },
                new SpecialSkillDTO()
                {
                    Name="Trade",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Willpower",
                    RelatedBaseSkillName = "Talk"
                },

                // DECEIT Deceit

                new SpecialSkillDTO()
                {
                    Name="Sneak",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Deceit"
                },
                new SpecialSkillDTO()
                {
                    Name="Gambling",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Instinct",
                    RelatedBaseSkillName = "Deceit"
                },
                new SpecialSkillDTO()
                {
                    Name="Dirty tricks",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Deceit"
                },
                new SpecialSkillDTO()
                {
                    Name="Investigation",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Deceit"
                },
                new SpecialSkillDTO()
                {
                    Name="Disguise",
                    RelatedAttribute1 =  "Intelligence",
                    RelatedAttribute2 = "Dexterity",
                    RelatedBaseSkillName = "Deceit"
                },
                new SpecialSkillDTO()
                {
                    Name="Intimidate",
                    ChosenAttribute = "Charisma",
                    RelatedBaseSkillName = "Deceit"
                },

                // PERCEPTION

                new SpecialSkillDTO()
                {
                    Name="Observation",
                    RelatedAttribute1 ="Intelligence",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Perception"
                },
                new SpecialSkillDTO()
                {
                    Name="Sense motives",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Instinct",
                    RelatedBaseSkillName = "Perception"
                },
                new SpecialSkillDTO()
                {
                    Name="Hearing",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Perception"
                },
                new SpecialSkillDTO()
                {
                    Name="Smell",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Perception"
                },
                new SpecialSkillDTO()
                {
                    Name="Vigilance",
                    RelatedAttribute1 = "Willpower",
                    RelatedAttribute2 = "Instinct",
                    RelatedBaseSkillName = "Perception"
                },

                // KNOWLADGE

                new SpecialSkillDTO()
                {
                    Name="History and religion",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },
                new SpecialSkillDTO()
                {
                    Name="Beasts",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },
                new SpecialSkillDTO()
                {
                    Name="Linguistics",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },
                new SpecialSkillDTO()
                {
                    Name="Races and nations",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },
                new SpecialSkillDTO()
                {
                    Name="Geography",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },
                new SpecialSkillDTO()
                {
                    Name="Plants and mushrooms",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },
                new SpecialSkillDTO()
                {
                    Name="Heraldry",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },
                new SpecialSkillDTO()
                {
                    Name="Mathematics and logic",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },
                new SpecialSkillDTO()
                {
                    Name="Alchemy and physics",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },
                new SpecialSkillDTO()
                {
                    Name="Strategy and tactics",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge"
                },

                // CRAFT 

                new SpecialSkillDTO()
                {
                    Name="Architecture and stonemasonry",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Strength", 
                    RelatedBaseSkillName = "Craft"
                },
                new SpecialSkillDTO()
                {
                    Name="Geology and mining",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Instinct", 
                    RelatedBaseSkillName = "Craft"
                },
                new SpecialSkillDTO()
                {
                    Name="Metallurgy and blacksmithing",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Strength", 
                    RelatedBaseSkillName = "Craft"
                },
                new SpecialSkillDTO()
                {
                    Name="Engineering and gunsmithing",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Craft"
                },
                new SpecialSkillDTO()
                {
                    Name="Shipbuilding and carpentry",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Craft"
                },
                new SpecialSkillDTO()
                {
                    Name="Fine arts",
                    Editable = true,
                    RelatedBaseSkillName = "Craft"
                },

                // SURVIVAL 

                new SpecialSkillDTO()
                {
                    Name="Tracking",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Survival"
                },
                new SpecialSkillDTO()
                {
                    Name="Sense of direction",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Survival"
                },
                new SpecialSkillDTO()
                {
                    Name="Trapping",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Survival"
                },
                new SpecialSkillDTO()
                {
                    Name="Wilderness knowledge",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Survival"
                },
                new SpecialSkillDTO()
                {
                    Name="Sailing",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Survival"
                },

                // MEDICINE 
                new SpecialSkillDTO()
                {
                    Name="Surgery",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Medicine"
                },
                new SpecialSkillDTO()
                {
                    Name="Tend wounds",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Medicine"
                },
                new SpecialSkillDTO()
                {
                    Name="Diseases",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Medicine"
                },
                new SpecialSkillDTO()
                {
                    Name="Tend beasts",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Instinct", 
                    RelatedBaseSkillName = "Medicine"
                },
                new SpecialSkillDTO()
                {
                    Name="Poisons and venoms",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Medicine"
                },
                new SpecialSkillDTO()
                {
                    Name="Torture",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Willpower", 
                    RelatedBaseSkillName = "Medicine"
                },

                // ANIMAL HANDLE 

                new SpecialSkillDTO()
                {
                    Name="Training",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Willpower", 
                    RelatedBaseSkillName = "Animal handle"
                },
                new SpecialSkillDTO()
                {
                    Name="Taming",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Willpower", 
                    RelatedBaseSkillName = "Animal handle"
                },
                new SpecialSkillDTO()
                {
                    Name="Riding",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Animal handle"
                },
                new SpecialSkillDTO()
                {
                    Name="Animals care",
                     RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Animal handle"
                },

            };
            return SpecialSkills;
        }
    }
}
