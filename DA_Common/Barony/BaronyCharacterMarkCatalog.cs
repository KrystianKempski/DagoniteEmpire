namespace DA_Common.Barony
{
    public sealed record CharacterMarkIconOption(string Key, string Label);

    public sealed record CharacterMarkColorOption(string Key, string Label, string Hex);

    /// <summary>Player-assigned visual mark for a relation or catalog lord.</summary>
    public readonly record struct BaronyCharacterMark(string IconKey, string ColorKey)
    {
        public bool IsSet =>
            BaronyCharacterMarkCatalog.IsValidIcon(IconKey)
            && BaronyCharacterMarkCatalog.IsValidColor(ColorKey);
    }

    public static class BaronyCharacterMarkCatalog
    {
        public static readonly CharacterMarkIconOption[] Icons =
        {
            new("vip", "Kluczowa osoba"),
            new("flag", "Na oku"),
            new("danger", "Zagrożenie"),
            new("ally", "Sojusznik"),
            new("faction", "Frakcja"),
            new("deal", "Kontakt / układ"),
        };

        public static readonly CharacterMarkColorOption[] Colors =
        {
            new("gold", "Złoty", "#c4a35a"),
            new("red", "Czerwony", "#b54a4a"),
            new("green", "Zielony", "#4a7a55"),
            new("blue", "Niebieski", "#4a6a8a"),
            new("purple", "Fioletowy", "#6b5b95"),
        };

        public static bool IsValidIcon(string? key) =>
            !string.IsNullOrWhiteSpace(key)
            && Icons.Any(i => string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase));

        public static bool IsValidColor(string? key) =>
            !string.IsNullOrWhiteSpace(key)
            && Colors.Any(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));

        public static bool IsSet(string? iconKey, string? colorKey) =>
            IsValidIcon(iconKey) && IsValidColor(colorKey);

        public static BaronyCharacterMark? Normalize(string? iconKey, string? colorKey)
        {
            if (!IsSet(iconKey, colorKey))
                return null;

            var icon = Icons.First(i => string.Equals(i.Key, iconKey, StringComparison.OrdinalIgnoreCase)).Key;
            var color = Colors.First(c => string.Equals(c.Key, colorKey, StringComparison.OrdinalIgnoreCase)).Key;
            return new BaronyCharacterMark(icon, color);
        }

        public static string? ColorHex(string? colorKey) =>
            Colors.FirstOrDefault(c => string.Equals(c.Key, colorKey, StringComparison.OrdinalIgnoreCase))?.Hex;

        public static string Tooltip(BaronyCharacterMark mark)
        {
            var icon = Icons.FirstOrDefault(i => string.Equals(i.Key, mark.IconKey, StringComparison.OrdinalIgnoreCase));
            var color = Colors.FirstOrDefault(c => string.Equals(c.Key, mark.ColorKey, StringComparison.OrdinalIgnoreCase));
            if (icon is null || color is null)
                return "Oznaczony";
            return $"{icon.Label} · {color.Label}";
        }

        public static bool Same(BaronyCharacterMark a, BaronyCharacterMark b) =>
            string.Equals(a.IconKey, b.IconKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(a.ColorKey, b.ColorKey, StringComparison.OrdinalIgnoreCase);

        public static string TooltipMany(IReadOnlyList<BaronyCharacterMark> marks)
        {
            if (marks.Count == 0)
                return "Oznacz postać";
            if (marks.Count == 1)
                return Tooltip(marks[0]);
            return string.Join("\n", marks.Select(Tooltip));
        }
    }
}
