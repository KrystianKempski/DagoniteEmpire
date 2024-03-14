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
        static public ICollection<AttributeDTO> GetAttributes()
        {
            var attributes = new List<AttributeDTO>()
            {
                new AttributeDTO()
                {
                    Name = "Strength",
                    Index = 0,
                },
                new AttributeDTO()
                {
                    Name = "Dexterity",
                    Index = 1,
                },
                new AttributeDTO()
                {
                    Name = "Endurance",
                    Index = 2,
                    TempBonuses = 0,
                },
                new AttributeDTO()
                {
                    Name = "Intelligence",
                    Index = 3,
                },
                new AttributeDTO()
                {
                    Name = "Instinct",
                    Index = 4,
                },
                new AttributeDTO()
                {
                    Name = "Willpower",
                    Index = 5,
                },
                new AttributeDTO()
                {
                    Name = "Charisma",
                    Index = 6,
                },
            };
            return attributes;
        }

        static public ICollection<BaseSkillDTO> GetBaseSkills()
        {
            var baseSkills = new List<BaseSkillDTO>()
            {
                new BaseSkillDTO()
                {
                    Name = "Melee",
                    RelatedAttribute1 = "Strength",
                    RelatedAttribute2 = "Dexterity",
                    Index= 0,
                   
                },
                new BaseSkillDTO()
                {
                    Name = "Shooting",
                    RelatedAttribute1 = "Instinct",
                    RelatedAttribute2 = "Dexterity",
                    Index= 1,
                },
                new BaseSkillDTO()
                {
                    Name = "Acrobatics",
                    RelatedAttribute1 = "Endurance", 
                    RelatedAttribute2 = "Dexterity",
                    Index= 2,
                },
                new BaseSkillDTO()
                {
                    Name = "Sleight of hands",
                    RelatedAttribute1 = "Instinct", 
                    RelatedAttribute2 = "Dexterity",
                    Index= 3,
                },
                new BaseSkillDTO()
                {
                    Name = "Athletics",
                    RelatedAttribute1 = "Endurance",
                    RelatedAttribute2 = "Strength",
                    Index= 4,
                },
                new BaseSkillDTO()
                {
                    Name = "Talk",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Instinct",
                    Index= 5,
                },
                new BaseSkillDTO()
                {
                    Name = "Deceit",
                    RelatedAttribute1 = "Instinct",
                    RelatedAttribute2 = "Charisma",
                    Index= 6,
                },
                 new BaseSkillDTO()
                {
                    Name = "Perception",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Instinct",
                    Index= 7,
                },
                new BaseSkillDTO()
                {
                    Name = "Knowledge",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Willpower",
                    Index= 8,
                },
                new BaseSkillDTO()
                {
                    Name = "Craft",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Endurance",
                    Index= 9,
                },
                new BaseSkillDTO()
                {
                    Name = "Survival",
                     RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Dexterity",
                    Index= 10,
                },
                new BaseSkillDTO()
                {
                    Name = "Animal handle",
                     RelatedAttribute1 = "Instinct",
                    RelatedAttribute2 = "Willpower",
                    Index= 11,
                },
                new BaseSkillDTO()
                {
                    Name = "Medicine",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Willpower",
                    Index= 12,
                },
                
            };
            return baseSkills;
        }
        static public ICollection<SpecialSkillDTO> GetSpecialSkills()
        {
            var SpecialSkills = new List<SpecialSkillDTO>()
            {
                // MELEE
                new SpecialSkillDTO()
                {
                    Name="Heavy weapons",
                    ChosenAttribute = "Strength",
                    RelatedBaseSkillName = "Melee",
                    Index = 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Swords and sabres",
                    RelatedAttribute1="Strength",
                    RelatedAttribute2="Dexterity", 
                    RelatedBaseSkillName = "Melee",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Fencing weapons",
                    RelatedAttribute1="Strength",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Melee",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Light weapons",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Melee",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Shields",
                    RelatedAttribute1="Strength",
                    RelatedAttribute2 = "Endurance", 
                    RelatedBaseSkillName = "Melee",
                    Index= 4,
                },
                new SpecialSkillDTO()
                {
                    Name="Polearms",
                    RelatedAttribute1="Strength",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Melee",
                    Index= 5,
                },
                new SpecialSkillDTO()
                {
                    Name="Exotic weapons",
                    RelatedBaseSkillName = "Melee",
                    Index= 6,
                    Editable = true,
                },

                // SHOOTING
                new SpecialSkillDTO()
                {
                    Name="Bows",
                    RelatedAttribute1= "Strength",
                    RelatedAttribute2 =  "Dexterity",
                    RelatedBaseSkillName = "Shooting",
                    Index= 0,
                },

                new SpecialSkillDTO()
                {
                    Name="Crossbows",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Shooting",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Throwing weapons",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 ="Strength" ,
                    RelatedBaseSkillName = "Shooting",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Slingshots",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Strength" ,
                    RelatedBaseSkillName = "Shooting",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Javelins",
                    ChosenAttribute = "Strength",
                    RelatedBaseSkillName = "Shooting",
                    Index= 4,
                },
                new SpecialSkillDTO()
                {
                    Name="Firearms",
                    RelatedAttribute1 = "Instinct",
                    RelatedAttribute2 = "Intelligence",
                    RelatedBaseSkillName = "Shooting",
                    Index= 5,
                },
                new SpecialSkillDTO()
                {
                    Name="Grenades",
                    RelatedAttribute1 =  "Instinct",
                    RelatedAttribute2 = "Strength",
                    RelatedBaseSkillName = "Shooting",
                    Index= 6,
                },

                // ACROBATICS

                new SpecialSkillDTO()
                {
                    Name="Jumping",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Acrobatics",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Climbing",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Strength" ,
                    RelatedBaseSkillName = "Acrobatics",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Balance",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Acrobatics",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Running",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Endurance",
                    RelatedBaseSkillName = "Acrobatics",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Dodge",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Acrobatics",
                    Index= 4,
                },

                // SLEIGHT OF HANDS

                new SpecialSkillDTO()
                {
                    Name="Pickpocketing",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Sleight of hands",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Lockpicking",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Intelligence" ,
                    RelatedBaseSkillName = "Sleight of hands",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Disarming traps",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Sleight of hands",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Tricks",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Sleight of hands",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Handcraft",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Sleight of hands",
                    Index= 4,
                },

                // ATHLETICS Athletics

                new SpecialSkillDTO()
                {
                    Name="Wrestling",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct",
                    RelatedBaseSkillName = "Athletics",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Lifting",
                    ChosenAttribute = "Strength",
                    RelatedBaseSkillName = "Athletics",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Threatening",
                    ChosenAttribute = "Strength",
                    RelatedBaseSkillName = "Athletics",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Armor",
                    ChosenAttribute = "Endurance",
                    RelatedBaseSkillName = "Athletics",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Pain Resistance",
                    RelatedAttribute1 = "Endurance",
                    RelatedAttribute2 = "Willpower",
                    RelatedBaseSkillName = "Athletics",
                    Index= 4,
                },
                new SpecialSkillDTO()
                {
                    Name="Swimming",
                    RelatedAttribute1 = "Endurance",
                    RelatedAttribute2 = "Strength" ,
                    RelatedBaseSkillName = "Athletics",
                    Index= 5,
                },

                // TALK 

                new SpecialSkillDTO()
                {
                    Name="Persuasion",
                    RelatedAttribute1 =  "Charisma",
                    RelatedAttribute2 = "Intelligence",
                    RelatedBaseSkillName = "Talk",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Bluff",
                    RelatedAttribute1 =  "Charisma",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Talk",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Acting",
                    ChosenAttribute = "Charisma",
                    RelatedBaseSkillName = "Talk",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Public speech",
                    RelatedAttribute1 =  "Charisma",
                    RelatedAttribute2 = "Willpower" ,
                    RelatedBaseSkillName = "Talk",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Inspire",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Willpower" ,
                    RelatedBaseSkillName = "Talk",
                    Index= 4,
                },
                new SpecialSkillDTO()
                {
                    Name="Diplomacy",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Talk",
                    Index= 5,
                },
                new SpecialSkillDTO()
                {
                    Name="Trade",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Willpower",
                    RelatedBaseSkillName = "Talk",
                    Index= 6,
                },

                // DECEIT Deceit

                new SpecialSkillDTO()
                {
                    Name="Sneak",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Deceit",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Gambling",
                    RelatedAttribute1 = "Intelligence",
                    RelatedAttribute2 = "Instinct",
                    RelatedBaseSkillName = "Deceit",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Dirty tricks",
                    RelatedAttribute1 = "Dexterity",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Deceit",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Investigation",
                    ChosenAttribute = "Dexterity",
                    RelatedBaseSkillName = "Deceit",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Disguise",
                    RelatedAttribute1 =  "Intelligence",
                    RelatedAttribute2 = "Dexterity",
                    RelatedBaseSkillName = "Deceit",
                    Index= 4,
                },
                new SpecialSkillDTO()
                {
                    Name="Intimidate",
                    ChosenAttribute = "Charisma",
                    RelatedBaseSkillName = "Deceit",
                    Index= 5,
                },

                // PERCEPTION

                new SpecialSkillDTO()
                {
                    Name="Observation",
                    RelatedAttribute1 ="Intelligence",
                    RelatedAttribute2 = "Instinct" ,
                    RelatedBaseSkillName = "Perception",
                        Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Sense motives",
                    RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Instinct",
                    RelatedBaseSkillName = "Perception",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Hearing",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Perception",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Smell",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Perception",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Vigilance",
                    RelatedAttribute1 = "Willpower",
                    RelatedAttribute2 = "Instinct",
                    RelatedBaseSkillName = "Perception",
                    Index= 4,
                },

                // KNOWLADGE

                new SpecialSkillDTO()
                {
                    Name="History and religion",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Beasts",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Linguistics",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Races and nations",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Geography",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 4,
                },
                new SpecialSkillDTO()
                {
                    Name="Plants and mushrooms",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 5,
                },
                new SpecialSkillDTO()
                {
                    Name="Heraldry",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 6,
                },
                new SpecialSkillDTO()
                {
                    Name="Mathematics and logic",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 7,
                },
                new SpecialSkillDTO()
                {
                    Name="Alchemy and physics",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 8,
                },
                new SpecialSkillDTO()
                {
                    Name="Strategy and tactics",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Knowledge",
                    Index= 9,
                },

                // CRAFT 

                new SpecialSkillDTO()
                {
                    Name="Architecture and stonemasonry",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Strength", 
                    RelatedBaseSkillName = "Craft",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Geology and mining",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Instinct", 
                    RelatedBaseSkillName = "Craft",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Metallurgy and blacksmithing",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Strength", 
                    RelatedBaseSkillName = "Craft",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Engineering and gunsmithing",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Craft",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Shipbuilding and carpentry",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Craft",
                    Index= 4,
                },
                new SpecialSkillDTO()
                {
                    Name="Fine arts",
                    Editable = true,
                    RelatedBaseSkillName = "Craft",
                    Index= 5,
                },

                // SURVIVAL 

                new SpecialSkillDTO()
                {
                    Name="Tracking",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Survival",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Sense of direction",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Survival",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Trapping",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Survival",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Wilderness knowledge",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Survival",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Sailing",
                    ChosenAttribute = "Instinct",
                    RelatedBaseSkillName = "Survival",
                    Index= 4,
                },

                // MEDICINE 
                new SpecialSkillDTO()
                {
                    Name="Surgery",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Medicine",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Tend wounds",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Medicine",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Diseases",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Medicine",
                        Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Tend beasts",
                    RelatedAttribute1="Intelligence",
                    RelatedAttribute2 = "Instinct", 
                    RelatedBaseSkillName = "Medicine",
                    Index= 3,
                },
                new SpecialSkillDTO()
                {
                    Name="Poisons and venoms",
                    ChosenAttribute = "Intelligence",
                    RelatedBaseSkillName = "Medicine",
                    Index= 4,
                },
                new SpecialSkillDTO()
                {
                    Name="Torture",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Willpower", 
                    RelatedBaseSkillName = "Medicine",
                    Index= 5,
                },

                // ANIMAL HANDLE 

                new SpecialSkillDTO()
                {
                    Name="Training",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Willpower", 
                    RelatedBaseSkillName = "Animal handle",
                    Index= 0,
                },
                new SpecialSkillDTO()
                {
                    Name="Taming",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Willpower", 
                    RelatedBaseSkillName = "Animal handle",
                    Index= 1,
                },
                new SpecialSkillDTO()
                {
                    Name="Riding",
                    RelatedAttribute1="Instinct",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Animal handle",
                    Index= 2,
                },
                new SpecialSkillDTO()
                {
                    Name="Animals care",
                     RelatedAttribute1 = "Charisma",
                    RelatedAttribute2 = "Dexterity", 
                    RelatedBaseSkillName = "Animal handle",
                    Index= 3,
                },

            };
            return SpecialSkills;
        }
    }
}
