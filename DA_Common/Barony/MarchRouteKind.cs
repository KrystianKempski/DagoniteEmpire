namespace DA_Common.Barony
{
    public static class MarchRouteKind
    {
        public const string Road = "road";
        public const string River = "river";

        public static readonly string[] All = { Road, River };

        public static string Label(string? kind) =>
            string.Equals(kind, River, StringComparison.OrdinalIgnoreCase) ? "Rzeka" : "Droga";

        public static string Normalize(string? kind) =>
            string.Equals(kind, River, StringComparison.OrdinalIgnoreCase) ? River : Road;
    }
}
