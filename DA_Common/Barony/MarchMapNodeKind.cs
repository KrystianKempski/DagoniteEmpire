namespace DA_Common.Barony
{
    public static class MarchMapNodeKind
    {
        public const string MarchCapital = "march-capital";
        public const string LargeCity = "large-city";
        public const string City = "city";
        public const string Village = "village";

        /// <summary>Legacy values kept for older saved maps.</summary>
        public const string LordSeat = "lord-seat";
        public const string Landmark = "landmark";

        public static readonly string[] PlaceKinds =
        {
            MarchCapital,
            LargeCity,
            City,
            Village,
        };

        public static string Label(string? kind) =>
            Normalize(kind) switch
            {
                MarchCapital => "March capital",
                LargeCity => "Large city",
                Village => "Village",
                _ => "City",
            };

        public static string Normalize(string? kind)
        {
            if (string.IsNullOrWhiteSpace(kind))
                return City;

            if (string.Equals(kind, MarchCapital, StringComparison.OrdinalIgnoreCase))
                return MarchCapital;
            if (string.Equals(kind, LargeCity, StringComparison.OrdinalIgnoreCase))
                return LargeCity;
            if (string.Equals(kind, Village, StringComparison.OrdinalIgnoreCase))
                return Village;
            if (string.Equals(kind, City, StringComparison.OrdinalIgnoreCase))
                return City;
            if (string.Equals(kind, LordSeat, StringComparison.OrdinalIgnoreCase))
                return LargeCity;
            if (string.Equals(kind, Landmark, StringComparison.OrdinalIgnoreCase))
                return City;

            return City;
        }

        public static MarchMapNodeVisual Visual(string? kind)
        {
            return Normalize(kind) switch
            {
                MarchCapital => new(16, 9, 18, "march-map-marker--capital"),
                LargeCity => new(14, 7.5, 16, "march-map-marker--large-city"),
                Village => new(10, 4, 12, "march-map-marker--village"),
                _ => new(12, 5.5, 14, "march-map-marker--city"),
            };
        }
    }

    public readonly record struct MarchMapNodeVisual(
        double HitRadius,
        double DotRadius,
        double LabelYOffset,
        string MarkerClass);
}
