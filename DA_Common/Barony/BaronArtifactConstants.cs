namespace DA_Common.Barony
{
    /// <summary>Kinds of trophies / treasures / artifacts on the Baron Card.</summary>
    public readonly struct BaronArtifactKind
    {
        public const string Weapon = "Weapon";
        public const string Armor = "Armor";
        public const string DisplayPiece = "Display piece";
        public const string Painting = "Painting";
        public const string Book = "Book";
        public const string Trophy = "Trophy";
        public const string Tapestry = "Tapestry";
        public const string Other = "Other";

        public static readonly string[] All =
        {
            Weapon, Armor, DisplayPiece, Painting, Book, Trophy, Tapestry, Other,
        };
    }

    /// <summary>How an artifact was obtained.</summary>
    public readonly struct BaronArtifactOrigin
    {
        public const string Bought = "Bought";
        public const string Acquired = "Acquired";
        public const string Stolen = "Stolen";
        public const string Inherited = "Inherited";
        public const string Won = "Won";
        public const string Gift = "Gift";
        public const string Other = "Other";

        public static readonly string[] All =
        {
            Bought, Acquired, Stolen, Inherited, Won, Gift, Other,
        };
    }

    /// <summary>Max artifacts per Lord's Seat chamber by size (Huge = unlimited).</summary>
    public static class BaronArtifactCapacity
    {
        public static int? MaxForSize(string? sizeCategory) => sizeCategory switch
        {
            SeatRoomSizeCategory.Small => 3,
            SeatRoomSizeCategory.Medium => 6,
            SeatRoomSizeCategory.Large => 12,
            SeatRoomSizeCategory.Huge => null,
            _ => 3,
        };

        public static string LimitLabel(string? sizeCategory)
        {
            var max = MaxForSize(sizeCategory);
            return max is int n ? $"{n}" : "unlimited";
        }
    }
}
