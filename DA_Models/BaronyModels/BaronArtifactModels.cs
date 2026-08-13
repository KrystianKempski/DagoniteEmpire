namespace DA_Models.BaronyModels
{
    public class BaronArtifactDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = DA_Common.Barony.BaronArtifactKind.Other;
        public string Origin { get; set; } = DA_Common.Barony.BaronArtifactOrigin.Acquired;
        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }
        public int? SeatRoomId { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }
}
