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
        public string? NPCName { get; set; }
        public string? Description { get; set; }
        public string? Class { get; set; }
        public string? Race { get; set; }
        [Range(16, 300, ErrorMessage = "Age must be between 16 and 300 years")]
        public int Age { get; set; }
        public string? ImageUrl { get; set; }

        public IEnumerable<AttributeDTO>? Attributes { get; set; }
        public IEnumerable<BaseSkillDTO>? BaseSkills { get; set; }
        public IEnumerable<SpecialSkillDTO>? SpecialSkills { get; set; }
    }
}
