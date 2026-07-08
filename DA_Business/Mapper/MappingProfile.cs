using AutoMapper;
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
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            // Global null handling configuration
            AllowNullCollections = true;
            AllowNullDestinationValues = true;

            CreateMap<Attribute, AttributeDTO>().ReverseMap();

            // Character: ignore navigation properties when mapping DTO -> Entity
            // Ignore CurrentDate - it's a computed property in DTO that wraps DateNumber
            CreateMap<Character, CharacterDTO>()
                .ForMember(dest => dest.CurrentDate, opt => opt.Ignore());
            CreateMap<CharacterDTO, Character>()
                .ForMember(dest => dest.Race, opt => opt.Ignore())
                .ForMember(dest => dest.Profession, opt => opt.Ignore())
                .ForMember(dest => dest.Campaigns, opt => opt.Ignore())
                .ForMember(dest => dest.Chapters, opt => opt.Ignore());

            CreateMap<BaseSkill, BaseSkillDTO>().ReverseMap();
            CreateMap<SpecialSkill, SpecialSkillDTO>().ReverseMap();
            CreateMap<TraitCharacter, TraitCharacterDTO>().ReverseMap();
            CreateMap<Mob,MobDTO>().ReverseMap();
            CreateMap<TraitRace, TraitRaceDTO>().ReverseMap();
            CreateMap<TraitEquipment, TraitEquipmentDTO>().ReverseMap();
            CreateMap<TraitProfession, TraitProfessionDTO>().ReverseMap();
            CreateMap<Bonus, BonusDTO>().ReverseMap();
            CreateMap<Race, RaceDTO>().ReverseMap();
            CreateMap<Language, LanguageDTO>().ReverseMap();
            CreateMap<Profession, ProfessionDTO>().ReverseMap();
            CreateMap<Equipment, EquipmentDTO>().ReverseMap();
            CreateMap<EquipmentSlot, EquipmentSlotDTO>().ReverseMap();
            
            // Chapter: ignore navigation properties when mapping DTO -> Entity
            CreateMap<Chapter, ChapterDTO>();
            CreateMap<ChapterDTO, Chapter>()
                .ForMember(dest => dest.Campaign, opt => opt.Ignore());

            // Post: ignore navigation properties when mapping DTO -> Entity  
            CreateMap<Post, PostDTO>();
            CreateMap<PostDTO, Post>()
                .ForMember(dest => dest.Character, opt => opt.Ignore())
                .ForMember(dest => dest.Chapter, opt => opt.Ignore());

            CreateMap<Campaign, CampaignDTO>().ReverseMap();
            CreateMap<BattlePhase, BattlePhaseDTO>().ReverseMap();
            CreateMap<BattleEvent, BattleEventDTO>().ReverseMap();
            CreateMap<SpellCircle, SpellCircleDTO>().ReverseMap();
            
            // Wound: ignore navigation property when mapping DTO -> Entity
            CreateMap<Wound, WoundDTO>();
            CreateMap<WoundDTO, Wound>()
                .ForMember(dest => dest.Character, opt => opt.Ignore());
            
            CreateMap<Wound, ConditionDTO>().ReverseMap();
            
            // WealthRecord: ignore computed CurrentDate property (wraps DateNumber)
            CreateMap<WealthRecord, WealthRecordDTO>()
                .ForMember(dest => dest.CurrentDate, opt => opt.Ignore());
            CreateMap<WealthRecordDTO, WealthRecord>();
        }
    }
}
