﻿using AutoMapper;
using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DA_DataAccess;

namespace DagoniteEmpire.Service
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser>_userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public DbInitializer(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            IMapper mapper)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
        }
        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }

                if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_HeroPlayer)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_DukePlayer)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_GameMaster)).GetAwaiter().GetResult();


                    ApplicationUser user = new()
                    {
                        UserName = "AdminKrystian",
                        Email = "krystian.kempski@gmail.com",
                        EmailConfirmed = true,
                    };

                    _userManager.CreateAsync(user, "Admin123*").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();

                    user = new()
                    {
                        UserName = "player",
                        Email = "player@example.com",
                        EmailConfirmed = true,
                    };

                    _userManager.CreateAsync(user, "Guest123*").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.Role_HeroPlayer).GetAwaiter().GetResult();

                    user = new()
                    {
                        UserName = "gm",
                        Email = "gm@example.com",
                        EmailConfirmed = true,
                    };

                    _userManager.CreateAsync(user, "Guest123*").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.Role_GameMaster).GetAwaiter().GetResult();
                }
                if (_db.Professions.FirstOrDefault(c => c.Name == SD.NPCName_GameMaster) == null)
                {
                    var proff = new Profession() { Name = SD.NPCName_GameMaster, Description="", RelatedAttributeName = "" };

                    _db.Professions.Add(proff);
                    _db.SaveChanges();
                }
                if (_db.Races.FirstOrDefault(c => c.Name == SD.NPCName_GameMaster) == null)
                {
                    var race = new Race() { Name = SD.NPCName_GameMaster };

                    _db.Races.Add(race);
                    _db.SaveChanges();
                }
                if (_db.Characters.FirstOrDefault(c=>c.NPCName == SD.NPCName_GameMaster) == null)
                {
                    var charac = new Character() { UserName = "GM", NPCName = SD.NPCName_GameMaster };

                    var profession = _db.Professions.FirstOrDefault(c => c.Name == SD.NPCName_GameMaster);
                    var race = _db.Races.FirstOrDefault(c => c.Name == SD.NPCName_GameMaster);
                    charac.ProfessionId = profession.Id;
                    charac.RaceId = race.Id;
                    charac.ImageUrl = "../images/gm_avatar.webp";

                    _db.Characters.Add(charac);
                    _db.SaveChanges();
                }

                if (_db.Races.FirstOrDefault(u => u.Name == "Human") == null)
                {
                    Race raceHuman = new Race()
                    {
                        Name = "Human",
                        Description = "Humans are universal.Their strength lies in their diversity and adaptability",
                        RaceApproved = true,
                        Traits = new List<TraitRace>()
                        {
                            new TraitRace()
                            {
                                Name="Diversity",
                                Descr = "Human characters gain a +2 racial bonus to one attribute score of their choice at creation to represent their varied nature",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                            },
                            new TraitRace()
                            {
                                Name="Attribute Score Modifier",
                                Descr = "Human characters gain the +1 racial bonus to two basic skills score of their choice at creation to represent their universal nature",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                            }
                        }
                    };
                    _db.Races.Add(raceHuman);
                    _db.SaveChanges();
                }
                if (_db.Races.FirstOrDefault(u => u.Name == "Dwarf") == null)
                {

                    Race raceDwarf = new Race()
                    {
                        Name = "Dwarf",
                        Description = "Common in the Empire, but rare in power. Fierce warriors and excellent craftsmen",
                        RaceApproved = true,
                        Traits = new List<TraitRace>()
                        {
                            new TraitRace()
                            {
                                Name="Attribute Score Modifier",
                                Descr = "Dwarves are both tough and wise, but also a bit gruff +2 Endurance, +2 Willpower, -2 Charisma.",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Endurance",
                                        BonusValue = 2,
                                    },
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Willpower",
                                        BonusValue = 2,
                                    },
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Charisma",
                                        BonusValue = -2,
                                    }
                                }
                            },
                            new TraitRace()
                            {
                                Name="Hardy",
                                Descr = "Dwarf are hard to overpower, and proficient in armor",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Athletics",
                                        BonusValue = 2,
                                    },
                                }
                            },
                            new TraitRace()
                            {
                                Name="Excelent craftsment",
                                Descr = "All dwarves have natural talent with craftsmenship",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Craft",
                                        BonusValue = 2,
                                    },
                                }
                            },
                            new TraitRace()
                            {
                                Name="Darkvision",
                                Descr = "",
                                TraitApproved = true,
                                IsUnique=false,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Darkvision",
                                        Description = "This race can see perfectly in the dark up to 60 feet"
                                    },
                                }
                            },
                            new TraitRace()
                            {
                                Name="Hatred",
                                Descr = "",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Hatred",
                                        Description = "Dwarves gain a +1 racial bonus on attack rolls against humanoid creatures of the orc and goblinoid subtypes because of their special training against these hated foes"
                                    },
                                }
                            },
                             new TraitRace()
                            {
                                Name="Unpopular amongst people",
                                Descr = "",
                                TraitApproved = true,
                                IsUnique=false,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureOther,
                                        FeatureName = "Unpopular amongst people",
                                        Description = "Non-human races receive a penalty for ruling and diplomacy as nobles in the Empire."
                                    },
                                }
                            },
                        }

                    };

                    _db.Races.Add(raceDwarf);
                    _db.SaveChanges();
                }

                if (_db.Races.FirstOrDefault(u => u.Name == "Elf") == null)
                {

                    Race raceElf = new Race()
                    {
                        Name = "Elf",
                        Description = "Long-lived children of natural world. Rather uncommon in Empire",
                        RaceApproved = true,
                        Traits = new List<TraitRace>()
                        {
                            new TraitRace()
                            {
                                Name="Attribute Score Modifier",
                                Descr = "",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Dexterity",
                                        BonusValue = 2,
                                    },
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Intelligence",
                                        BonusValue = 2,
                                    },
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Endurance",
                                        BonusValue = -2,
                                    }
                                }
                            },
                            new TraitRace()
                            {
                                Name="Keen Senses",
                                Descr = "Elves' senses are naturally heightened",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Perception",
                                        BonusValue = 2,
                                    },
                                }
                            },
                            new TraitRace()
                            {
                                Name="Elven Magic",
                                Descr =  "This ancient race have better connection to winds of magic",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureOther,
                                        FeatureName = "Elven Magic",
                                        Description = "Elves gets bonus +2 to all spell-related rolls and defences."
                                    },
                                }
                            },
                            new TraitRace()
                            {
                                Name="Low-Light Vision",
                                Descr = "",
                                TraitApproved = true,
                                IsUnique=false,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureOther,
                                        FeatureName = "Darkvision",
                                        Description = "This race can see twice as far as humans in conditions of dim light."
                                    },
                                }
                            },
                            new TraitRace()
                            {
                                Name="Unpopular amongst people",
                                Descr = "",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus()
                                    {
                                        FeatureType = SD.FeatureOther,
                                        FeatureName = "Unpopular amongst people",
                                        Description = "Non-human races receive a penalty for ruling and diplomacy as nobles in the Empire."
                                    },
                                }
                            },
                        }
                    };

                    _db.Races.Add(raceElf);
                    _db.SaveChanges();
                }
                TraitCharacter trait = null;
                if (_db.TraitsCharacter.FirstOrDefault(u => u.Name == "Lame") == null)
                {
                    trait = new TraitCharacter()
                    {
                        Name = "Lame",
                        Descr = "An old wound or disfigurement makes this character limp",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Character,
                        TraitValue = -4,
                        Bonuses = new List<Bonus>()
                        {
                            new Bonus()
                            {
                                FeatureType = SD.FeatureBaseSkill,
                                FeatureName = "Melee",
                                BonusValue = -2,
                            },
                            new Bonus()
                            {
                                FeatureType = SD.FeatureAttribute,
                                FeatureName = "Charisma",
                                BonusValue = -1,
                            },
                        },
                    };
                    _db.TraitsCharacter.Add(trait);
                    _db.SaveChanges();
                }
                if (_db.TraitsCharacter.FirstOrDefault(u => u.Name == "Beautiful") == null)
                {
                    trait = new TraitCharacter()
                    {
                        Name = "Beautiful",
                        Descr = "This character is somehow physically beautiful",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Character,
                        TraitValue = 4,
                        Bonuses = new List<Bonus>()
                        {
                            new Bonus()
                            {
                                FeatureType = SD.FeatureDukeTraits,
                                FeatureName = "Loyalty",
                                BonusValue = 1,
                            },
                            new Bonus()
                            {
                                FeatureType = SD.FeatureAttribute,
                                FeatureName = "Charisma",
                                BonusValue = 2,
                            },
                        },
                    };
                    _db.TraitsCharacter.Add(trait);
                    _db.SaveChanges();
                }
                if (_db.TraitsCharacter.FirstOrDefault(u => u.Name == "Genius") == null)
                {
                    trait = new TraitCharacter()
                    {
                        Name = "Genius",
                        Descr = "This character is exceptionally intelligent",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Character,
                        TraitValue = 10,
                        Bonuses = new List<Bonus>()
                        {
                            new Bonus()
                            {
                                FeatureType = SD.FeatureAttribute,
                                FeatureName = "Intelligence",
                                BonusValue = 3,
                            },
                            new Bonus()
                            {
                                FeatureType = SD.FeatureAttribute,
                                FeatureName = "Instinct",
                                BonusValue = 3,
                            },
                        },
                    };
                    _db.TraitsCharacter.Add(trait);
                    _db.SaveChanges();
                }
                if (_db.TraitsCharacter.FirstOrDefault(u => u.Name == "Ugly") == null)
                {
                    trait = new TraitCharacter()
                    {
                        Name = "Ugly",
                        Descr = "This character is not pleasant to eyes",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Character,
                        TraitValue = -4,
                        Bonuses = new List<Bonus>()
                        {
                             new Bonus()
                            {
                                FeatureType = SD.FeatureDukeTraits,
                                FeatureName = "Loyalty",
                                BonusValue = -1,
                            },
                            new Bonus()
                            {
                                FeatureType = SD.FeatureAttribute,
                                FeatureName = "Charisma",
                                BonusValue = -2,
                            },
                        },
                    };
                    _db.TraitsCharacter.Add(trait);
                    _db.SaveChanges();
                }
                if (_db.TraitsCharacter.FirstOrDefault(u => u.Name == "Wrathful") == null)
                {
                    trait = new TraitCharacter()
                    {
                        Name = "Wrathful",
                        Descr =  "This character's outbursts of anger are frequent and violent (somethimes makes will checks)",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Character,
                        TraitValue = -1,
                        Bonuses = new List<Bonus>()
                        {
                            new Bonus()
                            {
                                FeatureType = SD.FeatureBaseSkill,
                                FeatureName = "Melee",
                                BonusValue = 2,
                            },
                            new Bonus()
                            {
                                FeatureType = SD.FeatureBaseSkill,
                                FeatureName = "Talk",
                                BonusValue = -2,
                            },
                            new Bonus()
                            {
                                FeatureType = SD.FeatureOther,
                                FeatureName = "Occasional fits of rage",
                                Description = "When this character finds himself in an uncomfortable situation, sometimes the GM can force him to make willpower test against stupid fury."
                            },
                        },
                    };
                    _db.TraitsCharacter.Add(trait);
                    _db.SaveChanges();
                }
                /// EQUIPMENT

                Equipment item;

                if (_db.Equipment.FirstOrDefault(u => u.Name == "Bandage") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Bandage",
                        Description = "For dressing wounds",
                        EquipmentType = SD.EquipmentType.Other,
                        ShortDescr = "For dressing wounds",
                        Weight = 0.2m,
                        Price = 0.01m,
                        IsApproved = true,

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Wound balm") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Wound balm",
                        EquipmentType = SD.EquipmentType.Other,
                        Description = "Helps with healing wounds. 20 doses, +2 to tending wounds. 1 Dose for ligth and medium wounds, 2 for heavy, and 4 for critical",
                        ShortDescr = "Helps with healing wounds. 20 doses, +2 to tending wounds. 1 Dose for ligth and medium wounds, 2 for heavy, and 4 for critical",
                        Weight = 1.0m,
                        Price = 0.1m,
                        IsApproved = true,

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Rope") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Rope",
                        EquipmentType = SD.EquipmentType.Other,
                        Description = "20 feat of strong rope",
                        ShortDescr = "20 feat of strong rope",

                        Weight = 5.0m,
                        Price = 0.1m,
                        IsApproved = true,

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                

                // ARMORS
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Light leather armor") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Light leather armor",
                        EquipmentType = SD.EquipmentType.Body,
                        Description = "Light but sturdy",
                        ShortDescr = "Light but sturdy",
                        Weight = 10.0m,
                        Price = 5.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Armor,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = -4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 1,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Leather scale armor") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Leather scale armor",
                        EquipmentType = SD.EquipmentType.Body,
                        Description = "Offers good protection and mobility",
                        ShortDescr = "Offers good protection and mobility",
                        Weight = 15.0m,
                        Price = 10.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Armor,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = -2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Steal scale armor") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Steal scale armor",
                        EquipmentType = SD.EquipmentType.Body,
                        Description = "Offers good protection and mobility",
                        ShortDescr = "Offers good protection and mobility",
                        Weight = 20.0m,
                        Price = 20.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 6,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Armor,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = 1,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Half plate") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Half plate",
                        EquipmentType = SD.EquipmentType.Body,
                        Description = "Good protection of solid steal",
                        ShortDescr = "Good protection of solid steal",
                        Weight = 30.0m,
                        Price = 50.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 8,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Armor,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Full plate") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Full plate",
                        EquipmentType = SD.EquipmentType.Body,
                        Description = "Best protection there is",
                        ShortDescr = "Best protection there is",
                        Weight = 40.0m,
                        Price = 80.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 10,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Armor,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 6,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                //SHIELDS
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Wooden buckler") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Wooden buckler",
                        EquipmentType = SD.EquipmentType.Shield,
                        RelatedSkill = SD.SpecialSkills.Melee.Shields,
                        Description = "Small, but better than nothing",
                        ShortDescr = "Small, but better than nothing",
                        Weight = 2.0m,
                        Price = 0.6m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ShieldDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 1,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }

                if (_db.Equipment.FirstOrDefault(u => u.Name == "Metal buckler") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Metal buckler",
                        EquipmentType = SD.EquipmentType.Shield,
                        RelatedSkill = SD.SpecialSkills.Melee.Shields,
                        Description = "Small, but better than nothing",
                        ShortDescr = "Small, but better than nothing",
                        Weight = 2.0m,
                        Price = 2.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ShieldDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Wooden shield") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Wooden shield",
                        EquipmentType = SD.EquipmentType.Shield,
                        RelatedSkill = SD.SpecialSkills.Melee.Shields,
                        Description = "Simple, wooden shield",
                        ShortDescr = "Simple, wooden shield",
                        Weight = 5.0m,
                        Price = 1.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ShieldDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
               
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Metal shield") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Metal shield",
                        EquipmentType = SD.EquipmentType.Shield,
                        RelatedSkill = SD.SpecialSkills.Melee.Shields,
                        Description = "Strong, metal shield",
                        ShortDescr = "Strong, metal shield",
                        Weight = 5.0m,
                        Price = 6.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ShieldDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }

                if (_db.Equipment.FirstOrDefault(u => u.Name == "Big wooden shield") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Big wooden shield",
                        EquipmentType = SD.EquipmentType.Shield,
                        RelatedSkill = SD.SpecialSkills.Melee.Shields,
                        Description = "Biger for better protection",
                        ShortDescr = "Biger for better protectiond",
                        Weight = 10.0m,
                        Price = 2.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ShieldDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Big metal shield") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Big metal shield",
                        EquipmentType = SD.EquipmentType.Shield,
                        RelatedSkill = SD.SpecialSkills.Melee.Shields,
                        Description = "Biger for better protection",
                        ShortDescr = "Biger for better protectiond",
                        Weight = 10.0m,
                        Price = 9.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ShieldDefenceBonus,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPenalty,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                // WEAPONS MELEE
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Dagger") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Dagger",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Small and deadly",
                        ShortDescr = "Small and deadly",
                        RelatedSkill = SD.SpecialSkills.Melee.Light,
                        Weight = 1.0m,
                        Price = 0.5m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>() {
                            new TraitEquipment(){ 
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description =  SD.WeaponQuality.Fast,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Light,
                                        FeatureName = SD.WeaponQuality.Light,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPiercing,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    }
                                }
                            }
                        },

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Long sword") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Long sword",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Main tool of all adventurers",
                        ShortDescr = "Main tool of all adventurers",
                        RelatedSkill = SD.SpecialSkills.Melee.Swords,
                        Weight = 3.0m,
                        Price = 3.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Parrying,
                                        FeatureName = SD.WeaponQuality.Parrying,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Disarming,
                                        FeatureName = SD.WeaponQuality.Disarming,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Battle axe") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Battle axe",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Simple and deadly",
                        ShortDescr = "Simple and deadly",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 3.0m,
                        Price = 1.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>() {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description =  SD.WeaponQuality.ShieldDestructive,
                                        FeatureName = SD.WeaponQuality.ShieldDestructive,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Devastating,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    }
                                }
                            }
                        },
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Pickaxe") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Pickaxe",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Good for penetrating armor",
                        ShortDescr = "Good for penetrating armor",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 4.0m,
                        Price = 2.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPiercing,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ShieldDestructive,
                                        FeatureName = SD.WeaponQuality.ShieldDestructive,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Mace") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Mace",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "One handed and good way to stun opponent",
                        ShortDescr = "One handed and good way to stun opponent",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 5.0m,
                        Price = 2.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Devastating,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Stunning,
                                        FeatureName = SD.WeaponQuality.Stunning,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Morningstar") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Morningstar",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Weapon of heavily armed knights",
                        ShortDescr = "Weapon of heavily armed knights",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 5.0m,
                        Price = 6.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Devastating,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPiercing,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Short spear") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Short spear",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Basic weapon of all soldiers",
                        ShortDescr = "Basic weapon of all soldiers",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 1.0m,
                        Price = 0.5m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Long,
                                        FeatureName = SD.WeaponQuality.Long,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Fast,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Rapier") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Rapier",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Fast and elegant weapon",
                        ShortDescr = "Fast and elegant weapon",
                        RelatedSkill = SD.SpecialSkills.Melee.Fencing,
                        Weight = 2.0m,
                        Price = 6.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Parrying,
                                        FeatureName = SD.WeaponQuality.Parrying,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPiercing,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Fast,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }

                if (_db.Equipment.FirstOrDefault(u => u.Name == "Two-handed flail") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Two-handed flail",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Heavy and slow, but easy to knock down an opponent",
                        ShortDescr = "Heavy and slow, but easy to knock down an opponent",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 10.0m,
                        Price = 3.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Heavy,
                                        FeatureName = SD.WeaponQuality.Heavy,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Slow,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 7,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Stumbling,
                                        FeatureName = SD.WeaponQuality.Stumbling,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Warhammer") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Warhammer",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Powerful weapon that can easily stun the enemy",
                        ShortDescr = "Powerful weapon that can easily stun the enemy",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 15.0m,
                        Price = 7.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Heavy,
                                        FeatureName = SD.WeaponQuality.Heavy,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Slow,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Devastating,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 8,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Stunning,
                                        FeatureName = SD.WeaponQuality.Stunning,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Greataxe") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Greataxe",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "A truly devastating weapon",
                        ShortDescr = "A truly devastating weapon",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 10.0m,
                        Price = 7.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Heavy,
                                        FeatureName = SD.WeaponQuality.Heavy,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Slow,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Devastating,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ShieldDestructive,
                                        FeatureName = SD.WeaponQuality.ShieldDestructive,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Poleaxe") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Poleaxe",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Axe head on long pole",
                        ShortDescr = "Axe head on long pole",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 12.0m,
                        Price = 7.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Heavy,
                                        FeatureName = SD.WeaponQuality.Heavy,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Slow,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 6,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ShieldDestructive,
                                        FeatureName = SD.WeaponQuality.ShieldDestructive,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Long,
                                        FeatureName = SD.WeaponQuality.Long,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Sarissa") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Sarissa",
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Very long spear",
                        ShortDescr = "Very long spear",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 15.0m,
                        Price = 3.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Slow,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPiercing,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Long,
                                        FeatureName = SD.WeaponQuality.Long,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                // WEAPONS RANGED
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Crossbow, light") == null)
                { 
                    item = new Equipment()
                    {
                        Name = "Crossbow, light",
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Easy to use and slow to reload",
                        ShortDescr = "Easy to use and slow to reload",
                        RelatedSkill = SD.SpecialSkills.Shooting.Crossbows,
                        Weight = 6.0m,
                        Price = 10.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPiercing,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Fast,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                    new Bonus{
                                        BonusValue = 1,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Reload,
                                        FeatureName = SD.WeaponQuality.Reload,
                                    },
                                    new Bonus{
                                        BonusValue = 20,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Range,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Crossbow, heavy") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Crossbow, heavy",
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Powerfull but slow",
                        ShortDescr = "Powerfull but slow",
                        RelatedSkill = SD.SpecialSkills.Shooting.Crossbows,
                        Weight = 6.0m,
                        Price = 10.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPiercing,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Devastating,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Fast,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Reload,
                                        FeatureName = SD.WeaponQuality.Reload,
                                    },
                                    new Bonus{
                                        BonusValue = 30,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Range,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Bow, simple") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Bow, simple",
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Common tool of hunters",
                        ShortDescr = "Common tool of hunters",
                        RelatedSkill = SD.SpecialSkills.Shooting.Bows,
                        Weight = 3.0m,
                        Price = 1.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.ArmorPiercing,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Fast,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },

                                    new Bonus{
                                        BonusValue = 20,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Range,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Longbow") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Longbow",
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Military archers primary weapon",
                        ShortDescr = "Military archers primary weapon",
                        RelatedSkill = SD.SpecialSkills.Shooting.Bows,
                        Weight = 3.0m,
                        Price = 1.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Fast,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },

                                    new Bonus{
                                        BonusValue = 40,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Range,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Slingshot") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Slingshot",
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Simple but effective",
                        ShortDescr = "Simple but effective",
                        RelatedSkill = SD.SpecialSkills.Shooting.Slingshots,
                        Weight = 0.5m,
                        Price = 0.5m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = "Weapon properties",
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Devastating,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },

                                    new Bonus{
                                        BonusValue = 40,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.Range,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        Description = SD.WeaponQuality.TwoHanded,
                                        FeatureName = SD.WeaponQuality.TwoHanded,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }

                //// add proffesion
                //if(_db.Professions.FirstOrDefault(c => c.Name == "Warrior") is null)
                //{
                //    var profession = new Profession() { Name = "Warrior", Description = "Mighty warrior, proficent in melee and ranged weapons." };
                //    _db.Professions.Add(profession);
                //    _db.SaveChanges();
                //}

                // add character

                //if (_db.Characters.FirstOrDefault(c => c.NPCName == "Mściwój") == null)
                // {
                //     string contents = File.ReadAllText(@"../seederFiles/AttributesMsciwoj");
                //     object value = _db.Database.ExecuteSqlRaw(contents);
                //     //    var charac = new Character() { UserName = "player", NPCName = "Mściwój" };
                //     //    var attributes = new Feature() { Name = SD.Attributes.Strength, BaseBonus = 18  }

                //     //    var profession = _db.Professions.FirstOrDefault(c => c.Name == "Warrior");
                //     //    var race = _db.Races.FirstOrDefault(c => c.Name == "Dwarf");
                //     //    charac.ProfessionId = profession.Id;
                //     //    charac.RaceId = race.Id;
                //     //    charac.ImageUrl = "../images/Msciwoj.webp";

                //     //    _db.Characters.Add(charac);
                //     //    _db.SaveChanges();
                // }
            }
            catch (Exception ex)
            {
                ;
            }
        }
    }
}
