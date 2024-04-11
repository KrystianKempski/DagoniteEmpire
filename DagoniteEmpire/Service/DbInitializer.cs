using AutoMapper;
using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DagoniteEmpire.Service
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public DbInitializer(UserManager<IdentityUser> userManager,
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


                    IdentityUser user = new()
                    {
                        UserName = "krystian.kempski@gmail.com",
                        Email = "krystian.kempski@gmail.com",
                        EmailConfirmed = true,
                    };

                    _userManager.CreateAsync(user, "Admin123*").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();

                    user = new()
                    {
                        UserName = "guestplayer",
                        Email = "guestplayer@gmail.com",
                        EmailConfirmed = true,
                    };

                    _userManager.CreateAsync(user, "Guest123*").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.Role_HeroPlayer).GetAwaiter().GetResult();

                    user = new()
                    {
                        UserName = "guestGM",
                        Email = "guestGM@gmail.com",
                        EmailConfirmed = true,
                    };

                    _userManager.CreateAsync(user, "Guest123*").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.Role_GameMaster).GetAwaiter().GetResult();
                }

                if(_db.Races.FirstOrDefault(u=>u.Name == "Human") == null)
                {
                    RaceDTO raceHuman = new RaceDTO()
                    {
                        Name = "Human",
                        Description = "Humans are universal.Their strength lies in their diversity and adaptability",
                        RaceApproved = true,
                        Traits = new List<TraitRaceDTO>()
                        {
                            new TraitRaceDTO()
                            {
                                Name="Diversity",
                                Descr = "",
                                SummaryDescr = "Human characters gain a +2 racial bonus to one attribute score of their choice at creation to represent their varied nature",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Strength",
                                        BonusValue = 2,
                                    }
                                }
                            },
                            new TraitRaceDTO()
                            {
                                Name="Attribute Score Modifier",
                                Descr = "",
                                SummaryDescr = "Human characters gain the +1 racial bonus to two basic skills score of their choice at creation to represent their universal nature",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Melee",
                                        BonusValue = 1,
                                    },
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Shooting",
                                        BonusValue = 1,
                                    }

                                }
                            }
                        }
                    };
                    _db.Races.Add(_mapper.Map<RaceDTO, Race>(raceHuman));
                    _db.SaveChanges();
                }
                if (_db.Races.FirstOrDefault(u => u.Name == "Dwarf") == null)
                {

                    RaceDTO raceDwarf = new RaceDTO()
                    {
                        Name = "Dwarf",
                        Description = "Common in the Empire, but rare in power. Fierce warriors and excellent craftsmen",
                        RaceApproved = true,
                        Traits = new List<TraitRaceDTO>()
                        {
                            new TraitRaceDTO()
                            {
                                Name="Attribute Score Modifier",
                                Descr = "",
                                SummaryDescr = "Dwarves are both tough and wise, but also a bit gruff.",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Endurance",
                                        BonusValue = 2,
                                    },
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Willpower",
                                        BonusValue = 2,
                                    },
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Charisma",
                                        BonusValue = -2,
                                    }
                                }
                            },
                            new TraitRaceDTO()
                            {
                                Name="Hardy",
                                Descr = "",
                                SummaryDescr = "Dwarf are hard to overpower, and proficient in armor",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Athletics",
                                        BonusValue = 2,
                                    },
                                }
                            },
                            new TraitRaceDTO()
                            {
                                Name="Excelent craftsment",
                                Descr = "",
                                SummaryDescr = "All dwarves have natural talent with craftsmenship",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Craft",
                                        BonusValue = 2,
                                    },
                                }
                            },
                            new TraitRaceDTO()
                            {
                                Name="Darkvision",
                                Descr = "",
                                SummaryDescr = "This race can see perfectly in the dark up to 60 feet",
                                TraitApproved = true,
                                IsUnique=false,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Darkvision",
                                        Description = "This race can see perfectly in the dark up to 60 feet"
                                    },
                                }
                            },
                            new TraitRaceDTO()
                            {
                                Name="Hatred",
                                Descr = "",
                                SummaryDescr = "Dwarves gain a +1 racial bonus on attack rolls against humanoid creatures of the orc and goblinoid subtypes because of their special training against these hated foes",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Hatred",
                                        Description = "Dwarves gain a +1 racial bonus on attack rolls against humanoid creatures of the orc and goblinoid subtypes because of their special training against these hated foes"
                                    },
                                }
                            },
                             new TraitRaceDTO()
                            {
                                Name="Unpopular amongst people",
                                Descr = "",
                                SummaryDescr = "Non-human races receive a penalty for ruling and diplomacy as nobles in the Empire.",
                                TraitApproved = true,
                                IsUnique=false,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureOther,
                                        FeatureName = "Unpopular amongst people",
                                        Description = "Non-human races receive a penalty for ruling and diplomacy as nobles in the Empire."
                                    },
                                }
                            },
                        }

                    };

                    _db.Races.Add(_mapper.Map<RaceDTO, Race>(raceDwarf));
                    _db.SaveChanges();
                }

                if (_db.Races.FirstOrDefault(u => u.Name == "Elf") == null)
                {

                    RaceDTO raceElf = new RaceDTO()
                    {
                        Name = "Elf",
                        Description = "Long-lived children of natural world. Rather uncommon in Empire",
                        RaceApproved = true,
                        Traits = new List<TraitRaceDTO>()
                        {
                            new TraitRaceDTO()
                            {
                                Name="Attribute Score Modifier",
                                Descr = "",
                                SummaryDescr = "Elves are nimble, both in body and mind, but their form is frail.",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Dexterity",
                                        BonusValue = 2,
                                    },
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Intelligence",
                                        BonusValue = 2,
                                    },
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureAttribute,
                                        FeatureName = "Endurance",
                                        BonusValue = -2,
                                    }
                                }
                            },
                            new TraitRaceDTO()
                            {
                                Name="Keen Senses",
                                Descr = "",
                                SummaryDescr = "Elves' senses are naturally heightened",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureBaseSkill,
                                        FeatureName = "Perception",
                                        BonusValue = 2,
                                    },
                                }
                            },
                            new TraitRaceDTO()
                            {
                                Name="Elven Magic",
                                Descr = "",
                                SummaryDescr = "This ancient race have better connection to winds of magic",
                                TraitApproved = true,
                                IsUnique=true,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureOther,
                                        FeatureName = "Elven Magic",
                                        Description = "Elves gets bonus +2 to all spell-related rolls and defences."
                                    },
                                }
                            },
                            new TraitRaceDTO()
                            {
                                Name="Low-Light Vision",
                                Descr = "",
                                SummaryDescr = "This race can see twice as far as humans in conditions of dim light.",
                                TraitApproved = true,
                                IsUnique=false,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureOther,
                                        FeatureName = "Darkvision",
                                        Description = "This race can see twice as far as humans in conditions of dim light."
                                    },
                                }
                            },
                            new TraitRaceDTO()
                            {
                                Name="Unpopular amongst people",
                                Descr = "",
                                SummaryDescr = "Non-human races receive a penalty for ruling and diplomacy as nobles in the Empire.",
                                TraitApproved = true,
                                IsUnique=false,
                                TraitType=SD.TraitType_Race,
                                TraitValue = 0,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO()
                                    {
                                        FeatureType = SD.FeatureOther,
                                        FeatureName = "Unpopular amongst people",
                                        Description = "Non-human races receive a penalty for ruling and diplomacy as nobles in the Empire."
                                    },
                                }
                            },
                        }
                    };

                    _db.Races.Add(_mapper.Map<RaceDTO, Race>(raceElf));
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
