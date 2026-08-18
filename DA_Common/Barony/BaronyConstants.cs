using DA_Common.Localization;
using Microsoft.Extensions.Localization;

namespace DA_Common.Barony
{
    /// <summary>Rodzaje urzędów baronii.</summary>
    public readonly struct OfficeType
    {
        public const string Baron = "Baron";
        public const string Chancellor = "Chancellor";
        public const string GuardCaptain = "Guard Captain";
        public const string Steward = "Steward";
        public const string Custom = "Custom";

        public static readonly string[] Core = { Chancellor, GuardCaptain, Steward };
        public static readonly string[] All = { Baron, Chancellor, GuardCaptain, Steward, Custom };
    }

    /// <summary>Social groups whose relations affect PPB.</summary>
    public readonly struct SocialGroup
    {
        public const string Nobility = "Nobility";
        public const string Burghers = "Burghers";
        public const string Peasants = "Peasants";
        public const string Clergy = "Clergy";
        public const string Magnates = "Magnates";

        public static readonly string[] Active = { Nobility, Burghers, Peasants };
        public static readonly string[] All = { Nobility, Burghers, Peasants, Clergy, Magnates };

        public static int DefaultInfluence(string group) => group switch
        {
            Nobility => 60,
            Burghers => 30,
            Peasants => 10,
            _ => 0,
        };

        public static bool DefaultIsActive(string group) => group switch
        {
            Nobility or Burghers or Peasants => true,
            _ => false,
        };

        public static decimal DefaultTax(string group) => NormalizeKey(group) switch
        {
            Nobility => 5m,
            Burghers => 15m,
            Peasants => 30m,
            _ => 0m,
        };

        /// <summary>Maps legacy Polish group names stored in older rows.</summary>
        public static string NormalizeKey(string group) => group switch
        {
            "Szlachta" => Nobility,
            "Mieszczaństwo" => Burghers,
            "Chłopi" => Peasants,
            "Duchowieństwo" => Clergy,
            "Magnaci" => Magnates,
            _ => group,
        };
    }

    /// <summary>Social relation score label (Excel scale).</summary>
    public readonly struct SocialRelation
    {
        public static string Label(int score) => score switch
        {
            <= -60 => DA_Common.Localization.Loc.T("Rebellion"),
            <= -30 => DA_Common.Localization.Loc.T("Discontent"),
            <= -10 => DA_Common.Localization.Loc.T("Hostile"),
            <= 20 => DA_Common.Localization.Loc.T("Indifferent"),
            <= 40 => DA_Common.Localization.Loc.T("Satisfied"),
            <= 70 => DA_Common.Localization.Loc.T("Friendly"),
            < 100 => DA_Common.Localization.Loc.T("Adored"),
            _ => DA_Common.Localization.Loc.T("Error"),
        };
    }

    /// <summary>Legacy discrete relation steps (-3..+3). Prefer <see cref="SocialRelation"/> for display.</summary>
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
        public const string Water = "Water";
        public const string Plains = "Plains";
        public const string Hills = "Hills";
        public const string Mountains = "Mountains";

        public static readonly string[] All = { Water, Plains, Hills, Mountains };

        public static bool SupportsFertility(string? baseType)
        {
            var canonical = Canonical(baseType);
            return canonical == Plains || canonical == Hills;
        }

        public static bool IsKnown(string? baseType) =>
            All.Contains(Canonical(baseType));

        public static bool IsWater(string? baseType) =>
            string.Equals(Canonical(baseType), Water, StringComparison.Ordinal);

        public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

        public static string CanonicalNameOrRaw(string? name)
        {
            var canonical = Canonical(name);
            return All.Contains(canonical) ? canonical : (name?.Trim() ?? string.Empty);
        }

        public static string DisplayName(string? baseType) => DisplayName(baseType, localizer: null);

        public static string DisplayName(string? baseType, IStringLocalizer? localizer)
        {
            if (string.IsNullOrWhiteSpace(baseType))
                return localizer is null ? Loc.T(Plains) : localizer[Plains].Value;

            var canonical = Canonical(baseType);
            if (All.Contains(canonical))
                return localizer is null ? Loc.T(canonical) : localizer[canonical].Value;

            return baseType.Trim();
        }

        private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
        {
            ["woda"] = Water,
            ["równiny"] = Plains,
            ["wzgórza"] = Hills,
            ["góry"] = Mountains,
        });
    }

    /// <summary>Soil fertility on a terrain tile (0–5, or unknown).</summary>
    public readonly struct TerrainFertility
    {
        public const int Unknown = -1;
        public const int Min = 0;
        public const int Max = 5;

        public static bool IsKnown(int fertility) => fertility >= Min && fertility <= Max;

        /// <summary>Short label (0 wasteland … 5 exceptionally fertile).</summary>
        public static string DisplayName(int fertility) => DisplayName(fertility, localizer: null);

        public static string DisplayName(int fertility, IStringLocalizer? localizer)
        {
            var key = DisplayNameKey(fertility);
            return localizer is null ? Loc.T(key) : localizer[key].Value;
        }

        /// <summary>Phrase used in tile descriptions, e.g. "very good fertility".</summary>
        public static string Phrase(int fertility) => Phrase(fertility, localizer: null);

        public static string Phrase(int fertility, IStringLocalizer? localizer)
        {
            var key = PhraseKey(fertility);
            return localizer is null ? Loc.T(key) : localizer[key].Value;
        }

        public static IEnumerable<string> LocalizationKeys()
        {
            for (var value = Min; value <= Max; value++)
            {
                yield return DisplayNameKey(value);
                yield return PhraseKey(value);
            }

            yield return DisplayNameKey(Unknown);
            yield return PhraseKey(Unknown);
        }

        private static string DisplayNameKey(int fertility) => fertility switch
        {
            0 => "Wasteland",
            1 => "Very poorly fertile",
            2 => "Poorly fertile",
            3 => "Fertile",
            4 => "Very fertile",
            5 => "Exceptionally fertile",
            _ => "Unknown fertility",
        };

        private static string PhraseKey(int fertility) => fertility switch
        {
            0 => "Fertility (wasteland)",
            1 => "Fertility (very poor)",
            2 => "Fertility (poor)",
            3 => "Fertility (good)",
            4 => "Fertility (very good)",
            5 => "Fertility (exceptional)",
            _ => "Fertility (unknown)",
        };

        /// <summary>Farms require fertility 2–5; unknown / wasteland / very poor soil cannot host a farm.</summary>
        public static bool SupportsFarm(int fertility) => fertility is >= 2 and <= Max;

        /// <summary>Building-catalog farm template for a fertility tier, or null if farms are not allowed.</summary>
        public static string? FarmTemplateName(int fertility) => fertility switch
        {
            2 => "Farm - poor fertility",
            3 => "Farm",
            4 => "Farm - fertile",
            5 => "Farm - bountiful",
            _ => null,
        };
    }

    /// <summary>Terrain feature add-ons (combinable bit flags).</summary>
    public readonly struct TerrainFeature
    {
        public const int None = 0;
        public const int Forest = 1 << 0;  // 1
        public const int Coast = 1 << 1;   // 2
        public const int River = 1 << 2;   // 4
        public const int Swamp = 1 << 3;   // 8
        public const int Wasteland = 1 << 4; // 16 (legacy / future)
        public const int DenseForest = 1 << 5; // 32

        public static readonly int[] Paintable = { Forest, DenseForest, Coast, River, Swamp };

        public static bool Has(int mask, int flag) => (mask & flag) != 0;

        public static int Toggle(int mask, int flag) => mask ^ flag;

        public static int Set(int mask, int flag, bool enabled) =>
            enabled ? mask | flag : mask & ~flag;

        /// <summary>Applies feature paint; Forest and DenseForest are mutually exclusive.</summary>
        public static int ApplyPaintToggle(int mask, int flag)
        {
            var next = Toggle(mask, flag);
            if (!Has(next, flag))
                return next;

            if (flag == Forest)
                next = Set(next, DenseForest, false);
            else if (flag == DenseForest)
                next = Set(next, Forest, false);

            return next;
        }

        public const string ForestName = "Forest";
        public const string DenseForestName = "Dense forest";
        public const string CoastName = "Coast";
        public const string RiverName = "River";
        public const string SwampName = "Swamp";
        public const string WastelandName = "Wasteland";

        public static readonly string[] AllNames =
        {
            ForestName, DenseForestName, CoastName, RiverName, SwampName, WastelandName,
        };

        public static string NameKey(int flag) => flag switch
        {
            Forest => ForestName,
            DenseForest => DenseForestName,
            Coast => CoastName,
            River => RiverName,
            Swamp => SwampName,
            Wasteland => WastelandName,
            _ => flag.ToString(),
        };

        public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

        public static string DisplayName(int flag) => DisplayName(flag, localizer: null);

        public static string DisplayName(int flag, IStringLocalizer? localizer)
        {
            var key = NameKey(flag);
            if (AllNames.Contains(key))
                return localizer is null ? Loc.T(key) : localizer[key].Value;
            return key;
        }

        private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(AllNames, new Dictionary<string, string>
        {
            ["las"] = ForestName,
            ["gęsty las"] = DenseForestName,
            ["gesty las"] = DenseForestName,
            ["wybrzeże"] = CoastName,
            ["rzeka"] = RiverName,
            ["bagna"] = SwampName,
            ["bagno"] = SwampName,
            ["pustkowie"] = WastelandName,
            ["pustynia"] = WastelandName,
        });

        public static string? LegacyName(int flag) => flag switch
        {
            Forest => "Las",
            DenseForest => "Gęsty las",
            Coast => "Wybrzeże",
            River => "Rzeka",
            Swamp => "Bagna",
            Wasteland => "Pustkowie",
            _ => null,
        };

        public static int FromLegacyCsv(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
                return None;

            var mask = None;
            foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                mask |= part switch
                {
                    "Las" or "Forest" => Forest,
                    "Gęsty las" or "Dense forest" or "DenseForest" => DenseForest,
                    "Wybrzeże" or "Coast" => Coast,
                    "Rzeka" or "River" => River,
                    "Bagna" or "Swamp" => Swamp,
                    "Pustkowie" or "Wasteland" => Wasteland,
                    _ => None,
                };
            }
            return mask;
        }
    }

    /// <summary>Natural deposit on a terrain tile (stored as string key on TerrainTile.Resource).</summary>
    public readonly struct TerrainResource
    {
        public const string SoftMetals = "Soft metals";
        public const string Iron = "Iron";
        public const string Silver = "Silver";
        public const string Gold = "Gold";
        public const string Dagoferryt = "Dagoferryt";
        public const string Fishery = "Fishery";
        public const string Stone = "Stone";
        public const string Granite = "Granite";
        public const string Tarnit = "Tarnit";
        public const string Obsidian = "Obsidian";
        public const string Clay = "Clay";
        public const string Ironwood = "Ironwood";
        public const string ElvenAlder = "Elven alder";
        public const string ShipbuildingWood = "Shipbuilding wood";
        public const string Salt = "Salt";
        public const string Sulfur = "Sulfur";
        public const string Gemstones = "Gemstones";
        public const string Furs = "Furs";
        public const string Woad = "Woad";
        public const string Madder = "Madder";
        public const string Weld = "Weld";

        public static readonly string[] All =
        {
            SoftMetals, Iron, Silver, Gold, Dagoferryt,
            Fishery,
            Stone, Granite, Tarnit, Obsidian,
            Clay, Ironwood, ElvenAlder, ShipbuildingWood, Salt, Sulfur, Gemstones,
            Woad, Madder, Weld,
            Furs,
        };

        public static bool IsKnown(string? key) => All.Contains(Canonical(key));

        public static bool IsDyePlant(string? key)
        {
            var canonical = Canonical(key);
            return string.Equals(canonical, Woad, StringComparison.Ordinal)
                || string.Equals(canonical, Madder, StringComparison.Ordinal)
                || string.Equals(canonical, Weld, StringComparison.Ordinal);
        }

        public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

        public static string CanonicalNameOrRaw(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;
            var canonical = Canonical(name);
            return All.Contains(canonical) ? canonical : name.Trim();
        }

        public static string DisplayName(string? key) => DisplayName(key, localizer: null);

        public static string DisplayName(string? key, IStringLocalizer? localizer)
        {
            if (string.IsNullOrWhiteSpace(key))
                return localizer is null ? Loc.T("None") : localizer["None"].Value;

            var canonical = Canonical(key);
            if (All.Contains(canonical))
                return localizer is null ? Loc.T(canonical) : localizer[canonical].Value;

            return key.Trim();
        }

        private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
        {
            ["metale miękkie"] = SoftMetals,
            ["metale miekkie"] = SoftMetals,
            ["żelazo"] = Iron,
            ["zelazo"] = Iron,
            ["srebro"] = Silver,
            ["złoto"] = Gold,
            ["zloto"] = Gold,
            ["dagoferryt"] = Dagoferryt,
            ["łowisko"] = Fishery,
            ["lowisko"] = Fishery,
            ["rybołówstwo"] = Fishery,
            ["kamień"] = Stone,
            ["kamien"] = Stone,
            ["granit"] = Granite,
            ["tarnit"] = Tarnit,
            ["obsydian"] = Obsidian,
            ["glina"] = Clay,
            ["żelazne drewno"] = Ironwood,
            ["zelazne drewno"] = Ironwood,
            ["żelazodrzew"] = Ironwood,
            ["elfia olcha"] = ElvenAlder,
            ["drewno okrętowe"] = ShipbuildingWood,
            ["drewno okretowe"] = ShipbuildingWood,
            ["sól"] = Salt,
            ["sol"] = Salt,
            ["siarka"] = Sulfur,
            ["kamienie szlachetne"] = Gemstones,
            ["klejnoty"] = Gemstones,
            ["futra"] = Furs,
            ["urzet"] = Woad,
            ["marzana"] = Madder,
            ["rezeda"] = Weld,
        });

        public static string IconUrl(string? key) => key switch
        {
            SoftMetals or Iron or Silver or Gold or Dagoferryt => "/icons/metal-bar.svg",
            Fishery => "/icons/fishing.svg",
            Stone => "/icons/stone-block-stroke.svg",
            Granite or Tarnit => "/icons/stone-block.svg",
            Obsidian or Sulfur => "/icons/silex.svg",
            Clay => "/icons/coal-pile.svg",
            Salt => "/icons/coal-pile-stroke.svg",
            Ironwood or ElvenAlder or ShipbuildingWood => "/icons/wood-pile.svg",
            Gemstones => "/icons/crystal-growth.svg",
            Woad => "/icons/three-leaves.svg",
            Madder => "/icons/root-tip.svg",
            Weld => "/icons/vine-flower.svg",
            Furs => "/icons/animal-hide.svg",
            _ => "/icons/metal-bar.svg",
        };

        public static string ColorHex(string? key) => key switch
        {
            SoftMetals => "#e67e22",
            Iron => "#2e86c1",
            Silver => "#c0c7ce",
            Gold => "#d4af37",
            Dagoferryt => "#8e44ad",
            Fishery => "#1a9bb5",
            Stone => "#f4f1ea",
            Granite => "#7f8c8d",
            Tarnit => "#9b59b6",
            Obsidian => "#1c1c1c",
            Clay => "#c4783a",
            Ironwood => "#2471a3",
            ElvenAlder => "#d4af37",
            // Dark hull timber — distinct from Clay orange and Elven alder gold.
            ShipbuildingWood => "#5c3317",
            Salt => "#f8f8ff",
            // Yellow obsidian-shaped deposit.
            Sulfur => "#e6c200",
            Gemstones => "#9b59b6",
            Woad => "#2e6bb0",
            Madder => "#a83c3c",
            Weld => "#d4a017",
            Furs => "#8b5e3c",
            _ => "#888888",
        };

        /// <summary>
        /// Near-white fills that still use CSS masks — soft drop-shadow outline.
        /// Stone / Salt use <see cref="UsesBakedStrokeIcon"/> instead (real black SVG stroke).
        /// </summary>
        public static bool NeedsDarkOutline(string? key) =>
            string.Equals(key, Silver, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Icon is a ready-made SVG (white fill + black stroke). Render as &lt;img&gt;,
        /// not as a CSS mask — otherwise the stroke would inherit the resource fill color.
        /// </summary>
        public static bool UsesBakedStrokeIcon(string? key) =>
            string.Equals(key, Stone, StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, Salt, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Map-placed terrain improvements (stored as TerrainImprovement.Name).</summary>
    public readonly struct MapImprovement
    {
        public const string Town = "Town";
        public const string Village = "Village";
        public const string HuntersLodge = "Hunter's lodge";
        public const string FishingHarbor = "Fishing harbor";
        public const string Mine = "Mine";
        public const string Sawmill = "Sawmill";
        public const string Farm = "Farm";
        public const string Custom = "Custom";

        public const string StoneTowerIconUrl = "/icons/stone-tower.svg";

        public static readonly string[] All =
        {
            Town, Village, HuntersLodge, FishingHarbor, Mine, Sawmill, Farm, Custom,
        };

        /// <summary>Icons selectable for custom map improvements.</summary>
        public static readonly string[] IconChoices =
        {
            "/icons/medieval-village-01.svg",
            "/icons/wood-cabin.svg",
            "/icons/hunting-horn.svg",
            "/icons/fishing-net.svg",
            "/icons/mine-wagon.svg",
            "/icons/axe-in-stump.svg",
            "/icons/windmill.svg",
            StoneTowerIconUrl,
        };

        public static bool IsKnown(string? key) =>
            !string.IsNullOrWhiteSpace(key) && All.Contains(key);

        public static bool IsCustom(string? key) =>
            string.Equals(key, Custom, StringComparison.OrdinalIgnoreCase);

        public static string DisplayName(string? key) => key switch
        {
            Town => DA_Common.Localization.Loc.T(Town),
            Village => DA_Common.Localization.Loc.T(Village),
            HuntersLodge => DA_Common.Localization.Loc.T(HuntersLodge),
            FishingHarbor => DA_Common.Localization.Loc.T(FishingHarbor),
            Mine => DA_Common.Localization.Loc.T(Mine),
            Sawmill => DA_Common.Localization.Loc.T(Sawmill),
            Farm => DA_Common.Localization.Loc.T(Farm),
            Custom => DA_Common.Localization.Loc.T(Custom),
            _ => key ?? "None",
        };

        public static string IconUrl(string? key) => key switch
        {
            Town => "/icons/medieval-village-01.svg",
            Village => "/icons/wood-cabin.svg",
            HuntersLodge => "/icons/hunting-horn.svg",
            FishingHarbor => "/icons/fishing-net.svg",
            Mine => "/icons/mine-wagon.svg",
            Sawmill => "/icons/axe-in-stump.svg",
            Farm => "/icons/windmill.svg",
            Custom => StoneTowerIconUrl,
            _ => "/icons/wood-cabin.svg",
        };

        public const string ConstructionIconUrl = "/icons/crane.svg";

        /// <summary>Uses a stored custom icon when set; otherwise the map-kind default.</summary>
        public static string ResolveIconUrl(string? mapKind, string? iconUrl) =>
            !string.IsNullOrWhiteSpace(iconUrl) ? iconUrl.Trim() : IconUrl(mapKind);

        public static bool RequiresPlaceName(string? key) =>
            key is Town or Village;
    }

    /// <summary>Catalog entry type (BuildingKind).</summary>
    public readonly struct BuildingKind
    {
        public const string Building = "Building";
        public const string Improvement = "Improvement";

        public static readonly string[] All = { Building, Improvement };

        public static string DisplayName(string? kind) => DisplayName(kind, localizer: null);

        public static string DisplayName(string? kind, IStringLocalizer? localizer)
        {
            if (string.IsNullOrWhiteSpace(kind))
                return string.Empty;
            if (All.Contains(kind))
                return localizer is null ? Loc.T(kind) : localizer[kind].Value;
            return kind;
        }
    }

    /// <summary>Stable keys for fixed starter city buildings (seeded from Buildings catalog).</summary>
    public readonly struct CoreCityBuildingKey
    {
        public const string StewardsBuilding = "stewards-building";
        public const string Tavern = "tavern";
        public const string MarketSquare = "market-square";
        public const string TownGarrison = "town-garrison";

        public static readonly string[] All = { StewardsBuilding, Tavern, MarketSquare, TownGarrison };

        /// <summary>Catalog template name for each starter building.</summary>
        public static string CatalogName(string coreKey) => coreKey switch
        {
            StewardsBuilding => "Steward's Building",
            Tavern => "Tavern",
            MarketSquare => "Market Square",
            TownGarrison => "Town Garrison",
            _ => coreKey,
        };
    }

    /// <summary>Status projektu.</summary>
    public readonly struct ProjectStatus
    {
        /// <summary>Player proposal / unfinished setup — not accepted by MG yet.</summary>
        public const string Draft = "Draft";
        /// <summary>Accepted by MG (or auto-created); waiting for resource allocation. Turns do not tick.</summary>
        public const string ResourceAllocation = "Resource allocation";
        /// <summary>Fully funded (or past allocation); turns tick on Resolve when still funded.</summary>
        public const string InProgress = "In progress";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All =
        {
            Draft, ResourceAllocation, InProgress, Completed, Cancelled,
        };

        public static bool IsTerminal(string? status) =>
            IsCompleted(status) || IsCancelled(status);

        public static bool IsCompleted(string? status) =>
            string.Equals(status, Completed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase);

        public static bool IsCancelled(string? status) =>
            string.Equals(status, Cancelled, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);

        public static bool IsResourceAllocation(string? status) =>
            string.Equals(status, ResourceAllocation, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Resource allocation", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>How a project cost is paid.</summary>
    public readonly struct ProjectCostMode
    {
        public const string GoldProduction = "Gold & Production";
        public const string Materials = "Materials";
        /// <summary>Both tracks required together (unit training exception).</summary>
        public const string Combined = "Combined";

        public static readonly string[] All = { GoldProduction, Materials, Combined };
    }

    /// <summary>Which cost payment tracks the GM allows for a project.</summary>
    public readonly struct ProjectAllowedCostModes
    {
        public const string GoldProductionOnly = "Gold & Production only";
        public const string MaterialsOnly = "Materials only";
        public const string PlayerChoice = "Player choice";
        /// <summary>
        /// Exception: Gold+Production and Materials (e.g. Defense) are all required —
        /// not an either/or choice. Used by Unit Training.
        /// </summary>
        public const string Combined = "Combined";

        public static readonly string[] All =
        {
            GoldProductionOnly, MaterialsOnly, PlayerChoice, Combined,
        };
    }

    /// <summary>What a completed project becomes.</summary>
    public readonly struct ProjectOutputKind
    {
        public const string DecreeOrTechnology = "Decree / Technology";
        public const string Event = "Event";
        public const string OneTimeResources = "One-time resources";
        public const string Building = "Building";
        public const string Improvement = "Improvement";
        public const string UnitTraining = "Unit Training";
        public const string UnitReinforce = "Unit Reinforce";
        public const string UnitChangeEquipment = "Unit Change Equipment";

        public static readonly string[] All =
        {
            DecreeOrTechnology, Event, OneTimeResources, Building, Improvement,
            UnitTraining, UnitReinforce, UnitChangeEquipment,
        };
    }

    /// <summary>Sections on the Relations tab.</summary>
    public readonly struct RelationCategory
    {
        public const string SeniorHouses = "Senior Houses";
        public const string Vassals = "Vassals";
        public const string Neighbors = "Neighbors";
        public const string Organizations = "Organizations";
        public const string Acquaintances = "Friends, acquaintances and enemies";

        public static readonly string[] All =
        {
            SeniorHouses, Vassals, Neighbors, Organizations, Acquaintances,
        };

        public static string AttitudeLabel(int attitude)
        {
            var v = Math.Clamp(attitude, -200, 200);
            return v switch
            {
                <= -150 => DA_Common.Localization.Loc.T("Mortal enemy"),
                <= -80 => DA_Common.Localization.Loc.T("Hostile"),
                <= -30 => DA_Common.Localization.Loc.T("Cold"),
                < 30 => DA_Common.Localization.Loc.T("Neutral"),
                < 80 => DA_Common.Localization.Loc.T("Friendly"),
                < 150 => DA_Common.Localization.Loc.T("Ally"),
                _ => DA_Common.Localization.Loc.T("Best friend"),
            };
        }
    }

    /// <summary>Defaults for vassals auto-synced from terrain fiefs.</summary>
    public static class RelationVassalDefaults
    {
        public const string BaronetTitle = "Baronet";
        public const string DirectVassalModifier = "direct vassal";
        public const int DirectVassalAttitude = 30;
    }

    /// <summary>Defaults for Senior Houses relations.</summary>
    public static class RelationSeniorDefaults
    {
        public const string AllyEmpireVassalModifier = "ally, empire vassal";
        public const int AllyEmpireVassalAttitude = 10;
    }

    /// <summary>Budget page source labels (treasury gold column).</summary>
    public readonly struct BudgetSource
    {
        public const string Advisors = "Advisors";
        public const string Fief = "Fief";
        public const string Buildings = "City and Buildings";
        public const string SocialGroups = "Social Groups";
        public const string Improvements = "Terrain Improvements";
        public const string Decrees = "Decrees and Technologies";
        public const string Events = "Events";
        public const string Army = "Army";
        public const string Community = "Community";
        public const string PercentModifiers = "% modifiers";
        public const string Other = "Other";

        public static readonly string[] Income =
        {
            Advisors, Fief, Buildings, SocialGroups, Improvements, Decrees, Events, Army, Community, PercentModifiers, Other,
        };

        public static readonly string[] Expense =
        {
            Advisors, Fief, Buildings, SocialGroups, Improvements, Decrees, Events, Army, Community, PercentModifiers, Other,
        };
    }

    /// <summary>Kategorie kar i bonusów społeczności.</summary>
    public readonly struct CommunitySource
    {
        public const string Society = "Society";
        public const string Hunger = "Hunger";
        public const string Crime = "Crime";
        public const string Corruption = "Corruption";
        public const string Unrest = "Unrest";
        public const string Economy = "Economy";

        public static readonly string[] All = { Society, Hunger, Crime, Corruption, Unrest, Economy };

        public static string NormalizeKey(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            var s = raw.Trim().ToLowerInvariant();
            return s switch
            {
                "głód" or "glod" or "hunger" => Hunger,
                "przestępczość" or "przestepczosc" or "crime" => Crime,
                "korupcja" or "corruption" => Corruption,
                "niepokój" or "niepokoj" or "unrest" => Unrest,
                "ekonomia" or "economy" or "conjuncture" or "koniunktura" => Economy,
                _ => raw.Trim(),
            };
        }
    }

    /// <summary>Terrain map grid dimensions (per barony; defaults match legacy 15×15).</summary>
    public readonly struct TerrainMapGrid
    {
        public const int DefaultSize = 15;
        public const int MinSize = 5;
        public const int MaxSize = 40;

        /// <summary>Legacy square default (seed / migration).</summary>
        public const int Size = DefaultSize;
        public const int CellCount = Size * Size;

        public static int ClampDimension(int value) =>
            Math.Clamp(value, MinSize, MaxSize);

        public static bool IsValidDimension(int value) =>
            value >= MinSize && value <= MaxSize;
    }

    /// <summary>Lord's Seat room lifecycle.</summary>
    public readonly struct SeatRoomStatus
    {
        public const string Active = "Active";
        public const string Ruin = "Ruin";

        public static readonly string[] All = { Active, Ruin };
    }

    /// <summary>Room construction material.</summary>
    public readonly struct SeatRoomMaterial
    {
        public const string WeakWood = "Weak wood";
        public const string HardWood = "Hard wood";
        public const string Bricks = "Bricks";
        public const string Stone = "Stone";
        public const string Granite = "Granite";
        public const string Tarnit = "Tarnit";

        public static readonly string[] All =
        {
            WeakWood, HardWood, Bricks, Stone, Granite, Tarnit,
        };

        /// <summary>Stored key <see cref="WeakWood"/> is shown as “Wood”.</summary>
        public static string DisplayName(string material) => DisplayName(material, localizer: null);

        public static string DisplayName(string material, IStringLocalizer? localizer)
        {
            var key = string.Equals(material, WeakWood, StringComparison.Ordinal) ? "Wood" : material;
            return localizer is null ? Loc.T(key) : localizer[key].Value;
        }

        public static decimal PrestigeBonus(string? material) => material switch
        {
            WeakWood => 1.0m,
            HardWood => 1.1m,
            Bricks => 1.3m,
            Stone => 1.5m,
            Granite => 1.8m,
            Tarnit => 2.5m,
            _ => 1.0m,
        };

        public static string OptionLabel(string material) => OptionLabel(material, localizer: null);

        public static string OptionLabel(string material, IStringLocalizer? localizer)
        {
            var name = DisplayName(material, localizer);
            var bonus = PrestigeBonus(material).ToString("0.#");
            return localizer is null
                ? Loc.T("{0} ({1} prestige multiplier)", name, bonus)
                : localizer["{0} ({1} prestige multiplier)", name, bonus].Value;
        }
    }

    /// <summary>Room size tier derived from tile count (thresholds may change).</summary>
    public readonly struct SeatRoomSizeCategory
    {
        public const string Small = "Small";
        public const string Medium = "Medium";
        public const string Large = "Large";
        public const string Huge = "Huge";

        public static readonly string[] All = { Small, Medium, Large, Huge };

        /// <summary>Tile-count thresholds — tune later without schema changes.</summary>
        public static string FromTileCount(int tiles) => tiles switch
        {
            <= 0 => Small,
            <= 4 => Small,
            <= 9 => Medium,
            <= 16 => Large,
            _ => Huge,
        };

        public static int Rank(string category) => category switch
        {
            Small => 0,
            Medium => 1,
            Large => 2,
            Huge => 3,
            _ => 0,
        };

        public static decimal PrestigeBonus(string? category) => category switch
        {
            Small => 0.5m,
            Medium => 1.0m,
            Large => 1.5m,
            Huge => 2.0m,
            _ => 0.5m,
        };

        public static decimal PrestigeBonusFromTiles(int tiles) =>
            PrestigeBonus(FromTileCount(tiles));

        public static string DisplayName(string? category) => DisplayName(category, localizer: null);

        public static string DisplayName(string? category, IStringLocalizer? localizer)
        {
            if (string.IsNullOrWhiteSpace(category))
                return string.Empty;
            return localizer is null ? Loc.T(category) : localizer[category].Value;
        }

        public static string OptionLabel(string category) => OptionLabel(category, localizer: null);

        public static string OptionLabel(string category, IStringLocalizer? localizer)
        {
            var name = DisplayName(category, localizer);
            var bonus = PrestigeBonus(category).ToString("0.#");
            return localizer is null
                ? Loc.T("{0} ({1} prestige multiplier)", name, bonus)
                : localizer["{0} ({1} prestige multiplier)", name, bonus].Value;
        }

        /// <summary>Compact table label: S / M / L / H.</summary>
        public static string Letter(string? category) => category switch
        {
            Small => "S",
            Medium => "M",
            Large => "L",
            Huge => "H",
            _ => "?",
        };

        public static bool MeetsMinimum(int tileCount, string minCategory) =>
            Rank(FromTileCount(tileCount)) >= Rank(minCategory);
    }

    /// <summary>Suggested chamber prestige multiplier = material bonus + size bonus.</summary>
    public static class SeatRoomPrestige
    {
        public static decimal Suggested(string? material, string? sizeCategory) =>
            SeatRoomMaterial.PrestigeBonus(material) + SeatRoomSizeCategory.PrestigeBonus(sizeCategory);

        public static decimal Suggested(string? material, int tileCount) =>
            Suggested(material, SeatRoomSizeCategory.FromTileCount(tileCount));

        public static string FormulaHint(string? material, int tileCount) =>
            FormulaHint(material, tileCount, localizer: null);

        public static string FormulaHint(string? material, int tileCount, IStringLocalizer? localizer)
        {
            var size = SeatRoomSizeCategory.FromTileCount(tileCount);
            var matBonus = SeatRoomMaterial.PrestigeBonus(material);
            var sizeBonus = SeatRoomSizeCategory.PrestigeBonus(size);
            var total = matBonus + sizeBonus;
            var matName = SeatRoomMaterial.DisplayName(material ?? SeatRoomMaterial.WeakWood, localizer);
            var sizeName = SeatRoomSizeCategory.DisplayName(size, localizer);
            return $"{matName} {matBonus:0.#} + {sizeName} {sizeBonus:0.#} = {total:0.#}";
        }
    }

    /// <summary>Advantage / disadvantage text trait on a room.</summary>
    public readonly struct SeatRoomTraitKind
    {
        public const string Advantage = "Advantage";
        public const string Disadvantage = "Disadvantage";

        public static readonly string[] All = { Advantage, Disadvantage };
    }

    /// <summary>Vertical floor index for the lord's seat plan (-3 underground … 5 above ground).</summary>
    public static class SeatFloorLevel
    {
        public const int Min = -3;
        public const int Max = 5;
        public const int Ground = 0;

        public static bool IsValid(int level) => level >= Min && level <= Max;

        public static int Clamp(int level) => Math.Clamp(level, Min, Max);

        public static string Label(int level) => Label(level, localizer: null);

        public static string Label(int level, IStringLocalizer? localizer) => level switch
        {
            < Ground => localizer is null
                ? Loc.T("Level {0} (below)", level)
                : localizer["Level {0} (below)", level].Value,
            Ground => localizer is null
                ? Loc.T("Level 0 (ground)")
                : localizer["Level 0 (ground)"].Value,
            _ => localizer is null
                ? Loc.T("Level {0} (above)", level)
                : localizer["Level {0} (above)", level].Value,
        };
    }

    /// <summary>Painted map cell that is not a chamber.</summary>
    public readonly struct SeatTileKind
    {
        public const string Wall = "Wall";
        public const string Ground = "Ground";
        public const string Water = "Water";

        /// <summary>Legacy alias kept for older painted tiles.</summary>
        public const string Space = "Space";

        public static readonly string[] All = { Wall, Ground, Water };

        public static bool IsKnown(string? kind)
        {
            if (string.IsNullOrWhiteSpace(kind))
                return false;

            var trimmed = kind.Trim();
            return string.Equals(trimmed, Wall, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trimmed, Ground, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trimmed, Water, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trimmed, Space, StringComparison.OrdinalIgnoreCase);
        }

        public static string Normalize(string? kind)
        {
            if (string.IsNullOrWhiteSpace(kind))
                return Ground;

            var trimmed = kind.Trim();
            if (string.Equals(trimmed, Space, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trimmed, Water, StringComparison.OrdinalIgnoreCase))
                return Water;
            if (string.Equals(trimmed, Wall, StringComparison.OrdinalIgnoreCase))
                return Wall;
            if (string.Equals(trimmed, Ground, StringComparison.OrdinalIgnoreCase))
                return Ground;

            return Ground;
        }

        public static string Label(string kind) => Normalize(kind) switch
        {
            Wall => "Wall / fortification",
            Ground => "Earth / ground",
            Water => "Water",
            _ => kind,
        };
    }
}
