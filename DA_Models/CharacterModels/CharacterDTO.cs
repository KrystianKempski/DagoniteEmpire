using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_Models.ComponentModels;
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
        public Relation Relation { get; set; } = Relation.Teammate;
        public string? NPCName { get; set; }
        public string Description { get; set; } = string.Empty;

        [Range(16, 300, ErrorMessage = "Age must be between 16 and 300 years")]
        public int Age { get; set; } = 16;
        public string? ImageUrl { get; set; } = "/images/def-char-img.webp";
        public string? NPCType { get; set; } = SD.NPCType.Hero;
        public int AttributePoints { get; set; }
        public int CurrentExpPoints { get; set; }
        public int UsedExpPoints { get; set; }

        public int TraitBalance { get; set; } = 0;

        public string RaceName { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;

        public ICollection<AttributeDTO>? Attributes { get; set; }
        public ICollection<BaseSkillDTO>? BaseSkills { get; set; }
        public ICollection<SpecialSkillDTO>? SpecialSkills { get; set; }
        //public ICollection<TraitCharacterDTO>? TraitsCharacter { get; set; }
        public ICollection<EquipmentSlotDTO>? EquipmentSlots { get; set; }
        public ICollection<CampaignDTO>? Campaigns { get; set; }
        public ICollection<PostDTO>? Posts { get; set; }
        public ICollection<ChapterDTO>? Chapters { get; set; }
        public ICollection<WoundDTO>? Wounds { get; set; }
        public int RaceId { get; set; } = 0;
        public int ProfessionId { get; set; } = 0;
        public bool IsApproved { get; set; } = false;
        public int WeaponSet { get; set; } = 0;
        public int DateNumber { get; set; } = 1;
        public DateModel CurrentDate { 
            set
            {
                DateNumber = value.AllDays;
            }
            get =>  new DateModel(DateNumber);
        }
        public decimal Gold { get; set; } = 0.0m;
        public override string ToString() => NPCName;

    }

}
