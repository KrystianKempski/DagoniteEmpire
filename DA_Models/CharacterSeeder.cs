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
                    //CharacterId= charId,
                    Name = "Strength",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new AttributeDTO()
                {
                   //CharacterId= charId,
                    Name = "Dexterity",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new AttributeDTO()
                {
                    //CharacterId= charId,
                    Name = "Endurance",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new AttributeDTO()
                {
                    //CharacterId= charId,
                    Name = "Intelligence",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new AttributeDTO()
                {
                    //CharacterId= charId,
                    Name = "Instinct",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new AttributeDTO()
                {
                    //CharacterId= charId,
                    Name = "Willpower",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new AttributeDTO()
                {
                    //CharacterId= charId,
                    Name = "Charisma",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
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
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Shooting",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Acrobatics",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Sleight of hands",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Athletics",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Talk",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Deceit",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Knowledge",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Craft",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Survival",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Animal handle",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Medicine",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                new BaseSkillDTO()
                {
                    Name = "Special skill",
                    BaseBonus = 0,
                    GearBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = 0,
                    TempBonuses = 0,
                },
                
            };
            return baseSkills;
        }
    }
}
