using DA_DataAccess.CharacterClasses;
using System.ComponentModel.DataAnnotations;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Models.CharacterModels
{
    public class CharacterDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter name of character")]
        public string? UserName { get; set; }
        public string? NPCName { get; set; }
        public string? Description { get; set; }

        [Range(16, 300, ErrorMessage = "Age must be between 16 and 300 years")]
        public int Age { get; set; }
        public string? ImageUrl { get; set; }
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
        public int RaceId { get; set; } = 0;
        public int ProfessionId { get; set; } = 0;

    }
}
