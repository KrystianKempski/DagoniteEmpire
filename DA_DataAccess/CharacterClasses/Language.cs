using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.CharacterClasses
{
    public class Language
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public int Index { get; set; }
        public bool IsApproved { get; set; }

        public ICollection<Character>? Characters { get; set; }
    }
}
