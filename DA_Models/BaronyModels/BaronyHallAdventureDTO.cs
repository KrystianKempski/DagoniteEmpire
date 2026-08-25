namespace DA_Models.BaronyModels
{
    public class BaronyHallAdventureDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IconPath { get; set; } = "icons/bookmarklet.svg";
        public string LinkUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
