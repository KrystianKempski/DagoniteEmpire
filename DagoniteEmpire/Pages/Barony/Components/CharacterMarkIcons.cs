namespace DagoniteEmpire.Pages.Barony.Components;

using MudBlazor;

internal static class CharacterMarkIcons
{
    public static string MaterialIcon(string? iconKey) => (iconKey ?? "").Trim().ToLowerInvariant() switch
    {
        "vip" => Icons.Material.Filled.Star,
        "flag" => Icons.Material.Filled.Flag,
        "danger" => Icons.Material.Filled.Warning,
        "ally" => Icons.Material.Filled.Favorite,
        "faction" => Icons.Material.Filled.Groups,
        "deal" => Icons.Material.Filled.Handshake,
        _ => Icons.Material.Filled.Star,
    };
}
