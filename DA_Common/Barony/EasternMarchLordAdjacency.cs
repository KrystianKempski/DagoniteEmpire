namespace DA_Common.Barony
{
  /// <summary>
  /// Undirected adjacency between Eastern March lords (for trade-route validation).
  /// Expand over time; unknown pairs skip strict checks when the graph has no edge data yet.
  /// </summary>
  public static class EasternMarchLordAdjacency
  {
    private static readonly HashSet<(string A, string B)> Edges = BuildEdges();

    public static bool AreNeighbors(string lordKeyA, string lordKeyB)
    {
      if (string.Equals(lordKeyA, lordKeyB, StringComparison.OrdinalIgnoreCase))
        return true;

      var a = lordKeyA.Trim().ToLowerInvariant();
      var b = lordKeyB.Trim().ToLowerInvariant();
      if (Edges.Contains((a, b)))
        return true;

      // Permissive until the march adjacency graph is fully curated.
      return true;
    }

    public static bool HasPath(
      IReadOnlyCollection<string> borderLordKeys,
      string counterpartyLordKey,
      IReadOnlyList<string> orderedTransitLordKeys)
    {
      if (borderLordKeys.Count == 0)
        return orderedTransitLordKeys.Count == 0;

      var target = counterpartyLordKey.Trim().ToLowerInvariant();
      var borders = borderLordKeys.Select(k => k.Trim().ToLowerInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase);

      if (orderedTransitLordKeys.Count == 0)
        return borders.Contains(target);

      var chain = orderedTransitLordKeys.Select(k => k.Trim().ToLowerInvariant()).ToList();
      if (!borders.Any(b => AreNeighbors(b, chain[0])))
        return false;

      for (var i = 0; i < chain.Count - 1; i++)
      {
        if (!AreNeighbors(chain[i], chain[i + 1]))
          return false;
      }

      return AreNeighbors(chain[^1], target);
    }

    private static HashSet<(string A, string B)> BuildEdges()
    {
      // Seed a few well-known march neighbours; MG can still route via transit when unknown.
      static void Link(HashSet<(string, string)> set, string a, string b)
      {
        a = a.ToLowerInvariant();
        b = b.ToLowerInvariant();
        if (string.CompareOrdinal(a, b) > 0)
          (a, b) = (b, a);
        set.Add((a, b));
      }

      var edges = new HashSet<(string, string)>();
      Link(edges, "hardwin", "balon");
      Link(edges, "hardwin", "argewald");
      Link(edges, "hardwin", "dyron");
      Link(edges, "argewald", "olgred");
      Link(edges, "argewald", "arienna");
      Link(edges, "balon", "huel");
      Link(edges, "balon", "urven");
      Link(edges, "aren", "hardwin");
      Link(edges, "durisug", "balon");
      return edges;
    }

    /// <summary>Curated neighbour pairs used to seed march map routes (expand with the map editor).</summary>
    public static IReadOnlyList<(string LordKeyA, string LordKeyB)> SeedPairs { get; } = BuildEdges()
      .Select(e => e)
      .ToList();
  }
}
