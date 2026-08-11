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

    /// <summary>Metadane pojedynczego PPB (nazwy PL/EN, skróty, czy kumuluje się między turami).</summary>
    public sealed class PpbInfo
    {
        public Ppb Key { get; init; }
        public string NamePl { get; init; } = string.Empty;
        public string ShortPl { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
        public string ShortEn { get; init; } = string.Empty;

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
            new() { Key = Ppb.Food,         NamePl = "Wyżywienie",   ShortPl = "Wyż.",   NameEn = "Food",          ShortEn = "Food",  IsCumulative = true  },
            new() { Key = Ppb.Economy,      NamePl = "Ekonomia",     ShortPl = "Ekon.",  NameEn = "Economy",       ShortEn = "Econ",  IsCumulative = false },
            new() { Key = Ppb.Production,    NamePl = "Produkcja",    ShortPl = "Prod.",  NameEn = "Production",    ShortEn = "Prod",  IsCumulative = true  },
            new() { Key = Ppb.Loyalty,      NamePl = "Lojalność",    ShortPl = "Loj.",   NameEn = "Loyalty",       ShortEn = "Loy",   IsCumulative = false },
            new() { Key = Ppb.Stability,    NamePl = "Stabilność",   ShortPl = "Stab.",  NameEn = "Stability",     ShortEn = "Stab",  IsCumulative = false },
            new() { Key = Ppb.Law,          NamePl = "Prawo",        ShortPl = "Prawo",  NameEn = "Law",           ShortEn = "Law",   IsCumulative = false },
            new() { Key = Ppb.Corruption,   NamePl = "Korupcja",     ShortPl = "Korup.", NameEn = "Corruption",    ShortEn = "Corr",  IsCumulative = false },
            new() { Key = Ppb.Science,      NamePl = "Nauka",        ShortPl = "Nauka",  NameEn = "Science",       ShortEn = "Sci",   IsCumulative = true  },
            new() { Key = Ppb.Magic,        NamePl = "Magia",        ShortPl = "Magia",  NameEn = "Magic",         ShortEn = "Mag",   IsCumulative = true  },
            new() { Key = Ppb.Culture,      NamePl = "Kultura",      ShortPl = "Kult.",  NameEn = "Culture",       ShortEn = "Cult",  IsCumulative = true  },
            new() { Key = Ppb.Intelligence, NamePl = "Wywiad",       ShortPl = "Wyw.",   NameEn = "Intelligence",  ShortEn = "Intel", IsCumulative = true  },
            new() { Key = Ppb.Defense,      NamePl = "Obrona",       ShortPl = "Obr.",   NameEn = "Defense",       ShortEn = "Def",   IsCumulative = true  },
            new() { Key = Ppb.Treasury,     NamePl = "Skarb/Złoto",  ShortPl = "Złoto",  NameEn = "Treasury/Gold", ShortEn = "Gold",  IsCumulative = true  },
        };

        public static PpbInfo Info(Ppb p) => All[(int)p];

        public static string Name(Ppb p) => All[(int)p].NamePl;

        public static string Short(Ppb p) => All[(int)p].ShortPl;

        public static string NameEnglish(Ppb p) => All[(int)p].NameEn;

        public static string ShortEnglish(Ppb p) => All[(int)p].ShortEn;
    }

    /// <summary>
    /// Cumulative resource columns for the Resources tab (display order).
    /// Food, Production, Science, Magic, Culture, Intelligence, Defense, Gold.
    /// </summary>
    public static class ResourceCatalog
    {
        public static readonly IReadOnlyList<PpbInfo> All = new List<PpbInfo>
        {
            PpbCatalog.Info(Ppb.Food),
            PpbCatalog.Info(Ppb.Production),
            PpbCatalog.Info(Ppb.Science),
            PpbCatalog.Info(Ppb.Magic),
            PpbCatalog.Info(Ppb.Culture),
            PpbCatalog.Info(Ppb.Intelligence),
            PpbCatalog.Info(Ppb.Defense),
            PpbCatalog.Info(Ppb.Treasury),
        };

        public static readonly IReadOnlySet<Ppb> Keys = new HashSet<Ppb>(All.Select(i => i.Key));

        public static bool Contains(Ppb p) => Keys.Contains(p);

        /// <summary>CSS modifier for resource accent color (e.g. "food", "gold").</summary>
        public static string ColorKey(Ppb p) => PpbVisuals.ColorKey(p);

        /// <summary>HUD / UI icon path under wwwroot.</summary>
        public static string IconUrl(Ppb p) => PpbVisuals.IconUrl(p) ?? "/icons/wheat.svg";

        /// <summary>Accent hex for masked SVG icons.</summary>
        public static string ColorHex(Ppb p) => PpbVisuals.ColorHex(p);

        /// <summary>Copy only cumulative resource keys from <paramref name="source"/>.</summary>
        public static PpbVector Slice(PpbVector? source)
        {
            var result = new PpbVector();
            if (source is null)
                return result;
            foreach (var info in All)
                result[info.Key] = source[info.Key];
            return result;
        }

        public static PpbVector Subtract(PpbVector a, PpbVector b)
        {
            var result = new PpbVector();
            foreach (var info in All)
                result[info.Key] = a[info.Key] - b[info.Key];
            return result;
        }
    }

    /// <summary>
    /// Project cost tracks: pay with Gold + Production, or with other cumulative resources,
    /// or <see cref="ProjectCostMode.Combined"/> when both tracks are required together.
    /// </summary>
    public static class ProjectCostCatalog
    {
        public static readonly IReadOnlyList<PpbInfo> GoldProduction = new List<PpbInfo>
        {
            PpbCatalog.Info(Ppb.Production),
            PpbCatalog.Info(Ppb.Treasury),
        };

        public static readonly IReadOnlyList<PpbInfo> Materials = new List<PpbInfo>
        {
            PpbCatalog.Info(Ppb.Food),
            PpbCatalog.Info(Ppb.Science),
            PpbCatalog.Info(Ppb.Magic),
            PpbCatalog.Info(Ppb.Culture),
            PpbCatalog.Info(Ppb.Intelligence),
            PpbCatalog.Info(Ppb.Defense),
        };

        public static PpbVector SliceGoldProduction(PpbVector? source)
        {
            var result = new PpbVector();
            if (source is null)
                return result;
            foreach (var info in GoldProduction)
                result[info.Key] = source[info.Key];
            return result;
        }

        public static PpbVector SliceMaterials(PpbVector? source)
        {
            var result = new PpbVector();
            if (source is null)
                return result;
            foreach (var info in Materials)
                result[info.Key] = source[info.Key];
            return result;
        }

        /// <summary>Merge both payment tracks into one requirement vector.</summary>
        public static PpbVector MergeTracks(PpbVector? goldProduction, PpbVector? materials)
        {
            var result = new PpbVector();
            foreach (var info in GoldProduction)
                result[info.Key] = goldProduction is null ? 0m : goldProduction[info.Key];
            foreach (var info in Materials)
                result[info.Key] = materials is null ? 0m : materials[info.Key];
            return result;
        }

        /// <summary>Columns that have a positive requirement across both tracks.</summary>
        public static IReadOnlyList<PpbInfo> CombinedActiveColumns(
            PpbVector? goldProduction,
            PpbVector? materials)
        {
            var cols = new List<PpbInfo>();
            foreach (var info in GoldProduction)
            {
                if (goldProduction is not null && goldProduction[info.Key] > 0m)
                    cols.Add(info);
            }
            foreach (var info in Materials)
            {
                if (materials is not null && materials[info.Key] > 0m)
                    cols.Add(info);
            }
            return cols;
        }

        public static bool HasRequirement(PpbVector? source) =>
            source is not null && GoldProduction.Concat(Materials).Any(info => source[info.Key] > 0m);

        public static void SplitLegacyCost(PpbVector legacy, out PpbVector goldProduction, out PpbVector materials)
        {
            goldProduction = SliceGoldProduction(legacy);
            materials = SliceMaterials(legacy);
        }
    }
}
