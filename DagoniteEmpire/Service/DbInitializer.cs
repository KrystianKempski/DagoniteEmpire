using AutoMapper;
using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DA_DataAccess;
using System;
using DA_Models;
using MudBlazor;
using Abp.Collections.Extensions;
using Microsoft.JSInterop;
using DagoniteEmpire.Helper;
using DA_DataAccess.BaronyData;
using DA_Business.Repository.BaronyRepos;
using DA_Business.Repository.MarchMapRepos;

namespace DagoniteEmpire.Service
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser>_userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public DbInitializer(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IDbContextFactory<ApplicationDbContext> db,
            IMapper mapper,
            IConfiguration configuration
            )
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
            _configuration = configuration;
        }
        public async Task Initialize()
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (contex.Database.GetPendingMigrations().Count() > 0)
                {
                    contex.Database.Migrate();
                }

                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_HeroPlayer));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_DukePlayer));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_GameMaster));
                }
                
                // characters
                if (_configuration.GetConnectionString("GameMasterEmail").IsNullOrEmpty() == true || _configuration.GetConnectionString("GameMasterPassword").IsNullOrEmpty() == true)
                {
                    throw new Exception("Could not get email or password from appsettings.json");
                }
                if (await _userManager.FindByEmailAsync(_configuration.GetConnectionString("GameMasterEmail")) is null)
                {
                    var email = _configuration.GetConnectionString("GameMasterEmail");
                    if (email.IsNullOrEmpty())
                    {
                        throw new Exception("Could not get email from appsettings.json");
                    }

                    ApplicationUser user = new()
                    {
                        UserName = "GameMaster",
                        Email = email,
                        EmailConfirmed = true,
                    };

                    var pass = _configuration.GetConnectionString("GameMasterPassword");
                    if (pass.IsNullOrEmpty())
                    {
                        throw new Exception("Could not get password from appsettings.json");
                    }
                    var res1 = await _userManager.CreateAsync(user, pass);
                    if (res1.Errors.Any())
                    {
                        foreach (var err in res1.Errors)
                        {
                            throw new Exception("Error while creating user: " + err.Code);
                        }

                    }
                    var res2 = await _userManager.AddToRoleAsync(user, SD.Role_Admin);
                    if (res2.Errors.Any())
                    {
                        foreach (var err in res1.Errors)
                        {
                            throw new Exception("Error while creating role: " + err.Code);
                        }
                    }
                }
                if (_configuration.GetConnectionString("TestAccountsEnable") == "true")
                {
                    if (await _userManager.FindByEmailAsync("player@example.com") is null)
                    {
                        ApplicationUser user = new()
                        {
                            UserName = "player",
                            Email = "player@example.com",
                            EmailConfirmed = true,
                        };

                        await _userManager.CreateAsync(user, "Guest123*");
                        await _userManager.AddToRoleAsync(user, SD.Role_HeroPlayer);

                    }
                    if (await _userManager.FindByEmailAsync("player2@example.com") is null)
                    {
                        ApplicationUser user = new()
                        {
                            UserName = "player2",
                            Email = "player2@example.com",
                            EmailConfirmed = true,
                        };

                        await _userManager.CreateAsync(user, "Guest123*");
                        await _userManager.AddToRoleAsync(user, SD.Role_HeroPlayer);

                    }

                    if (await _userManager.FindByEmailAsync("gm@example.com") is null)
                    {
                        ApplicationUser user = new()
                        {
                            UserName = "gm",
                            Email = "gm@example.com",
                            EmailConfirmed = true,
                        };
                        await _userManager.CreateAsync(user, "Guest123*");
                        await _userManager.AddToRoleAsync(user, SD.Role_GameMaster);
                    }

                    if (await _userManager.FindByEmailAsync("duke@duke.com") is null)
                    {
                        ApplicationUser user = new()
                        {
                            UserName = "Dukemaster",
                            Email = "duke@duke.com",
                            EmailConfirmed = true,
                        };
                        await _userManager.CreateAsync(user, "Guest123!");
                        await _userManager.AddToRoleAsync(user, SD.Role_DukePlayer);
                    }
                }

                // Hidden account backing the public "Try baron" demo. Always present (demo is public);
                // random password because sign-in happens server-side, never via the login form.
                if (await _userManager.FindByEmailAsync(SD.DemoBaronEmail) is null)
                {
                    ApplicationUser demoUser = new()
                    {
                        UserName = SD.DemoBaronUserName,
                        Email = SD.DemoBaronEmail,
                        EmailConfirmed = true,
                    };
                    await _userManager.CreateAsync(demoUser, "Demo!" + Guid.NewGuid().ToString("N") + "A1");
                    await _userManager.AddToRoleAsync(demoUser, SD.Role_DukePlayer);
                }

                // Hidden account backing the public "Try Game Master" demo. Same throwaway session
                // machinery as the baron demo, but signed in with the GameMaster role.
                if (await _userManager.FindByEmailAsync(SD.DemoGmEmail) is null)
                {
                    ApplicationUser demoGmUser = new()
                    {
                        UserName = SD.DemoGmUserName,
                        Email = SD.DemoGmEmail,
                        EmailConfirmed = true,
                    };
                    await _userManager.CreateAsync(demoGmUser, "Demo!" + Guid.NewGuid().ToString("N") + "A1");
                    await _userManager.AddToRoleAsync(demoGmUser, SD.Role_GameMaster);
                }

                await SyncBuildingTemplateCatalogAsync(contex);

                if (contex.Professions.FirstOrDefault(c => c.Name == SD.GameMaster_NPCName) == null)
                {
                    var prof = new Profession() { Name = SD.GameMaster_NPCName, Description="", RelatedAttributeName = "" };

                    contex.Professions.Add(prof);
                    contex.SaveChanges();
                }
                if (contex.Races.FirstOrDefault(c => c.Name == SD.GameMaster_NPCName) == null)
                {
                    var race = new Race() { Name = SD.GameMaster_NPCName };

                    contex.Races.Add(race);
                    contex.SaveChanges();
                }
                if (contex.Characters.FirstOrDefault(c=>c.NPCName == SD.GameMaster_NPCName) == null)
                {
                    var charac = new Character() { UserName = "GM", NPCName = SD.GameMaster_NPCName, Description="" };

                    var profession = contex.Professions.FirstOrDefault(c => c.Name == SD.GameMaster_NPCName);
                    var race = contex.Races.FirstOrDefault(c => c.Name == SD.GameMaster_NPCName);
                    charac.ProfessionId = profession.Id;
                    charac.RaceId = race.Id;
                    charac.ImageUrl = SD.GameMaster_Portrait;
                    charac.IsApproved = false;
                    contex.Characters.Add(charac);
                    contex.SaveChanges();
                }

                if (contex.Races.FirstOrDefault(u => u.Name == "Human") == null)
                {
                    Race raceHuman = new Race()
                    {
                        Name = "Human",
                        Description = "Humans are universal. Their strength lies in their diversity and adaptability",
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
                    contex.Races.Add(raceHuman);
                    contex.SaveChanges();
                }
                if (contex.Races.FirstOrDefault(u => u.Name == "Dwarf") == null)
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
                                Descr = "Dwarves are both tough and wise, but also a bit gruff",
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
                                Descr = "Dwarves are hard to overpower, and proficient in armor",
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
                                Name="Excellent craftsman",
                                Descr = "All dwarves have natural talent with craftsmanship",
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

                    contex.Races.Add(raceDwarf);
                    contex.SaveChanges();
                }

                if (contex.Races.FirstOrDefault(u => u.Name == "Elf") == null)
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
                                Descr = "Elves are nimble, both in body and mind, but their form is frail",
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
                                Descr =  "This ancient race has a better connection to winds of magic",
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
                                        Description = "Elves get a +2 bonus to all spell-related rolls and defences."
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
                        }
                    };
                    var unpopularTrait = contex.TraitsRace.FirstOrDefault(u => u.Name == "Unpopular amongst people");
                    if (unpopularTrait is null)
                    {

                        unpopularTrait = new TraitRace()
                        {
                            Name = "Unpopular amongst people",
                            Descr = "",
                            TraitApproved = true,
                            IsUnique = false,
                            TraitType = SD.TraitType_Race,
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
                        };
                    }
                    raceElf.Traits.Add(unpopularTrait);
                    contex.Races.Add(raceElf);
                    contex.SaveChanges();
                }

                /// TRAITS CHARACTER
                TraitCharacter trait = null;
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == "Lame") == null)
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
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == "Beautiful") == null)
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
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == "Genius") == null)
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
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == "Ugly") == null)
                {
                    trait = new TraitCharacter()
                    {
                        Name = "Ugly",
                        Descr = "This character is not pleasant to the eye",
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
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == "Wrathful") == null)
                {
                    trait = new TraitCharacter()
                    {
                        Name = "Wrathful",
                        Descr =  "This character's outbursts of anger are frequent and violent (sometimes makes will checks)",
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
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }

                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == "Ambidextrous") == null)
                {
                    trait = new TraitCharacter()
                    {
                        Name = "Ambidextrous",
                        Descr = "Able to use the right and left hands equally well",
                        TraitApproved = true,
                        IsUnique = false,
                        TraitType = SD.TraitType_Character,
                        TraitValue = 4,
                    };
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                /// TRAITS TEMPORARY STATES
                
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Stunned) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Stunned, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Stumbled) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Stumbled, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Snatched) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Snatched, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Disarmed) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Disarmed, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }

                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Blinded) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Blinded, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Unaware) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Unaware, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Invisible) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Invisible, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Surrounded) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Surrounded, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }

                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Unbalanced) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Unbalanced, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Cautious) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Cautious, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.FullDefence) == null)
                {
                    trait = StateSeeder.GetState(States.Names.FullDefence, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Bleeding) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Bleeding, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Unconscious) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Unconscious, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }
                if (contex.TraitsCharacter.FirstOrDefault(u => u.Name == States.Names.Dead) == null)
                {
                    trait = StateSeeder.GetState(States.Names.Dead, true);
                    contex.TraitsCharacter.Add(trait);
                    contex.SaveChanges();
                }

                /// TRAITS PROFESSION (PASSIVE)
                TraitProfession traitProf = null;
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.WizardMagic+ " 1") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.WizardMagic + " 1",
                        Descr = "Able to cast magic with wizard pool for cantrips and spells of 1st circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 1,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.WizardMagic + " 2") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.WizardMagic + " 2",
                        Descr = "Able to cast magic with wizard pool for spells of 2nd and 3rd circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 2,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.WizardMagic + " 3") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.WizardMagic + " 3",
                        Descr = "Able to cast magic with wizard pool for spells of 4th circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 3,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.WizardMagic + " 4") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.WizardMagic + " 4",
                        Descr = "Able to cast magic with wizard pool for spells of 5th and 6th circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 4,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.WizardMagic + " 5") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.WizardMagic + " 5",
                        Descr = "Able to cast magic with wizard pool for spells of 7th and 8th circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 5,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.WizardMagic + " 6") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.WizardMagic + " 6",
                        Descr = "Able to cast magic with wizard pool for spells of 9th circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 6,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.WizardMagic + " 7") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.WizardMagic + " 7",
                        Descr = "Able to cast magic with wizard pool for spells of mythic level",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 7,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }

                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.SorcererMagic  + " 1") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.SorcererMagic + " 1",
                        Descr = "Able to cast magic with sorcerer pool for cantrips and spells of 1st circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 1,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.SorcererMagic + " 2") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.SorcererMagic + " 2",
                        Descr = "Able to cast magic with sorcerer pool for spells of 2nd and 3rd circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 2,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.SorcererMagic + " 3") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.SorcererMagic + " 3",
                        Descr = "Able to cast magic with sorcerer pool for spells of 4th circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 3,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.SorcererMagic + " 4") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.SorcererMagic + " 4",
                        Descr = "Able to cast magic with sorcerer pool for spells of 5th and 6th circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 4,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.SorcererMagic + " 5") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.SorcererMagic + " 5",
                        Descr = "Able to cast magic with sorcerer pool for spells of 7th and 8th circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 5,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.SorcererMagic + " 6") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.SorcererMagic + " 6",
                        Descr = "Able to cast magic with sorcerer pool for spells of 9th circle",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 6,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.SorcererMagic + " 7") == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.SorcererMagic + " 7",
                        Descr = "Able to cast magic with sorcerer pool for spells of mythic level",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 7,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }

                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.DoubleWeaponFighting) == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.DoubleWeaponFighting,
                        Descr = "Allows character to fight with two weapons without penalties, if the second weapon is light",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 1,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.GreaterDoubleWeaponFighting) == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.GreaterDoubleWeaponFighting,
                        Descr = "Allows character to fight with two weapons without penalties. Requires 14 strength",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 3,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }
                if (contex.TraitsProfession.FirstOrDefault(u => u.Name == SD.ProfessionSkills.MightyGrip) == null)
                {
                    traitProf = new TraitProfession()
                    {
                        Name = SD.ProfessionSkills.MightyGrip,
                        Descr = "Allows character to wield two-handed weapon with one hand. Requires 20 strength",
                        TraitApproved = true,
                        IsUnique = false,
                        Level = 2,
                        TraitType = SD.TraitType_Profession,
                    };
                    contex.TraitsProfession.Add(traitProf);
                    contex.SaveChanges();
                }

                /// EQUIPMENT

                Equipment item;

                if (contex.Equipment.FirstOrDefault(u => u.Name == "Bandage") == null)
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
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == "Wound balm") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Wound balm",
                        EquipmentType = SD.EquipmentType.Other,
                        Description = "Helps with healing wounds. 20 doses, +2 to tending wounds. 1 dose for light and medium wounds, 2 for heavy, and 4 for critical",
                        ShortDescr = "Helps with healing wounds. 20 doses, +2 to tending wounds. 1 dose for light and medium wounds, 2 for heavy, and 4 for critical",
                        Weight = 1.0m,
                        Price = 0.1m,
                        IsApproved = true,

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == "Rope") == null)
                {
                    item = new Equipment()
                    {
                        Name = "Rope",
                        EquipmentType = SD.EquipmentType.Other,
                        Description = "20 feet of strong rope",
                        ShortDescr = "20 feet of strong rope",

                        Weight = 5.0m,
                        Price = 0.1m,
                        IsApproved = true,

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                

                // ARMORS
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicArmors.LightLeatherArmor) == null)
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = -4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 1,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicArmors.LeatherScaleArmor) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicArmors.LeatherScaleArmor,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = -2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicArmors.SteelScaleArmor || u.Name == "Steal scale armor") == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicArmors.SteelScaleArmor,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 6,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = 1,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicArmors.HalfPlate) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicArmors.HalfPlate,
                        EquipmentType = SD.EquipmentType.Body,
                        Description = "Good protection of solid steel",
                        ShortDescr = "Good protection of solid steel",
                        Weight = 30.0m,
                        Price = 50.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 8,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicArmors.FullPlate) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicArmors.FullPlate,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 10,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Armor,
                                    },
                                     new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 6,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                //SHIELDS
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicShields.WoodenBuckler) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicShields.WoodenBuckler,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 1,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }

                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicShields.MetalBuckler) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicShields.MetalBuckler,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicShields.WoodenShield) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicShields.WoodenShield,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
               
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicShields.MetalShield) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicShields.MetalShield,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }

                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicShields.BigWoodenShield) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicShields.BigWoodenShield,
                        EquipmentType = SD.EquipmentType.Shield,
                        RelatedSkill = SD.SpecialSkills.Melee.Shields,
                        Description = "Bigger for better protection",
                        ShortDescr = "Bigger for better protection",
                        Weight = 10.0m,
                        Price = 2.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicShields.BigMetalShield) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicShields.BigMetalShield,
                        EquipmentType = SD.EquipmentType.Shield,
                        RelatedSkill = SD.SpecialSkills.Melee.Shields,
                        Description = "Bigger for better protection",
                        ShortDescr = "Bigger for better protection",
                        Weight = 10.0m,
                        Price = 9.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                     new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                      new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicShields.Pavise) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicShields.Pavise,
                        EquipmentType = SD.EquipmentType.Shield,
                        RelatedSkill = SD.SpecialSkills.Melee.Shields,
                        Description = "Large stationary shield. −2 to fight tests; can provide full cover in some situations",
                        ShortDescr = "Large stationary shield",
                        Weight = 18.0m,
                        Price = 15.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 7,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDefenceBonus,
                                    },
                                    new Bonus{
                                        BonusValue = 8,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPenalty,
                                    },
                                    new Bonus{
                                        BonusValue = 50,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Durability,
                                    },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                // WEAPONS MELEE
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Dagger) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Dagger,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                    new Bonus{
                                        BonusValue = 0,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Light,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    }
                                }
                            }
                        },

                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.LongSword) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.LongSword,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Parrying,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Disarming,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.BattleAxe) == null)
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDestructive,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    }
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Pickaxe) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Pickaxe,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDestructive,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Mace) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Mace,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Stunning,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Morningstar) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Morningstar,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Unarmed) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Unarmed,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Punches, kicks, bites, and other unarmed attacks",
                        ShortDescr = "Punches, kicks, bites, and other unarmed attacks",
                        RelatedSkill = SD.SpecialSkills.Melee.Unarmed,
                        IsTwoHanded = true,
                        Weight = 0.0m,
                        Price = 0.0m,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Weak,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.ShorSpear) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.ShorSpear,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Long,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Rapier) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Rapier,
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
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Parrying,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }

                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.TwoHandedFlail) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.TwoHandedFlail,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Heavy and slow, but easy to knock down an opponent",
                        ShortDescr = "Heavy and slow, but easy to knock down an opponent",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 10.0m,
                        Price = 3.0m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Heavy,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 7,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Stumbling,
                                    },
                                    new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Warhammer) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Warhammer,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Powerful weapon that can easily stun the enemy",
                        ShortDescr = "Powerful weapon that can easily stun the enemy",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 15.0m,
                        Price = 7.0m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Heavy,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 8,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Stunning,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Greataxe) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Greataxe,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "A truly devastating weapon",
                        ShortDescr = "A truly devastating weapon",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 10.0m,
                        Price = 7.0m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Heavy,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDestructive,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Poleaxe) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Poleaxe,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Axe head on long pole",
                        ShortDescr = "Axe head on long pole",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 12.0m,
                        Price = 7.0m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Heavy,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 6,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ShieldDestructive,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Long,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Sarissa) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Sarissa,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Very long spear",
                        ShortDescr = "Very long spear",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 15.0m,
                        Price = 3.0m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Slow,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 3,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Long,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                // WEAPONS RANGED
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsShooting.CrossbowLight) == null)
                { 
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsShooting.CrossbowLight,
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Easy to use and slow to reload",
                        ShortDescr = "Easy to use and slow to reload",
                        RelatedSkill = SD.SpecialSkills.Shooting.Crossbows,
                        Weight = 6.0m,
                        Price = 10.0m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                    new Bonus{
                                        BonusValue = 1,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Reload,
                                    },
                                    new Bonus{
                                        BonusValue = 20,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsShooting.CrossbowHeavy) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsShooting.CrossbowHeavy,
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Powerful but slow",
                        ShortDescr = "Powerful but slow",
                        RelatedSkill = SD.SpecialSkills.Shooting.Crossbows,
                        Weight = 6.0m,
                        Price = 10.0m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 5,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Reload,
                                    },
                                    new Bonus{
                                        BonusValue = 30,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsShooting.BowSimple) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsShooting.BowSimple,
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Common tool of hunters",
                        ShortDescr = "Common tool of hunters",
                        RelatedSkill = SD.SpecialSkills.Shooting.Bows,
                        Weight = 3.0m,
                        Price = 1.0m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.ArmorPiercing,
                                    },
                                    new Bonus{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },

                                    new Bonus{
                                        BonusValue = 20,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsShooting.Longbow) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsShooting.Longbow,
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Military archers' primary weapon",
                        ShortDescr = "Military archers' primary weapon",
                        RelatedSkill = SD.SpecialSkills.Shooting.Bows,
                        Weight = 3.0m,
                        Price = 1.0m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Fast,
                                    },

                                    new Bonus{
                                        BonusValue = 40,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsShooting.Slingshot) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsShooting.Slingshot,
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Simple but effective",
                        ShortDescr = "Simple but effective",
                        RelatedSkill = SD.SpecialSkills.Shooting.Slingshots,
                        Weight = 0.5m,
                        Price = 0.5m,
                        IsTwoHanded = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{
                                        BonusValue = 4,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Devastating,
                                    },

                                    new Bonus{
                                        BonusValue = 40,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Range,
                                    },
                                }
                            }
                        },
                        IsApproved = true,
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Khopesh) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Khopesh,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Curved exotic blade",
                        ShortDescr = "Curved exotic blade",
                        RelatedSkill = SD.SpecialSkills.Melee.Swords,
                        Weight = 3.0m,
                        Price = 4.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Fast },
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Parrying },
                                    new Bonus{ BonusValue = 5, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Disarming },
                                    new Bonus{ BonusValue = 5, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Stumbling },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Whip) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Whip,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Flexible reach weapon",
                        ShortDescr = "Flexible reach weapon",
                        RelatedSkill = SD.SpecialSkills.Melee.Light,
                        Weight = 1.0m,
                        Price = 2.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Range },
                                    new Bonus{ BonusValue = 4, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Fast },
                                    new Bonus{ BonusValue = 5, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Snatching },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.WarClub) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.WarClub,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Heavy bludgeoning weapon",
                        ShortDescr = "Heavy bludgeoning weapon",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 3.0m,
                        Price = 2.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 5, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Stunning },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Devastating },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Bardiche) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Bardiche,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Heavy pole axe with long reach",
                        ShortDescr = "Heavy pole axe with long reach",
                        RelatedSkill = SD.SpecialSkills.Melee.Heavy,
                        Weight = 8.0m,
                        Price = 8.0m,
                        IsTwoHanded = true,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 4, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Heavy },
                                    new Bonus{ BonusValue = 4, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Slow },
                                    new Bonus{ BonusValue = 6, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.ShieldDestructive },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Range },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.LanceCavalry) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.LanceCavalry,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Lance designed for mounted combat",
                        ShortDescr = "Lance designed for mounted combat",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 6.0m,
                        Price = 5.0m,
                        IsTwoHanded = true,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Long },
                                    new Bonus{ BonusValue = 5, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.ArmorPiercing },
                                    new Bonus{ BonusValue = 8, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Stumbling },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.LanceInfantry) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.LanceInfantry,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Heavy infantry lance",
                        ShortDescr = "Heavy infantry lance",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 10.0m,
                        Price = 4.0m,
                        IsTwoHanded = true,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Heavy },
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Slow },
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.ArmorPiercing },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Greatsword) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Greatsword,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Large two-handed sword",
                        ShortDescr = "Large two-handed sword",
                        RelatedSkill = SD.SpecialSkills.Melee.Swords,
                        Weight = 6.0m,
                        Price = 12.0m,
                        IsTwoHanded = true,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Heavy },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Slow },
                                    new Bonus{ BonusValue = 4, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Devastating },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Parrying },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Halberd) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Halberd,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Versatile polearm with axe blade",
                        ShortDescr = "Versatile polearm with axe blade",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 10.0m,
                        Price = 9.0m,
                        IsTwoHanded = true,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Heavy },
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Slow },
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Devastating },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.ShieldDestructive },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Stumbling },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Long },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Billhook) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Billhook,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Hooked polearm for pulling and tripping",
                        ShortDescr = "Hooked polearm for pulling and tripping",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 7.0m,
                        Price = 5.0m,
                        IsTwoHanded = true,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Long },
                                    new Bonus{ BonusValue = 4, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Snatching },
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Stumbling },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.MainGauche) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.MainGauche,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Parrying dagger for off-hand use",
                        ShortDescr = "Parrying dagger for off-hand use",
                        RelatedSkill = SD.SpecialSkills.Melee.Fencing,
                        Weight = 1.0m,
                        Price = 3.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 0, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Light },
                                    new Bonus{ BonusValue = 4, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Parrying },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsMelee.Staff) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsMelee.Staff,
                        EquipmentType = SD.EquipmentType.WeaponMelee,
                        Description = "Simple wooden staff",
                        ShortDescr = "Simple wooden staff",
                        RelatedSkill = SD.SpecialSkills.Melee.Polearms,
                        Weight = 3.0m,
                        Price = 0.5m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Parrying },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Long },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Weak },
                                    new Bonus{ BonusValue = 3, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Stunning },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsShooting.Musket) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsShooting.Musket,
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Black powder firearm",
                        ShortDescr = "Black powder firearm",
                        RelatedSkill = SD.SpecialSkills.Shooting.Firearms,
                        Weight = 8.0m,
                        Price = 25.0m,
                        IsTwoHanded = true,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 5, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Devastating },
                                    new Bonus{ BonusValue = 5, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Fast },
                                    new Bonus{ BonusValue = 4, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Reload },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }
                if (contex.Equipment.FirstOrDefault(u => u.Name == SD.BasicWeaponsShooting.Javelin) == null)
                {
                    item = new Equipment()
                    {
                        Name = SD.BasicWeaponsShooting.Javelin,
                        EquipmentType = SD.EquipmentType.WeaponRanged,
                        Description = "Thrown spear",
                        ShortDescr = "Thrown spear",
                        RelatedSkill = SD.SpecialSkills.Shooting.Javelins,
                        Weight = 2.0m,
                        Price = 1.0m,
                        IsApproved = true,
                        Traits = new List<TraitEquipment>()
                        {
                            new TraitEquipment(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<Bonus>()
                                {
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.ArmorPiercing },
                                    new Bonus{ BonusValue = 2, FeatureType = SD.FeatureWeaponQuality, FeatureName = SD.WeaponQuality.Devastating },
                                }
                            }
                        },
                    };
                    contex.Equipment.Add(item);
                    contex.SaveChanges();
                }

                var renamedEquipment = new Dictionary<string, string>
                {
                    ["Steal scale armor"] = SD.BasicArmors.SteelScaleArmor,
                    ["Fists"] = SD.BasicWeaponsMelee.Unarmed,
                };

                foreach (var (oldName, newName) in renamedEquipment)
                {
                    var equipment = contex.Equipment.FirstOrDefault(e => e.Name == oldName);
                    if (equipment is null)
                        continue;

                    var existingWithNewName = contex.Equipment.FirstOrDefault(e => e.Name == newName && e.Id != equipment.Id);
                    if (existingWithNewName is not null)
                        contex.Equipment.Remove(equipment);
                    else
                        equipment.Name = newName;
                }

                foreach (var name in SD.BasicEquipment.All)
                {
                    var items = contex.Equipment.Where(e => e.Name == name).OrderBy(e => e.Id).ToList();
                    if (items.Count <= 1)
                        continue;

                    var keeper = items[0];
                    foreach (var duplicate in items.Skip(1))
                    {
                        foreach (var slot in contex.EquipmentSlots.Where(s => s.EquipmentID == duplicate.Id))
                            slot.EquipmentID = keeper.Id;

                        contex.Equipment.Remove(duplicate);
                    }
                }

                var correctedEquipmentDescriptions = new Dictionary<string, (string Description, string ShortDescr)>
                {
                    [SD.BasicArmors.HalfPlate] = ("Good protection of solid steel", "Good protection of solid steel"),
                    ["Wound balm"] = (
                        "Helps with healing wounds. 20 doses, +2 to tending wounds. 1 dose for light and medium wounds, 2 for heavy, and 4 for critical",
                        "Helps with healing wounds. 20 doses, +2 to tending wounds. 1 dose for light and medium wounds, 2 for heavy, and 4 for critical"),
                    ["Rope"] = ("20 feet of strong rope", "20 feet of strong rope"),
                    [SD.BasicShields.BigWoodenShield] = ("Bigger for better protection", "Bigger for better protection"),
                    [SD.BasicShields.BigMetalShield] = ("Bigger for better protection", "Bigger for better protection"),
                    [SD.BasicWeaponsMelee.Unarmed] = ("Punches, kicks, bites, and other unarmed attacks", "Punches, kicks, bites, and other unarmed attacks"),
                    [SD.BasicWeaponsShooting.CrossbowHeavy] = ("Powerful but slow", "Powerful but slow"),
                };

                foreach (var (name, descriptions) in correctedEquipmentDescriptions)
                {
                    var equipment = contex.Equipment.FirstOrDefault(e => e.Name == name);
                    if (equipment is null)
                        continue;

                    equipment.Description = descriptions.Description;
                    equipment.ShortDescr = descriptions.ShortDescr;
                }

                foreach (var seed in LanguageSeeder.GetAll())
                {
                    var existing = contex.Languages.FirstOrDefault(l => l.Name == seed.Name);
                    if (existing is null)
                    {
                        contex.Languages.Add(new Language
                        {
                            Name = seed.Name,
                            Description = seed.Description,
                            Category = seed.Category,
                            Script = seed.Script,
                            Index = seed.Index,
                            IsApproved = seed.IsApproved
                        });
                    }
                    else
                    {
                        existing.Description = seed.Description;
                        existing.Category = seed.Category;
                        existing.Script = seed.Script;
                        existing.Index = seed.Index;
                        existing.IsApproved = seed.IsApproved;
                    }
                }

                var renamedLanguages = new Dictionary<string, string>
                {
                    ["Imperial (Taledin)"] = "taledin",
                    ["Solimian"] = "Solime",
                    ["Old Vorgoweld Speech"] = "stara mowa Vorgoweldów",
                    ["Dalyjan Dialect"] = "dialekt dalyjczyków",
                    ["Felvgardic"] = "felvgardzki",
                    ["Suochian"] = "suochiański",
                    ["Bingdonian"] = "bingdoński",
                    ["Classical Gu-ilanian"] = "klasyczny Gu-ilański",
                    ["Nindu"] = "nindu",
                    ["Rashi"] = "rashi",
                };

                foreach (var (oldName, newName) in renamedLanguages)
                {
                    var oldLanguage = contex.Languages.FirstOrDefault(l => l.Name == oldName);
                    if (oldLanguage is null)
                        continue;

                    var replacement = contex.Languages.FirstOrDefault(l => l.Name == newName);
                    if (replacement is not null && replacement.Id != oldLanguage.Id)
                    {
                        foreach (var character in contex.Characters.Include(c => c.Languages)
                                     .Where(c => c.Languages != null && c.Languages.Any(l => l.Id == oldLanguage.Id)))
                        {
                            character.Languages!.Remove(oldLanguage);
                            if (character.Languages.All(l => l.Id != replacement.Id))
                                character.Languages.Add(replacement);
                        }

                        contex.Languages.Remove(oldLanguage);
                    }
                    else
                    {
                        oldLanguage.Name = newName;
                    }
                }

                foreach (var language in contex.Languages.Where(l => l.Category == "Imperial").ToList())
                    language.Category = SD.Languages.CategoryHuman;

                contex.SaveChanges();

                await EnsureGenericDemoBaronAsync(contex);

                await SeniorHousesSeeder.EnsureForAllBaroniesAsync(contex);
                await OrganizationsSeeder.EnsureForAllBaroniesAsync(contex);
                await VassalFamilySeeder.EnsureForAllBaroniesAsync(contex);
                await NeighborsSeeder.FixGroupNamesAsync(contex);
                await MarchMapSeeder.EnsureInitializedAsync(contex);
                await SeatPurposeTemplatesSeeder.EnsureDefaultsAsync(contex);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static async Task EnsureGenericDemoBaronAsync(ApplicationDbContext contex)
        {
            const string sourceName = "Thaddeus Direbolt";
            const string demoName = "Aldric Emberfall";

            if (await contex.Characters.AnyAsync(c => c.NPCName == demoName))
                return;

            var source = await contex.Characters
                .AsNoTracking()
                .Include(c => c.Attributes)
                .Include(c => c.BaseSkills)
                .Include(c => c.SpecialSkills)
                .Include(c => c.EquipmentSlots)
                .Include(c => c.Languages)
                .FirstOrDefaultAsync(c => c.NPCName == sourceName);

            if (source is null)
            {
                // Production/fresh databases have no hand-made "Thaddeus Direbolt" to clone from,
                // so build the demo baron from the standard code-embedded character template instead.
                // Without this the demo endpoints throw "source character not found" → HTTP 500.
                var templated = await BuildDemoBaronFromTemplateAsync(contex, demoName);
                ApplyDemoBaronSkillProfile(templated);
                contex.Characters.Add(templated);
                await contex.SaveChangesAsync();
                return;
            }

            var created = new Character
            {
                UserName = source.UserName,
                Relation = source.Relation,
                NPCName = demoName,
                Description =
                    "A former adventurer who now rules as a practical baron. Veteran of border expeditions, " +
                    "strong in melee command and battle leadership, while still keeping a blacksmith's and " +
                    "craftsman's discipline.",
                Age = source.Age,
                ImageUrl = source.ImageUrl,
                IconUrl = source.IconUrl,
                NPCType = SD.NPCType.Duke,
                AttributePoints = source.AttributePoints,
                CurrentExpPoints = source.CurrentExpPoints,
                UsedExpPoints = source.UsedExpPoints,
                TraitBalance = source.TraitBalance,
                RaceId = source.RaceId,
                IsApproved = true,
                ProfessionId = source.ProfessionId,
                WeaponSet = source.WeaponSet,
                DateNumber = source.DateNumber,
                Attributes = source.Attributes?
                    .OrderBy(a => a.Index)
                    .Select(a => new DA_DataAccess.CharacterClasses.Attribute
                    {
                        Name = a.Name,
                        FeatureType = a.FeatureType,
                        Index = a.Index,
                        BaseBonus = a.BaseBonus,
                        RaceBonus = a.RaceBonus,
                        GearBonus = a.GearBonus,
                        TraitBonus = a.TraitBonus,
                        OtherBonuses = a.OtherBonuses,
                        TempBonuses = a.TempBonuses,
                        HealthBonus = a.HealthBonus,
                    })
                    .ToList(),
                BaseSkills = source.BaseSkills?
                    .OrderBy(s => s.Index)
                    .Select(s => new BaseSkill
                    {
                        Name = s.Name,
                        FeatureType = s.FeatureType,
                        Index = s.Index,
                        BaseBonus = s.BaseBonus,
                        RaceBonus = s.RaceBonus,
                        GearBonus = s.GearBonus,
                        TraitBonus = s.TraitBonus,
                        OtherBonuses = s.OtherBonuses,
                        TempBonuses = s.TempBonuses,
                        HealthBonus = s.HealthBonus,
                        RelatedAttribute1 = s.RelatedAttribute1,
                        RelatedAttribute2 = s.RelatedAttribute2,
                    })
                    .ToList(),
                SpecialSkills = source.SpecialSkills?
                    .OrderBy(s => s.RelatedBaseSkillName)
                    .ThenBy(s => s.Index)
                    .ThenBy(s => s.Name)
                    .Select(s => new SpecialSkill
                    {
                        Name = s.Name,
                        FeatureType = s.FeatureType,
                        Index = s.Index,
                        BaseBonus = s.BaseBonus,
                        RaceBonus = s.RaceBonus,
                        GearBonus = s.GearBonus,
                        TraitBonus = s.TraitBonus,
                        OtherBonuses = s.OtherBonuses,
                        TempBonuses = s.TempBonuses,
                        HealthBonus = s.HealthBonus,
                        RelatedAttribute1 = s.RelatedAttribute1,
                        RelatedAttribute2 = s.RelatedAttribute2,
                        RelatedBaseSkillName = s.RelatedBaseSkillName,
                        ChosenAttribute = s.ChosenAttribute,
                        Editable = s.Editable,
                    })
                    .ToList(),
                EquipmentSlots = source.EquipmentSlots?
                    .OrderBy(s => s.Id)
                    .Select(s => new EquipmentSlot
                    {
                        Count = s.Count,
                        EquipmentID = s.EquipmentID,
                        IsEquipped = s.IsEquipped,
                        SlotType = s.SlotType,
                    })
                    .ToList(),
            };

            if (source.Languages is { Count: > 0 })
            {
                var languageIds = source.Languages.Select(l => l.Id).ToList();
                created.Languages = await contex.Languages
                    .Where(l => languageIds.Contains(l.Id))
                    .ToListAsync();
            }

            ApplyDemoBaronSkillProfile(created);

            contex.Characters.Add(created);
            await contex.SaveChangesAsync();
        }

        /// <summary>
        /// Builds the persistent demo-baron source character from the standard code-embedded
        /// character template (<see cref="DA_Models.CharacterSeeder"/>) so it exists on any
        /// database, including fresh production ones that lack the hand-made clone source.
        /// </summary>
        private static async Task<Character> BuildDemoBaronFromTemplateAsync(ApplicationDbContext contex, string demoName)
        {
            var created = new Character
            {
                UserName = SD.DemoBaronTemplateUserName,
                NPCName = demoName,
                Description =
                    "A former adventurer who now rules as a practical baron. Veteran of border expeditions, " +
                    "strong in melee command and battle leadership, while still keeping a blacksmith's and " +
                    "craftsman's discipline.",
                Age = 42,
                NPCType = SD.NPCType.Duke,
                IsApproved = true,
                Attributes = DA_Models.CharacterSeeder.GetAttributes().Values
                    .OrderBy(a => a.Index)
                    .Select(a => new DA_DataAccess.CharacterClasses.Attribute
                    {
                        Name = a.Name,
                        Index = a.Index,
                    })
                    .ToList(),
                BaseSkills = DA_Models.CharacterSeeder.GetBaseSkills()
                    .OrderBy(s => s.Index)
                    .Select(s => new BaseSkill
                    {
                        Name = s.Name,
                        Index = s.Index,
                        RelatedAttribute1 = s.RelatedAttribute1,
                        RelatedAttribute2 = s.RelatedAttribute2,
                    })
                    .ToList(),
                SpecialSkills = DA_Models.CharacterSeeder.GetSpecialSkills()
                    .OrderBy(s => s.RelatedBaseSkillName)
                    .ThenBy(s => s.Index)
                    .ThenBy(s => s.Name)
                    .Select(s => new SpecialSkill
                    {
                        Name = s.Name,
                        Index = s.Index,
                        RelatedAttribute1 = s.RelatedAttribute1,
                        RelatedAttribute2 = s.RelatedAttribute2,
                        RelatedBaseSkillName = s.RelatedBaseSkillName,
                        ChosenAttribute = s.ChosenAttribute,
                    })
                    .ToList(),
            };

            var common = (await contex.Languages.ToListAsync())
                .FirstOrDefault(l => SD.Languages.IsCommon(l.Name));
            if (common is not null)
                created.Languages = new List<Language> { common };

            return created;
        }

        private static void ApplyDemoBaronSkillProfile(Character character)
        {
            if (character.Attributes is not null)
            {
                BoostAttribute(character.Attributes, "Strength", 4);
                BoostAttribute(character.Attributes, "Charisma", 5);
                BoostAttribute(character.Attributes, "Intelligence", 1);
                BoostAttribute(character.Attributes, "Willpower", 1);
            }

            if (character.BaseSkills is not null)
            {
                SetBaseSkill(character.BaseSkills, "Melee", 6);
                SetBaseSkill(character.BaseSkills, "Athletics", 5);
                SetBaseSkill(character.BaseSkills, "Talk", 5);
                SetBaseSkill(character.BaseSkills, "Knowledge", 5);
                SetBaseSkill(character.BaseSkills, "Craft", 6);
            }

            if (character.SpecialSkills is not null)
            {
                SetSpecialSkill(character.SpecialSkills, "Acting", 1);
                SetSpecialSkill(character.SpecialSkills, "Animals care", 1);
                SetSpecialSkill(character.SpecialSkills, "Armor", 3);
                SetSpecialSkill(character.SpecialSkills, "Balance", 1);
                SetSpecialSkill(character.SpecialSkills, "Bluff", 3);
                SetSpecialSkill(character.SpecialSkills, "Climbing", 1);
                SetSpecialSkill(character.SpecialSkills, "Diplomacy", 3);
                SetSpecialSkill(character.SpecialSkills, "Dirty tricks", 2);
                SetSpecialSkill(character.SpecialSkills, "Dodge", 3);
                SetSpecialSkill(character.SpecialSkills, "Geography", 2);
                SetSpecialSkill(character.SpecialSkills, "Geology and mining", 2);
                SetSpecialSkill(character.SpecialSkills, "Handcraft", 4);
                SetSpecialSkill(character.SpecialSkills, "Hearing", 1);
                SetSpecialSkill(character.SpecialSkills, "Heraldry", 1);
                SetSpecialSkill(character.SpecialSkills, "History and religion", 2);
                SetSpecialSkill(character.SpecialSkills, "Shields", 3);
                SetSpecialSkill(character.SpecialSkills, "Strategy and tactics", 5);
                SetSpecialSkill(character.SpecialSkills, "Inspire", 4);
                SetSpecialSkill(character.SpecialSkills, "Intimidate", 3);
                SetSpecialSkill(character.SpecialSkills, "Jumping", 1);
                SetSpecialSkill(character.SpecialSkills, "Lifting", 2);
                SetSpecialSkill(character.SpecialSkills, "Linguistics", 5);
                SetSpecialSkill(character.SpecialSkills, "Mathematics and logic", 2);
                SetSpecialSkill(character.SpecialSkills, "Metallurgy and blacksmithing", 5);
                SetSpecialSkill(character.SpecialSkills, "Pain Resistance", 4);
                SetSpecialSkill(character.SpecialSkills, "Persuasion", 4);
                SetSpecialSkill(character.SpecialSkills, "Pickpocketing", 4);
                SetSpecialSkill(character.SpecialSkills, "Plants and mushrooms", 3);
                SetSpecialSkill(character.SpecialSkills, "Public speech", 4);
                SetSpecialSkill(character.SpecialSkills, "Riding", 3);
                SetSpecialSkill(character.SpecialSkills, "Running", 2);
                SetSpecialSkill(character.SpecialSkills, "Sense motives", 1);
                SetSpecialSkill(character.SpecialSkills, "Sense of direction", 2);
                SetSpecialSkill(character.SpecialSkills, "Sneak", 3);
                SetSpecialSkill(character.SpecialSkills, "Survival", 3);
                SetSpecialSkill(character.SpecialSkills, "Swimming", 1);
                SetSpecialSkill(character.SpecialSkills, "Swords and sabres", 5);
                SetSpecialSkill(character.SpecialSkills, "Tend wounds", 2);
                SetSpecialSkill(character.SpecialSkills, "Threatening", 3);
                SetSpecialSkill(character.SpecialSkills, "Torture", 2);
                SetSpecialSkill(character.SpecialSkills, "Trade", 2);
                SetSpecialSkill(character.SpecialSkills, "Trapping", 1);
                SetSpecialSkill(character.SpecialSkills, "Vigilance", 2);
                SetSpecialSkill(character.SpecialSkills, "Wilderness knowledge", 3);
                SetSpecialSkill(character.SpecialSkills, "Wrestling", 4);
            }
        }

        private static void BoostAttribute(IEnumerable<DA_DataAccess.CharacterClasses.Attribute> attributes, string name, int add)
        {
            var attr = attributes.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
            if (attr is null)
                return;
            attr.BaseBonus += add;
        }

        private static void SetBaseSkill(IEnumerable<BaseSkill> skills, string name, int value)
        {
            var skill = skills.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            if (skill is null)
                return;
            skill.BaseBonus = value;
        }

        private static void SetSpecialSkill(IEnumerable<SpecialSkill> skills, string name, int value)
        {
            var skill = skills.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            if (skill is null)
                return;
            skill.BaseBonus = value;
        }

        /// <summary>Reseed catalog when empty, legacy Polish, or outdated farm/mine/quarry/sawmill names.</summary>
        private static async Task SyncBuildingTemplateCatalogAsync(ApplicationDbContext ctx)
        {
            var needsReseed = !await ctx.BuildingTemplates.AnyAsync()
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Kind == "Budynek"
                    || t.Kind == "Ulepszenie"
                    || t.Name == "Akademia Wojskowa"
                    || t.Name == "Cmentarzysko"
                    || t.Name == "Poor Farms"
                    || t.Name == "Common Farms"
                    || t.Name == "Fertile Farms"
                    || t.Name == "Iron Mine"
                    || t.Name == "Copper Mine"
                    || t.Name == "Mine - Copper"
                    || t.Name == "Mine - hard metals"
                    || t.Name == "Mine - luxury metals"
                    || t.Name == "Mine - precious gems (common)"
                    || t.Name == "Granite Quarry"
                    || t.Name == "Common Quarry"
                    || t.Name == "Obsidian Quarry"
                    || t.Name == "Dagonite Mine"
                    || t.Name == "Mine - Dagonite"
                    || t.Name == "Soft Metal Mine"
                    || t.Name == "Hard Metal Mine"
                    || t.Name == "Luxury Metal Mine"
                    || t.Name == "Silver Mine"
                    || t.Name == "Gold Mine"
                    || t.Name == "Precious Gem Mine (Luxury)"
                    || t.Name == "Precious Gem Mine (Common)"
                    || t.Name == "Ironwood Sawmill"
                    || t.Name == "Common Sawmill"
                    || t.Name == "Farm - very poor"
                    || t.Name == "Farm - poor"
                    || t.Name == "Farm - regular fertility"
                    || t.Name == "Farm - very fertile"
                    || t.Name == "Farm - exceptionally fertile")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Clay pit")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Mine - Salt")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Sawmill - Elven alder")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Sawmill - Shipbuilding wood")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Mine - Sulfur")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Quarry - Tarnit")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Mine - Dagoferryt")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Farm - poor fertility")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Farm")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Farm - fertile")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Farm - bountiful")
                || await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Mine - Dagonite")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Market Square")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Steward's Building")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Marketplace")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Inn")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Brewery")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Candlemaker")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Farm (Dye plant)")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Smithy")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Forge")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Armorer")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Plate Workshop")
                || !await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Horse Stud (regular)")
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Sheep pastures"
                    && (t.Description == null || !t.Description.Contains("Requires trade access to Sheep")))
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Pastures (cattle)"
                    && (t.Description == null || !t.Description.Contains("Requires trade access to Cattle")))
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Horse Stud (regular)"
                    && (t.Description == null || !t.Description.Contains("Requires trade access to Horses")))
                // Trade-building lore: Pastures / Vineyard cost explanations + Produces lines.
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Pastures (cattle)"
                    && (t.Description == null || !t.Description.Contains("herd itself must be bought")))
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Apiary"
                    && (t.Description == null || !t.Description.Contains("Produces Honey & Wax")))
                || await ctx.BuildingTemplates.AnyAsync(t => t.Name == "Small Brewery")
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Fishing Pier"
                    && (t.EffectAdditiveJson == null || !t.EffectAdditiveJson.Contains("-0.5,10")))
                // Farm PPB sync: defense -1 + treasury 8/10/15/20 (was wrongly treasury -1).
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Farm - poor fertility"
                    && (t.EffectAdditiveJson == null || !t.EffectAdditiveJson.Contains("-1,8")))
                // Hunter's Lodge: Army 2 was wrongly stored as Corruption 2.
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Hunter's Lodge"
                    && (t.EffectAdditiveJson == null
                        || t.EffectAdditiveJson.Contains("2,0,0,0,0,3,5")
                        || !t.EffectAdditiveJson.Contains("0,0,0,0,0,0,3,5")))
                // Barracks: Defense 20, Treasury −60; Town Garrison: Defense 6, Treasury −25 + guard/upgrade text.
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Barracks"
                    && (t.EffectAdditiveJson == null || !t.EffectAdditiveJson.Contains("20,-60")))
                || await ctx.BuildingTemplates.AnyAsync(t =>
                    t.Name == "Town Garrison"
                    && (t.EffectAdditiveJson == null
                        || !t.EffectAdditiveJson.Contains("6,-25")
                        || t.Description == null
                        || !t.Description.Contains("city guard unit")));

            if (!needsReseed)
                return;

            foreach (var building in await ctx.BaronyBuildings.Where(b => b.TemplateId != null).ToListAsync())
                building.TemplateId = null;

            ctx.BuildingTemplates.RemoveRange(await ctx.BuildingTemplates.ToListAsync());
            ctx.BuildingTemplates.AddRange(BuildingTemplateSeeder.CreateAll());
            await ctx.SaveChangesAsync();
        }
    }
}
