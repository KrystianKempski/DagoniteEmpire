using DA_DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_DataAccess
{
    public class CharacterSeeder
    {
        ApplicationDbContext _dbContext;
        public CharacterSeeder(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {
                if (_dbContext.Characters.Any())
                {
                    foreach(var character in _dbContext.Characters)
                    {
                        if (character != null && !character.Attributes.Any())
                        {
                            var attributes = GetAttributes(character.Id);
                            _dbContext.Attributes.AddRange(attributes);
                            _dbContext.SaveChanges();

                        }
                    }
                }
            }
        }

        private IEnumerable<Attribute> GetAttributes(int charId)
        {
            var attributes = new List<Attribute>()
            {
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Strength",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Dexterity",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Endurance",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Intelligence",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Instinct",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Willpower",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Charisma",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
            };
            return attributes;
        }
    }
}
