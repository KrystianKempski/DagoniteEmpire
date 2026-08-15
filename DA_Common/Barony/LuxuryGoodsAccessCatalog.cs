namespace DA_Common.Barony
{
    /// <summary>How widely luxury goods reach the barony (Blackhammer strategic-goods tiers).</summary>
    public sealed class LuxuryGoodsAccessTier
    {
        public required string Key { get; init; }
        public required string NameEn { get; init; }
        public required string BonusDisplay { get; init; }
        public required string Description { get; init; }
        public PpbVector BonusAdditive { get; init; } = new();
        public int SortOrder { get; init; }
    }

    public static class LuxuryGoodsAccessCatalog
    {
        public const string Insufficient = "insufficient";
        public const string Basic = "basic";
        public const string Expanded = "expanded";
        public const string Variety = "variety";
        public const string March = "march";
        public const string World = "world";

        public static string DefaultKey => Basic;

        public static IReadOnlyList<LuxuryGoodsAccessTier> All { get; } = Build();

        public static LuxuryGoodsAccessTier Find(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return All.First(t => t.Key == Basic);
            return All.FirstOrDefault(t => string.Equals(t.Key, key, StringComparison.OrdinalIgnoreCase))
                   ?? All.First(t => t.Key == Basic);
        }

        private static IReadOnlyList<LuxuryGoodsAccessTier> Build()
        {
            static PpbVector Fx(
                decimal? economy = null, decimal? loyalty = null, decimal? stability = null,
                decimal? culture = null, decimal? magic = null)
            {
                var v = new PpbVector();
                if (economy.HasValue) v[Ppb.Economy] = economy.Value;
                if (loyalty.HasValue) v[Ppb.Loyalty] = loyalty.Value;
                if (stability.HasValue) v[Ppb.Stability] = stability.Value;
                if (culture.HasValue) v[Ppb.Culture] = culture.Value;
                if (magic.HasValue) v[Ppb.Magic] = magic.Value;
                return v;
            }

            return new LuxuryGoodsAccessTier[]
            {
                new()
                {
                    Key = Insufficient,
                    NameEn = "Niewystarczające towary luksusowe",
                    BonusDisplay = "−2 Stabilność, −2 Lojalność",
                    Description = "Baronii brakuje wygód, których oczekują elity i mieszczanie — szemranie i niepokój są tego skutkiem.",
                    BonusAdditive = Fx(loyalty: -2, stability: -2),
                    SortOrder = 0,
                },
                new()
                {
                    Key = Basic,
                    NameEn = "Podstawowe artykuły pierwszej potrzeby",
                    BonusDisplay = "—",
                    Description = "Tylko codzienne produkty i proste towary. Brak rynku dóbr luksusowych.",
                    BonusAdditive = Fx(),
                    SortOrder = 1,
                },
                new()
                {
                    Key = Expanded,
                    NameEn = "Rozszerzona oferta",
                    BonusDisplay = "+2 Stabilność, +2 Lojalność",
                    Description = "Szerszy wybór wygód i drobnych luksusów z pobliskiego handlu.",
                    BonusAdditive = Fx(loyalty: 2, stability: 2),
                    SortOrder = 2,
                },
                new()
                {
                    Key = Variety,
                    NameEn = "Znaczna różnorodność",
                    BonusDisplay = "+4 Stabilność, +4 Lojalność, +4 Ekonomia",
                    Description = "Kupcy przywożą prawdziwy wybór wykwintnych towarów; na rynkach czuć dobrobyt.",
                    BonusAdditive = Fx(economy: 4, loyalty: 4, stability: 4),
                    SortOrder = 3,
                },
                new()
                {
                    Key = March,
                    NameEn = "Z całej Marchii",
                    BonusDisplay = "+7 Stabilność, +7 Lojalność, +7 Ekonomia, +7 Kultura",
                    Description = "Luksusy Wschodniej Marchii przybywają regularnie — moda, uczty i dobra prestiżowe.",
                    BonusAdditive = Fx(economy: 7, loyalty: 7, stability: 7, culture: 7),
                    SortOrder = 4,
                },
                new()
                {
                    Key = World,
                    NameEn = "Z całego świata!",
                    BonusDisplay = "+10 Stabilność, +10 Lojalność, +10 Ekonomia, +10 Kultura, +10 Magia",
                    Description = "Odległe przyprawy, jedwabie i cudowności: baronia smakuje szerszego świata.",
                    BonusAdditive = Fx(economy: 10, loyalty: 10, stability: 10, culture: 10, magic: 10),
                    SortOrder = 5,
                },
            };
        }
    }
}
