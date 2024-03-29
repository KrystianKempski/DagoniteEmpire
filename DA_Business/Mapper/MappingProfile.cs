﻿using AutoMapper;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Business.Mapper
{
    internal class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<Attribute, AttributeDTO>().ReverseMap();

            CreateMap<Character, CharacterDTO>().ReverseMap();
            CreateMap<BaseSkill, BaseSkillDTO>().ReverseMap();
            CreateMap<SpecialSkill, SpecialSkillDTO>().ReverseMap();
            CreateMap<Trait, TraitDTO>().ReverseMap();
            CreateMap<Bonus, BonusDTO>().ReverseMap();

        }
    }
}
