﻿using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Models.CharacterModels
{
    public class CharacterDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter name of character")]
        public string? UserName { get; set; }
        public string? NPCName { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Range(16, 300, ErrorMessage = "Age must be between 16 and 300 years")]
        public int Age { get; set; }
        public string? ImageUrl { get; set; } = "/upload/portraits/def-char-img.webp";
        public string? NPCType { get; set; }
        public int AttributePoints { get; set; }
        public int CurrentExpPoints { get; set; }
        public int UsedExpPoints { get; set; }

        public int TraitBalance { get; set; } = 0;

        public string RaceName { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;

        public ICollection<AttributeDTO>? Attributes { get; set; }
        public ICollection<BaseSkillDTO>? BaseSkills { get; set; }
        public ICollection<SpecialSkillDTO>? SpecialSkills { get; set; }
        public ICollection <TraitAdvDTO>? TraitsAdv { get; set; }
        public ICollection<EquipmentDTO>? Equipment { get; set; }
        public ICollection<CampaignDTO>? Campaigns { get; set; }
        public ICollection<PostDTO>? Posts { get; set; }
        public ICollection<ChapterDTO>? Chapters { get; set; }
        public int RaceId { get; set; } = 0;
        public int ProfessionId { get; set; } = 0;
        public bool IsApproved { get; set; } = false;
        public EquipmentDTO Armor { get; set; }
        public EquipmentDTO Weapon { get; set; }

        public override string ToString() => NPCName;
    }
}
