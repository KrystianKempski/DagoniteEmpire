namespace DA_Common.Barony
{
  public static partial class KnownLordsCatalog
  {
    private static readonly Dictionary<string, KnownLordEntry> ByKey =
      EasternMarch.ToDictionary(LordKey, StringComparer.OrdinalIgnoreCase);

    public static string LordKey(KnownLordEntry lord) =>
      lord.Name.Trim().ToLowerInvariant()
        .Replace(' ', '-')
        .Replace("'", string.Empty);

    public static KnownLordEntry? FindByKey(string? key) =>
      string.IsNullOrWhiteSpace(key) ? null :
      ByKey.TryGetValue(key.Trim(), out var lord) ? lord : null;

    /// <summary>Resolve lord for a map place label (usually holdings name).</summary>
    public static KnownLordEntry? FindByPlaceLabel(string? placeLabel)
    {
      if (string.IsNullOrWhiteSpace(placeLabel))
        return null;

      var label = placeLabel.Trim();
      var exact = EasternMarch.FirstOrDefault(l =>
        string.Equals(l.Holdings, label, StringComparison.OrdinalIgnoreCase));
      if (exact is not null)
        return exact;

      var contains = EasternMarch
        .Where(l => label.Contains(l.Holdings, StringComparison.OrdinalIgnoreCase) ||
                    l.Holdings.Contains(label, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(l => l.Wealth)
        .FirstOrDefault();
      return contains;
    }

    public static string? LordKeyForPlaceLabel(string? placeLabel)
    {
      var lord = FindByPlaceLabel(placeLabel);
      return lord is null ? null : LordKey(lord);
    }

    public static KnownLordEntry? ResolveLordForMapNode(MarchMapNode node)
    {
      var byKey = FindByKey(node.LordKey);
      if (byKey is not null)
        return byKey;
      return FindByPlaceLabel(node.Label);
    }

    public static void ApplyKnownLordLinks(IEnumerable<MarchMapNode> nodes)
    {
      foreach (var node in nodes)
      {
        if (!string.IsNullOrWhiteSpace(node.LordKey) && FindByKey(node.LordKey) is not null)
          continue;

        var key = LordKeyForPlaceLabel(node.Label);
        if (key is not null)
          node.LordKey = key;
      }
    }

    public static string DisplayName(KnownLordEntry lord) =>
      lord.DisplayFullName();

    public static IReadOnlyList<string> ProducedGoodKeys(KnownLordEntry lord) =>
      TradeGoodLordNames.ParseLordGoodsList(lord.ProducedGoods);

    /// <summary>Match a Neighbors relation name (e.g. "Balon Greywarden") to a catalog lord key.</summary>
    public static string? MatchLordKeyFromRelationName(string? relationName)
    {
      if (string.IsNullOrWhiteSpace(relationName))
        return null;

      var name = relationName.Trim();
      foreach (var lord in EasternMarch)
      {
        if (name.StartsWith(lord.Name, StringComparison.OrdinalIgnoreCase))
          return LordKey(lord);
        if (name.Contains(lord.Name, StringComparison.OrdinalIgnoreCase))
          return LordKey(lord);
      }

      var first = name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
      if (first is null)
        return null;

      return EasternMarch.FirstOrDefault(l => string.Equals(l.Name, first, StringComparison.OrdinalIgnoreCase)) is { } hit
        ? LordKey(hit)
        : null;
    }
  }
}
