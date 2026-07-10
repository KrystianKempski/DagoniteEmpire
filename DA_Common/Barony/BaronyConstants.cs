namespace DA_Common.Barony
{
    /// <summary>Rodzaje urzędów baronii.</summary>
    public readonly struct OfficeType
    {
        public const string Baron = "Baron";
        public const string Chancellor = "Kanclerz";
        public const string GuardCaptain = "Kapitan Straży";
        public const string Steward = "Ekonom";
        public const string Custom = "Inny";

        public static readonly string[] Core = { Chancellor, GuardCaptain, Steward };
        public static readonly string[] All = { Baron, Chancellor, GuardCaptain, Steward, Custom };
    }

    /// <summary>Grupy społeczne, których relacje wpływają na PPB.</summary>
    public readonly struct SocialGroup
    {
        public const string Nobility = "Szlachta";
        public const string Burghers = "Mieszczaństwo";
        public const string Peasants = "Chłopi";

        public static readonly string[] All = { Nobility, Burghers, Peasants };
    }

    /// <summary>Poziomy relacji z grupą społeczną. 0 = obojętność.</summary>
    public readonly struct RelationLevel
    {
        public const int Hostile = -3;
        public const int Unfriendly = -2;
        public const int Cool = -1;
        public const int Indifferent = 0;
        public const int Warm = 1;
        public const int Friendly = 2;
        public const int Devoted = 3;

        public static string Name(int level) => level switch
        {
            <= -3 => "Wroga",
            -2 => "Niechętna",
            -1 => "Chłodna",
            0 => "Obojętna",
            1 => "Przychylna",
            2 => "Przyjazna",
            _ => "Oddana",
        };
    }

    /// <summary>Bazowy rodzaj terenu pola baronii.</summary>
    public readonly struct TerrainBaseType
    {
        public const string Plains = "Równiny";
        public const string Hills = "Wzgórza";
        public const string Mountains = "Góry";

        public static readonly string[] All = { Plains, Hills, Mountains };

        public static bool SupportsFertility(string? baseType) =>
            baseType == Plains || baseType == Hills;
    }

    /// <summary>Dodatki terenu (mogą się łączyć).</summary>
    public readonly struct TerrainFeature
    {
        public const string Forest = "Las";
        public const string Coast = "Wybrzeże";
        public const string River = "Rzeka";
        public const string Wasteland = "Pustkowie";
        public const string Swamp = "Bagna";

        public static readonly string[] All = { Forest, Coast, River, Wasteland, Swamp };
    }

    /// <summary>Rodzaj wpisu katalogu budowy.</summary>
    public readonly struct BuildingKind
    {
        public const string Building = "Budynek";
        public const string Improvement = "Ulepszenie";

        public static readonly string[] All = { Building, Improvement };
    }

    /// <summary>Status projektu.</summary>
    public readonly struct ProjectStatus
    {
        public const string Draft = "Szkic";
        public const string InProgress = "W trakcie";
        public const string Completed = "Zakończony";
        public const string Cancelled = "Anulowany";

        public static readonly string[] All = { Draft, InProgress, Completed, Cancelled };
    }

    /// <summary>Kategorie źródeł na stronie Budżet (kolumna Skarb/Złoto).</summary>
    public readonly struct BudgetSource
    {
        public const string Economy = "Ekonomia";
        public const string Fief = "Leno";
        public const string Buildings = "Miasto i budynki";
        public const string Improvements = "Ulepszenia";
        public const string Decrees = "Dekrety i technologie";
        public const string Events = "Wydarzenia";
        public const string Advisors = "Doradcy";
        public const string Other = "Inne";

        public static readonly string[] Income = { Economy, Fief, Buildings, Improvements, Decrees, Events, Other };
        public static readonly string[] Expense = { Economy, Fief, Buildings, Improvements, Decrees, Events, Advisors, Other };
    }

    /// <summary>Kategorie kar i bonusów społeczności.</summary>
    public readonly struct CommunitySource
    {
        public const string Society = "Społeczeństwo";
        public const string Hunger = "Głód";
        public const string Crime = "Przestępczość";
        public const string Corruption = "Korupcja";
        public const string Unrest = "Niepokój";

        public static readonly string[] All = { Society, Hunger, Crime, Corruption, Unrest };
    }
}
