namespace DA_Common.Barony
{
    /// <summary>
    /// Podstawowe Parametry Baronii (PPB). Kolejność wartości odpowiada indeksom w <see cref="PpbVector"/>.
    /// </summary>
    public enum Ppb
    {
        Food = 0,
        Economy = 1,
        Production = 2,
        Loyalty = 3,
        Stability = 4,
        Law = 5,
        Corruption = 6,
        Science = 7,
        Magic = 8,
        Culture = 9,
        Intelligence = 10,
        Defense = 11,
        Treasury = 12,
    }

    /// <summary>Metadane pojedynczego PPB (nazwa PL, skrót, czy kumuluje się między turami).</summary>
    public sealed class PpbInfo
    {
        public Ppb Key { get; init; }
        public string NamePl { get; init; } = string.Empty;
        public string ShortPl { get; init; } = string.Empty;

        /// <summary>Czy wartość przenosi się między turami (akumulator), np. skarbiec.</summary>
        public bool IsCumulative { get; init; }

        public string Code => Key.ToString();
    }

    /// <summary>Statyczny katalog wszystkich PPB w kanonicznej kolejności kolumn.</summary>
    public static class PpbCatalog
    {
        public const int Count = 13;

        public static readonly IReadOnlyList<PpbInfo> All = new List<PpbInfo>
        {
            new() { Key = Ppb.Food,         NamePl = "Wyżywienie",   ShortPl = "Wyż.",   IsCumulative = false },
            new() { Key = Ppb.Economy,      NamePl = "Ekonomia",     ShortPl = "Ekon.",  IsCumulative = false },
            new() { Key = Ppb.Production,    NamePl = "Produkcja",    ShortPl = "Prod.",  IsCumulative = false },
            new() { Key = Ppb.Loyalty,      NamePl = "Lojalność",    ShortPl = "Loj.",   IsCumulative = false },
            new() { Key = Ppb.Stability,    NamePl = "Stabilność",   ShortPl = "Stab.",  IsCumulative = false },
            new() { Key = Ppb.Law,          NamePl = "Prawo",        ShortPl = "Prawo",  IsCumulative = false },
            new() { Key = Ppb.Corruption,   NamePl = "Korupcja",     ShortPl = "Korup.", IsCumulative = false },
            new() { Key = Ppb.Science,      NamePl = "Nauka",        ShortPl = "Nauka",  IsCumulative = false },
            new() { Key = Ppb.Magic,        NamePl = "Magia",        ShortPl = "Magia",  IsCumulative = false },
            new() { Key = Ppb.Culture,      NamePl = "Kultura",      ShortPl = "Kult.",  IsCumulative = false },
            new() { Key = Ppb.Intelligence, NamePl = "Wywiad",       ShortPl = "Wyw.",   IsCumulative = false },
            new() { Key = Ppb.Defense,      NamePl = "Obrona",       ShortPl = "Obr.",   IsCumulative = false },
            new() { Key = Ppb.Treasury,     NamePl = "Skarb/Złoto",  ShortPl = "Złoto",  IsCumulative = true  },
        };

        public static PpbInfo Info(Ppb p) => All[(int)p];

        public static string Name(Ppb p) => All[(int)p].NamePl;

        public static string Short(Ppb p) => All[(int)p].ShortPl;
    }
}
