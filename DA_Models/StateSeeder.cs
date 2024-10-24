using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models
{
    public class StateSeeder
    {
        public static TraitCharacterDTO GetStateDTO(string name, bool approved = false, int duration = 0, int characterId = 0)
        {
            return new TraitCharacterDTO(GetState(name, approved, duration, characterId));
        } 

        public static TraitCharacter GetState(string name, bool approved = false, int duration = 0,int characterId =0)
        {
            TraitCharacter trait;
            switch (name)
            {
                default:
                    throw new Exception("need name for character");
                case States.Names.NoTurn:
                    trait = new TraitCharacter(true)
                    {
                        Name = States.Names.NoTurn,
                        Descr = "This character can't do anything this turn",
                        TraitApproved = false,
                        IsUnique = false,
                        TraitType = SD.TraitType_Temporary,
                        Level = (int)States.Level.NoTurn,
                        TraitValue = 1,
                        CharacterId = characterId,
                    };
                    break;
                case States.Names.HalfTurn:
                    trait = new TraitCharacter(true)
                    {
                        Name = States.Names.HalfTurn,
                        Descr = "This character still have one action this turn",
                        TraitApproved = false,
                        IsUnique = false,
                        TraitType = SD.TraitType_Temporary,
                        Level = (int)States.Level.HalfTurn,
                        TraitValue = 1,
                        CharacterId = characterId,
                    };
                    break;
                case States.Names.Dead:
                    trait = new TraitCharacter(true)
                    {
                        Name = States.Names.Dead,
                        Descr = "This character is dead...",
                        TraitApproved = approved,
                        IsUnique = false,
                        TraitType = SD.TraitType_Temporary,
                        Level = (int)States.Level.Dead,
                        TraitValue = duration > 0 ? duration : 999,
                        CharacterId = characterId,
                    };
                    break;
                case States.Names.Unconscious:
                    trait = new TraitCharacter(true)
                    {
                        Name = States.Names.Unconscious,
                        Descr = "This character is excluded from any fight",
                        TraitApproved = approved,
                        IsUnique = false,
                        TraitType = SD.TraitType_Temporary,
                        Level = (int)States.Level.Unconscious,
                        TraitValue = duration > 0 ? duration : 99,
                        CharacterId = characterId,
                    };
                    break;
                case States.Names.Stunned:
                    trait = new TraitCharacter(true)
                        {
                            Name = States.Names.Stunned,
                            Descr = "This character is dazed, it cannot perform any actions and its defence is impaired",
                            TraitApproved = approved,
                            IsUnique = false,
                            Level = (int)States.Level.Stunned,
                            TraitType = SD.TraitType_Temporary,
                            TraitValue = duration > 0 ? duration : 99,
                            CharacterId = characterId,
                    };
                    break;
                case States.Names.Stumbled:
                    trait = new TraitCharacter(true)
                        {
                            Name = States.Names.Stumbled,
                            Descr = "This character lost its balance, and lies on the ground. To get up it needs to use action (or two if in heavy armor)",
                            TraitApproved = approved,
                            IsUnique = false,
                            TraitType = SD.TraitType_Temporary,
                            Level = (int)States.Level.Stumbled,
                            TraitValue = duration > 0 ? duration : 99,
                            CharacterId = characterId,
                    };
                    break;
                case States.Names.Snatched:
                        trait = new TraitCharacter(true)
                        {
                            Name = States.Names.Snatched,
                            Descr = "This character was captured. It cannot move or use captured limb until it gets free",
                            TraitApproved = approved,
                            IsUnique = false,
                            Level = (int)States.Level.Snatched,
                            TraitType = SD.TraitType_Temporary,
                            TraitValue = duration > 0 ? duration : 99,
                            CharacterId = characterId,
                        };
                    break;
                case States.Names.Disarmed:
                        trait = new TraitCharacter(true)
                        {
                            Name = States.Names.Disarmed,
                            Descr = "This character lost it primary weapon",
                            TraitApproved = approved,
                            IsUnique = false,
                            TraitType = SD.TraitType_Temporary,
                            Level = (int)States.Level.Disarmed,
                            TraitValue = duration > 0 ? duration : 99,
                            CharacterId = characterId,
                        };
                        break;
               case States.Names.Blinded:
                        trait = new TraitCharacter(true)
                        {
                            Name = States.Names.Blinded,
                            Descr = "This character lost its sight. This causes penalty to defence equal 8, unless there is other way to see incoming attacks. This character can attack with penalty equal to 5",
                            TraitApproved = approved,
                            IsUnique = false,
                            Level = (int)States.Level.Blinded,
                            TraitType = SD.TraitType_Temporary,
                            TraitValue = duration > 0 ? duration : 99,
                            CharacterId = characterId,
                        };
                        break;
               case States.Names.Unaware:
                    trait = new TraitCharacter(true)
                    {
                        Name = States.Names.Unaware,
                        Descr = "This character is unaware of it's enemies. This causes penalty to defence equal to 10. Unaware characters become aware after first attack",
                        TraitApproved = approved,
                        IsUnique = false,
                        TraitType = SD.TraitType_Temporary,
                        Level = (int)States.Level.Unaware,
                        TraitValue = duration > 0 ? duration : 1,
                        CharacterId = characterId,
                    };
                    break;
                case States.Names.Invisible:
                trait = new TraitCharacter(true)
                {
                    Name = States.Names.Invisible,
                    Descr = "This character cannot be seen, but enemies are aware of its presence. This causes bonus to attack equal to 5, and bonus defence equal to 5",
                    TraitApproved = approved,
                    IsUnique = false,
                    TraitType = SD.TraitType_Temporary,
                    Level = (int)States.Level.Invisible,
                    TraitValue = duration > 0 ? duration : 99,
                    CharacterId = characterId,
                };
                    break;
                case States.Names.Flanking:
                trait = new TraitCharacter(true)
                {
                    Name = States.Names.Flanking,
                    Descr = "This character attack someone from the back, when they are busy fighting with someone else. This gives bonus to attack equal to 3, and opponent cannot use shield",
                    TraitApproved = approved,
                    IsUnique = false,
                    TraitType = SD.TraitType_Temporary,
                    Level = (int)States.Level.Flanking,
                    TraitValue = duration > 0 ? duration : 99,
                    CharacterId = characterId,
                };
                    break;
                case States.Names.Surrounded:
                trait = new TraitCharacter(true)
                {
                    Name = States.Names.Surrounded,
                    Descr = "This character is surrounded by enemies. For every other enemy attacking this character there is added penalty to defence equal to 2",
                    TraitApproved = approved,
                    IsUnique = false,
                    TraitType = SD.TraitType_Temporary,
                    Level = (int)States.Level.Surrounded,
                    TraitValue = duration > 0 ? duration : 1,
                    CharacterId = characterId,
                };
                    break;
                case States.Names.Unbalanced:
                trait = new TraitCharacter(true)
                {
                    Name = States.Names.Unbalanced,
                    Descr = "This character lost its balance. For remainging turn he have penalty of 7 to defence",
                    TraitApproved = approved,
                    IsUnique = false,
                    TraitType = SD.TraitType_Temporary,
                    Level = (int)States.Level.Unbalanced,
                    TraitValue = duration > 0 ? duration : 1,
                    CharacterId = characterId,
                };
                    break;
                case States.Names.Cautious:
                trait = new TraitCharacter(true)
                {
                    Name = States.Names.Cautious,
                    Descr = "This character went in semi-defencive state. It gets bonus to all kinds of defence equal to 2",
                    TraitApproved = approved,
                    IsUnique = false,
                    TraitType = SD.TraitType_Temporary,
                    Level = (int)States.Level.Cautious,
                    TraitValue = duration > 0 ? duration : 1,
                    CharacterId = characterId,
                };
                    break;
                case States.Names.FullDefence:
                trait = new TraitCharacter(true)
                {
                    Name = States.Names.FullDefence,
                    Descr = "This character went in full defence. It gets bonus to all kinds of defence equal to 5, but it cannot attack or make any actions",
                    TraitApproved = approved,
                    IsUnique = false,
                    TraitType = SD.TraitType_Temporary,
                    Level = (int)States.Level.FullDefence,
                    TraitValue = duration > 0 ? duration : 1,
                    CharacterId = characterId,
                };
                    break;
                case States.Names.Bleeding:
                trait = new TraitCharacter(true)
                {
                    Name = States.Names.Bleeding,
                    Descr = "This character is seriously bleeding. It gets one wound every turn, untill 10 round or the wound is taken care of",
                    TraitApproved = approved,
                    IsUnique = false,
                    TraitType = SD.TraitType_Temporary,
                    Level = (int)States.Level.Bleeding,
                    TraitValue = duration > 0 ? duration : 10,
                    CharacterId = characterId,
                };
                    break;
            }
            return trait;
        }

    }
}
