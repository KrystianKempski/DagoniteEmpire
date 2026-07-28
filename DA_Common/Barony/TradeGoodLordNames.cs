namespace DA_Common.Barony
{
  /// <summary>Maps lord roster display labels to <see cref="TradeGoodsCatalog"/> keys.</summary>
  public static class TradeGoodLordNames
  {
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
      ["Soft metals"] = "soft-metals",
      ["Military weapons"] = "access-arms-military",
      ["Light armor"] = "access-armor-light",
      ["Medium armor"] = "access-armor-medium",
      ["Heavy armor"] = "access-armor-heavy",
      ["Firearms"] = "access-arms-firearms",
      ["War horses"] = "war-horses",
      ["Noble horses"] = "noble-horses",
      ["Salted fish & meat"] = "fish-meat-salted",
      ["Honey & wax"] = "honey-wax",
      ["Herbs & roots"] = "herbs-roots",
      ["Flax & hemp"] = "flax-hemp",
      ["Elven forest crafts"] = "elf-forest-goods",
      ["Elven alder"] = "elven-alder",
      ["Shipbuilding timber"] = "shipbuilding-wood",
      ["Building stone"] = "building-stone",
      ["Ironwood"] = "ironwood",
    };

    public static string? ResolveGoodKey(string? displayLabel)
    {
      if (string.IsNullOrWhiteSpace(displayLabel))
        return null;

      var label = displayLabel.Trim();
      if (Aliases.TryGetValue(label, out var aliasKey))
        return aliasKey;

      foreach (var g in TradeGoodsCatalog.All)
      {
        if (string.Equals(g.Name, label, StringComparison.OrdinalIgnoreCase))
          return g.Key;
      }

      var slug = Slug(label);
      var bySlug = TradeGoodsCatalog.All.FirstOrDefault(g =>
        string.Equals(Slug(g.Name), slug, StringComparison.OrdinalIgnoreCase));
      return bySlug?.Key;
    }

    public static IReadOnlyList<string> ParseLordGoodsList(string? commaSeparated)
    {
      if (string.IsNullOrWhiteSpace(commaSeparated))
        return Array.Empty<string>();

      var keys = new List<string>();
      foreach (var part in commaSeparated.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
      {
        var key = ResolveGoodKey(part);
        if (key is not null && keys.All(k => !string.Equals(k, key, StringComparison.OrdinalIgnoreCase)))
          keys.Add(key);
      }

      return keys;
    }

    private static string Slug(string text) =>
      string.Concat(text.ToLowerInvariant()
        .Select(c => char.IsLetterOrDigit(c) ? c : '-'))
        .Trim('-');
  }
}
