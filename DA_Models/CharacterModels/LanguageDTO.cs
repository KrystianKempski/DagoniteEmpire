namespace DA_Models.CharacterModels
{
    public class LanguageDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public int Index { get; set; }
        public bool IsApproved { get; set; }
    }
}
