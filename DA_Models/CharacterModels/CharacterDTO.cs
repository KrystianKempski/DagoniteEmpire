using DA_DataAccess.CharacterClasses;
using System.ComponentModel.DataAnnotations;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Models.CharacterModels
{
    public class CharacterDTO
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter name of character")]

        public string? UserName { get; set; }
        public string? NPCName { get; set; }
        public string? Description { get; set; }
        public string? Class { get; set; }
        public string? Race { get; set; }
        [Range(16, 300, ErrorMessage = "Age must be between 16 and 300 years")]
        public int Age { get; set; }
        public string? ImageUrl { get; set; }
        public string? NPCType { get; set; }
        public int AttributePoints { get; set; }
        public int CurrentExpPoints { get; set; }
        public int UsedExpPoints { get; set; }

        public int TraitBalance { get; set; }

        public ICollection<AttributeDTO>? Attributes { get; set; }
        public ICollection<BaseSkillDTO>? BaseSkills { get; set; }
        public ICollection<SpecialSkillDTO>? SpecialSkills { get; set; }
    }
}
