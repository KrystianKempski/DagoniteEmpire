using AutoMapper;
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
                        UserName = "krystian.kempski@gmail.com",
                        Email = "krystian.kempski@gmail.com",
                        EmailConfirmed = true,
                    };

                    _userManager.CreateAsync(user, "Admin123*").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();

                    user = new()
                    {
                        UserName = "player@example.com",
                        Email = "player@example.com",
                        EmailConfirmed = true,
                    };

                    _userManager.CreateAsync(user, "Guest123*").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.Role_HeroPlayer).GetAwaiter().GetResult();

                    user = new()
                    {
                        UserName = "gm@example.com",
                        Email = "gm@example.com",
                        EmailConfirmed = true,
                    };

                    _userManager.CreateAsync(user, "Guest123*").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.Role_GameMaster).GetAwaiter().GetResult();
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
                                Descr = "",
                                SummaryDescr = "Human characters gain a +2 racial bonus to one attribute score of their choice at creation to represent their varied nature",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                            },
                            new TraitRace()
                            {
                                Name="Attribute Score Modifier",
                                Descr = "",
                                SummaryDescr = "Human characters gain the +1 racial bonus to two basic skills score of their choice at creation to represent their universal nature",
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
                                Descr = "",
                                SummaryDescr = "Dwarves are both tough and wise, but also a bit gruff +2 Endurance, +2 Willpower, -2 Charisma.",
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
                                Descr = "",
                                SummaryDescr = "Dwarf are hard to overpower, and proficient in armor",
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
                                Descr = "",
                                SummaryDescr = "All dwarves have natural talent with craftsmenship",
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
                                SummaryDescr = "This race can see perfectly in the dark up to 60 feet",
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
                                SummaryDescr = "Dwarves gain a +1 racial bonus on attack rolls against humanoid creatures of the orc and goblinoid subtypes because of their special training against these hated foes",
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
                                SummaryDescr = "Non-human races receive a penalty for ruling and diplomacy as nobles in the Empire.",
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
                                SummaryDescr = "Elves are nimble, both in body and mind, but their form is frail. +2 Dexterity, +2 Inteligence, -2 Endurance",
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
                                Descr = "",
                                SummaryDescr = "Elves' senses are naturally heightened",
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
                                Descr = "",
                                SummaryDescr = "This ancient race have better connection to winds of magic",
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
                                SummaryDescr = "This race can see twice as far as humans in conditions of dim light.",
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
                                SummaryDescr = "Non-human races receive a penalty for ruling and diplomacy as nobles in the Empire.",
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
                TraitAdv trait = null;
                if (_db.TraitsAdv.FirstOrDefault(u => u.Name == "Lame") == null)
                {
                    trait = new TraitAdv()
                    {
                        Name = "Lame",
                        Descr = "",
                        SummaryDescr = "An old wound or disfigurement makes this character limp. +2 Melee, -1 Charisma",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Advantage,
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
                    _db.TraitsAdv.Add(trait);
                    _db.SaveChanges();
                }
                if (_db.TraitsAdv.FirstOrDefault(u => u.Name == "Beautiful") == null)
                {
                    trait = new TraitAdv()
                    {
                        Name = "Beautiful",
                        Descr = "",
                        SummaryDescr = "This character is somehow physically beautiful. +2 Charisma, +1 Loyalty",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Advantage,
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
                    _db.TraitsAdv.Add(trait);
                    _db.SaveChanges();
                }
                if (_db.TraitsAdv.FirstOrDefault(u => u.Name == "Genius") == null)
                {
                    trait = new TraitAdv()
                    {
                        Name = "Genius",
                        Descr = "",
                        SummaryDescr = "This character is exceptionally intelligent. +3 Intelligence, +3 Instinct",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Advantage,
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
                    _db.TraitsAdv.Add(trait);
                    _db.SaveChanges();
                }
                if (_db.TraitsAdv.FirstOrDefault(u => u.Name == "Ugly") == null)
                {
                    trait = new TraitAdv()
                    {
                        Name = "Ugly",
                        Descr = "",
                        SummaryDescr = "This character is not pleasant to eyes. -2 Charisma, -1 Loyalty",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Advantage,
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
                    _db.TraitsAdv.Add(trait);
                    _db.SaveChanges();
                }
                if (_db.TraitsAdv.FirstOrDefault(u => u.Name == "Wrathful") == null)
                {
                    trait = new TraitAdv()
                    {
                        Name = "Wrathful",
                        Descr = "",
                        SummaryDescr = "This character's outbursts of anger are frequent and violent (somethimes makes will checks). +2 Melee, -2 Talk",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Advantage,
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
                    _db.TraitsAdv.Add(trait);
                    _db.SaveChanges();
                }
                /// EQUIPMENT

                Equipment item;
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Long sword") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Long sword",
                        Description = "Main tool of all adventurers",
                        ShortDescr = "Main tool of all adventurers",
                        Count = 1,
                        Weight = 3.0m,
                        Price = 1.0m,

                        IsApproved = true,
                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Leather armor") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Leather armor",
                        Description = "Light but sturdy",
                        ShortDescr = "Light but sturdy",
                        Weight = 10.0m,
                        Price = 5.0m,
                        Count = 1,
                        IsApproved = true,

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Bandage") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Bandage",
                        Description = "For dressing wounds",
                        ShortDescr = "For dressing wounds",
                        Weight = 0.2m,
                        Price = 0.01m,
                        Count = 1,
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
                        Description = "Helps with healing wounds. 20 doses, +2 to tending wounds. 1 Dose for ligth and medium wounds, 2 for heavy, and 4 for critical",
                        ShortDescr = "Helps with healing wounds. 20 doses, +2 to tending wounds. 1 Dose for ligth and medium wounds, 2 for heavy, and 4 for critical",
                        Weight = 1.0m,
                        Price = 0.1m,
                        Count = 1,
                        IsApproved = true,

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
                if (_db.Equipment.FirstOrDefault(u => u.Name == "Dagger") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Dagger",
                        Description = "Small and deadly",
                        ShortDescr = "Small and deadly",
                        Weight = 1.0m,
                        Price = 0.1m,
                        Count = 1,
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
                        Description = "20 feat of strong rope",
                        ShortDescr = "20 feat of strong rope",
                        Weight = 5.0m,
                        Price = 0.1m,
                        Count = 1,
                        IsApproved = true,

                    };
                    _db.Equipment.Add(item);
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
    }
}
