using DA_Common.Barony;
using DA_Models.BaronyModels;
using MudBlazor;

namespace DagoniteEmpire.Pages.Barony;

/// <summary>Shared colours and labels for the terrain map grid.</summary>
public static class TerrainMapVisuals
{
    public const string UnknownFertilityColor = "rgba(184, 174, 152, 0.35)";
    public const string HiddenFertilityFill = "#ffffff";
    public const string WaterFill = "rgba(90, 168, 220, 0.42)";

    public static bool IsFertilityUnknown(TerrainTileDTO? tile) =>
        tile is null
        || !TerrainBaseType.SupportsFertility(tile.BaseType)
        || !TerrainFertility.IsKnown(tile.Fertility);

    public static string FertilityLabel(TerrainTileDTO? tile)
    {
        if (IsFertilityUnknown(tile))
            return string.Empty;
        return tile!.Fertility.ToString();
    }

    public static string CellBackgroundColor(TerrainTileDTO? tile, bool showFertility)
    {
        if (tile?.BaseType == TerrainBaseType.Water)
            return WaterFill;

        if (!showFertility)
            return HiddenFertilityFill;

        return FertilityColor(tile);
    }

    public static string FertilityColor(TerrainTileDTO? tile)
    {
        if (IsFertilityUnknown(tile))
            return UnknownFertilityColor;
        return FertilityColorForValue(tile!.Fertility);
    }

    public static string FertilityColorForValue(int fertility)
    {
        if (!TerrainFertility.IsKnown(fertility))
            return UnknownFertilityColor;

        var t = Math.Clamp(fertility, TerrainFertility.Min, TerrainFertility.Max) / (decimal)TerrainFertility.Max;
        var r = (int)(245 + (45 - 245) * t);
        var g = (int)(230 + (122 - 230) * t);
        var b = (int)(66 + (51 - 66) * t);
        return $"rgba({r}, {g}, {b}, 0.85)";
    }

    public static string DomainOverlayColor(TerrainMapDomainDTO domain) =>
        HexToRgba(domain.ColorHex, domain.IsPrimary ? 0.18m : 0.35m);

    public static string FiefOverlayColor(FiefDTO fief) =>
        HexToRgba(fief.ColorHex, 0.50m);

    public static string BaseTypeIcon(string? baseType) => (baseType ?? TerrainBaseType.Plains) switch
    {
        TerrainBaseType.Water => Icons.Material.Filled.Water,
        TerrainBaseType.Hills => Icons.Material.Rounded.FilterHdr, // fallback; prefer custom SVG
        TerrainBaseType.Mountains => Icons.Material.Filled.Landscape,
        _ => Icons.Material.Filled.Grass,
    };

    public const string HillsIconUrl = "/icons/hills.svg";
    public const string ForestIconUrl = "/icons/tree.svg";
    public const string BeechIconUrl = "/icons/beech.svg";
    public const string OakIconUrl = "/icons/oak.svg";
    public const string SwampIconUrl = "/icons/dead-wood.svg";

    public static string? CustomBaseTypeIconUrl(string? baseType) => (baseType ?? TerrainBaseType.Plains) switch
    {
        TerrainBaseType.Hills => HillsIconUrl,
        _ => null,
    };

    public static bool UsesCustomBaseTypeIcon(string? baseType) =>
        CustomBaseTypeIconUrl(baseType) is not null;

    public static string? FeatureIconUrl(int feature) => feature switch
    {
        TerrainFeature.Forest => ForestIconUrl,
        TerrainFeature.DenseForest => ForestIconUrl,
        TerrainFeature.Swamp => SwampIconUrl,
        _ => null,
    };

    /// <summary>Icon URLs for a feature (Dense forest: tree, beech, tree).</summary>
    public static IReadOnlyList<string> FeatureIconUrls(int feature) => feature switch
    {
        TerrainFeature.DenseForest => new[] { ForestIconUrl, BeechIconUrl, ForestIconUrl },
        _ => FeatureIconUrl(feature) is string url ? new[] { url } : Array.Empty<string>(),
    };

    public static string FeatureMudIcon(int feature) => feature switch
    {
        TerrainFeature.Coast => Icons.Material.Filled.Water,
        TerrainFeature.River => Icons.Material.Filled.WaterDrop,
        _ => Icons.Material.Filled.Lens,
    };

    public static string FeatureCss(int feature) => feature switch
    {
        TerrainFeature.Forest => "forest",
        TerrainFeature.DenseForest => "dense-forest",
        TerrainFeature.Coast => "coast",
        TerrainFeature.River => "river",
        TerrainFeature.Swamp => "swamp",
        _ => "unknown",
    };

    public static int FeatureIconCount(int feature) => FeatureIconUrls(feature).Count;

    public static bool UsesLordTitle(FiefDTO fief) =>
        fief.IsBaronDemesne || fief.IsDomainDefault;

    public static bool IsProtectedDefaultFief(FiefDTO fief) =>
        UsesLordTitle(fief);

    public static string DefaultLordFiefName(string lordName) => $"Lord {lordName}";

    public static string FiefChipTitle(FiefDTO fief) =>
        UsesLordTitle(fief) ? DefaultLordFiefName(fief.LiegeName) : fief.LiegeName;

    public static string FiefChipSubtitle(FiefDTO fief, TerrainMapDomainDTO? seniorDomain) =>
        UsesLordTitle(fief)
            ? seniorDomain?.Name ?? "unassigned"
            : seniorDomain is null
                ? "unassigned"
                : $"{seniorDomain.Name}, senior: {seniorDomain.LordName}";

    public static string HexToRgba(string hex, decimal alpha)
    {
        var a = alpha.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var normalized = (hex ?? "#888888").Trim();
        if (!normalized.StartsWith('#'))
            normalized = "#" + normalized;
        if (normalized.Length != 7)
            return $"rgba(136, 136, 136, {a})";

        var r = Convert.ToInt32(normalized[1..3], 16);
        var g = Convert.ToInt32(normalized[3..5], 16);
        var b = Convert.ToInt32(normalized[5..7], 16);
        return $"rgba({r}, {g}, {b}, {a})";
    }
}
