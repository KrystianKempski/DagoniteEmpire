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
                    NameEn = "Insufficient luxury goods",
                    BonusDisplay = "−2 Stability, −2 Loyalty",
                    Description = "The barony lacks comforts the elite and townsfolk expect — grumbling and unrest follow.",
                    BonusAdditive = Fx(loyalty: -2, stability: -2),
                    SortOrder = 0,
                },
                new()
                {
                    Key = Basic,
                    NameEn = "Basic necessities",
                    BonusDisplay = "—",
                    Description = "Everyday staples and plain goods only. No luxury market to speak of.",
                    BonusAdditive = Fx(),
                    SortOrder = 1,
                },
                new()
                {
                    Key = Expanded,
                    NameEn = "Expanded offer",
                    BonusDisplay = "+2 Stability, +2 Loyalty",
                    Description = "A wider stall of comforts and small luxuries from nearby trade.",
                    BonusAdditive = Fx(loyalty: 2, stability: 2),
                    SortOrder = 2,
                },
                new()
                {
                    Key = Variety,
                    NameEn = "Considerable variety",
                    BonusDisplay = "+4 Stability, +4 Loyalty, +4 Economy",
                    Description = "Merchants bring a real choice of fine goods; markets feel prosperous.",
                    BonusAdditive = Fx(economy: 4, loyalty: 4, stability: 4),
                    SortOrder = 3,
                },
                new()
                {
                    Key = March,
                    NameEn = "From the whole March",
                    BonusDisplay = "+7 Stability, +7 Loyalty, +7 Economy, +7 Culture",
                    Description = "Eastern March luxuries arrive regularly—fashion, feast, and status goods.",
                    BonusAdditive = Fx(economy: 7, loyalty: 7, stability: 7, culture: 7),
                    SortOrder = 4,
                },
                new()
                {
                    Key = World,
                    NameEn = "From the whole world!",
                    BonusDisplay = "+10 Stability, +10 Loyalty, +10 Economy, +10 Culture, +10 Magic",
                    Description = "Distant spices, silks, and wonders: the barony tastes of the wider world.",
                    BonusAdditive = Fx(economy: 10, loyalty: 10, stability: 10, culture: 10, magic: 10),
                    SortOrder = 5,
                },
            };
        }
    }
}
