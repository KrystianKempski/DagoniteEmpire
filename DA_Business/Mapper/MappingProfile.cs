﻿using AutoMapper;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_Models.CharacterModels;
using DA_Models.ChatModels;
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
            CreateMap<TraitCharacter, TraitCharacterDTO>().ReverseMap();
            CreateMap<Mob,MobDTO>().ReverseMap();
            CreateMap<TraitRace, TraitRaceDTO>().ReverseMap();
            CreateMap<TraitEquipment, TraitEquipmentDTO>().ReverseMap();
            CreateMap<TraitProfession, TraitProfessionDTO>().ReverseMap();
            CreateMap<Bonus, BonusDTO>().ReverseMap();
            CreateMap<Race, RaceDTO>().ReverseMap();
            CreateMap<Profession, ProfessionDTO>().ReverseMap();
            CreateMap<Equipment, EquipmentDTO>().ReverseMap();
            CreateMap<EquipmentSlot, EquipmentSlotDTO>().ReverseMap();
            CreateMap<Chapter,ChapterDTO>().ReverseMap();
            CreateMap<Post, PostDTO>().ReverseMap();
            CreateMap<Campaign, CampaignDTO>().ReverseMap();
            CreateMap<BattlePhase, BattlePhaseDTO>().ReverseMap();
            CreateMap<SpellCircle, SpellCircleDTO>().ReverseMap();
            CreateMap<Wound, WoundDTO>().ReverseMap();
            CreateMap<Wound, ConditionDTO>().ReverseMap();
            CreateMap<WealthRecord, WealthRecordDTO>().ReverseMap();
        }
    }
}
