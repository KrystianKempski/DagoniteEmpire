namespace DA_Common.Barony
{
  public static class TradeTreatyCalculator
  {
    public const int MaxGranteesPerProducedGood = 1;

    public static IReadOnlyList<BaronyTradeTreaty> TreatiesUsingLord(
      IEnumerable<BaronyTradeTreaty> treaties,
      string lordKey)
    {
      if (string.IsNullOrWhiteSpace(lordKey))
        return Array.Empty<BaronyTradeTreaty>();

      return treaties
        .Where(t =>
          string.Equals(t.CounterpartyLordKey, lordKey, StringComparison.OrdinalIgnoreCase) ||
          t.Paragraphs.Any(p => string.Equals(p.LordKey, lordKey, StringComparison.OrdinalIgnoreCase)))
        .ToList();
    }

    public static IReadOnlyList<TradeGoodEntry> BaronyReceivedGoods(IEnumerable<BaronyTradeTreaty> treaties)
    {
      var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var treaty in TradeTreatyApproval.EffectivelyApproved(treaties))
      {
        foreach (var paragraph in treaty.Paragraphs)
        {
          foreach (var key in paragraph.CounterpartyGrantsGoodKeys)
            keys.Add(key);
        }
      }

      return keys
        .Select(TradeGoodsCatalog.Find)
        .Where(g => g is not null)
        .Cast<TradeGoodEntry>()
        .ToList();
    }

    /// <summary>Economy additive from route: each route paragraph lord contributes max(1, Wealth − 2).</summary>
    public static decimal RouteEconomyBonus(BaronyTradeTreaty treaty)
    {
      decimal total = 0;
      var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (var paragraph in treaty.Paragraphs)
      {
        if (string.IsNullOrWhiteSpace(paragraph.LordKey) || !seen.Add(paragraph.LordKey))
          continue;
        var lord = KnownLordsCatalog.FindByKey(paragraph.LordKey);
        if (lord is not null)
          total += MarchMapTradePathfinder.EconomyFromWealth(lord.Wealth);
      }

      // Fallback for legacy empty paragraphs: use treaty counterparty only.
      if (total == 0m)
      {
        var counterparty = KnownLordsCatalog.FindByKey(treaty.CounterpartyLordKey);
        if (counterparty is not null)
          total += MarchMapTradePathfinder.EconomyFromWealth(counterparty.Wealth);
      }

      return total;
    }

    public static decimal TotalRouteEconomyBonus(IEnumerable<BaronyTradeTreaty> treaties) =>
      TradeTreatyApproval.EffectivelyApproved(treaties).Sum(RouteEconomyBonus);

    public static decimal TotalCustomsGoldPerTurn(IEnumerable<BaronyTradeTreaty> treaties) =>
      TradeTreatyApproval.EffectivelyApproved(treaties).SelectMany(t => t.Paragraphs)
        .Where(p => !p.IsDestination)
        .Sum(p => p.CustomsGoldPerTurn);

    /// <summary>
    /// Net sweetener gold/turn across treaties.
    /// Positive = barony pays net; negative = barony receives net.
    /// </summary>
    public static decimal TotalSweetenerGoldPerTurn(IEnumerable<BaronyTradeTreaty> treaties) =>
      TradeTreatyApproval.EffectivelyApproved(treaties).SelectMany(t => t.Paragraphs).Sum(p => p.SweetenerGoldPerTurn);

    /// <summary>Customs + sweetener net outflow (positive = gold leaving the barony).</summary>
    public static decimal TotalGoldOutflowPerTurn(IEnumerable<BaronyTradeTreaty> treaties) =>
      TotalCustomsGoldPerTurn(treaties) + TotalSweetenerGoldPerTurn(treaties);

    public static void SumTreatyBonuses(
      IEnumerable<BaronyTradeTreaty> treaties,
      out PpbVector additive,
      out PpbVector percent)
    {
      var treatyList = treaties as IList<BaronyTradeTreaty> ?? treaties.ToList();
      var goods = BaronyReceivedGoods(treatyList);
      TradeGoodsBonusAggregator.Sum(goods, out additive, out percent);
      var economy = TotalRouteEconomyBonus(treatyList);
      if (economy != 0m)
        additive[Ppb.Economy] += economy;

      // Net gold leaving the barony reduces Treasury (same sign as Domain Panel).
      var goldOutflow = TotalGoldOutflowPerTurn(treatyList);
      if (goldOutflow != 0m)
        additive[Ppb.Treasury] -= goldOutflow;
    }

    /// <summary>Ordered lord keys on the route (transit… then destination).</summary>
    public static IReadOnlyList<string> RouteLordKeys(BaronyTradeTreaty treaty) =>
      treaty.Paragraphs
        .Where(p => !string.IsNullOrWhiteSpace(p.LordKey))
        .Select(p => p.LordKey)
        .ToList();

    public static IReadOnlyList<string> ValidateTreaty(
      BaronyTradeTreaty treaty,
      IReadOnlyCollection<string> baronyAvailableGoodKeys,
      IReadOnlyCollection<string> borderLordKeys,
      IReadOnlyList<BaronyTradeTreaty> allTreaties,
      MarchMapDocument? map = null,
      string? playerSeatNodeId = null,
      IReadOnlyCollection<string>? blockedLordKeys = null)
    {
      var errors = new List<string>();
      var counterparty = KnownLordsCatalog.FindByKey(treaty.CounterpartyLordKey);
      if (counterparty is null)
      {
        errors.Add("Select a valid destination lord.");
        return errors;
      }

      var blocked = new HashSet<string>(
        blockedLordKeys ?? Array.Empty<string>(),
        StringComparer.OrdinalIgnoreCase);
      if (blocked.Contains(treaty.CounterpartyLordKey))
        errors.Add($"{counterparty.Name} refuses trade with this barony (blocked on the march map).");

      if (treaty.Paragraphs.Count == 0)
        errors.Add("Add at least one route paragraph (one per seat on the path).");

      var baronyAvailable = new HashSet<string>(baronyAvailableGoodKeys, StringComparer.OrdinalIgnoreCase);
      var destParagraphs = treaty.Paragraphs.Where(p => p.IsDestination).ToList();
      if (destParagraphs.Count != 1)
        errors.Add("Exactly one paragraph must be the destination seat.");
      else if (!string.Equals(destParagraphs[0].LordKey, treaty.CounterpartyLordKey, StringComparison.OrdinalIgnoreCase))
        errors.Add("Destination paragraph must address the treaty destination lord.");

      MarchMapNode? endNode = null;
      if (map is not null)
      {
        endNode = map.Nodes.FirstOrDefault(n =>
          string.Equals(n.LordKey, treaty.CounterpartyLordKey, StringComparison.OrdinalIgnoreCase));
        if (endNode is null)
          errors.Add($"{counterparty.Name} has no seat on the march map.");
      }

      var seenLords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var (paragraph, index) in treaty.Paragraphs.Select((p, i) => (p, i)))
      {
        var label = $"Paragraph {index + 1}";
        var addressee = KnownLordsCatalog.FindByKey(paragraph.LordKey);
        if (addressee is null)
        {
          errors.Add($"{label}: unknown addressee lord.");
          continue;
        }

        if (!seenLords.Add(paragraph.LordKey))
          errors.Add($"{label}: duplicate seat {addressee.Name} on the route.");

        if (blocked.Contains(paragraph.LordKey))
          errors.Add($"{label}: {addressee.Name} is blocked for this barony.");

        if (paragraph.CustomsGoldPerTurn < 0m)
          errors.Add($"{label}: customs cannot be negative.");
        if (paragraph.IsDestination && paragraph.CustomsGoldPerTurn != 0m)
          errors.Add($"{label}: destination seat does not charge transit customs.");

        var role = TradeTreatyParagraphLabels.RoleLabel(paragraph);
        if (paragraph.IsDestination &&
            paragraph.BaronyGrantsGoodKeys.Count == 0 &&
            paragraph.CounterpartyGrantsGoodKeys.Count == 0)
          errors.Add($"{label} ({role} · {addressee.Holdings}): choose at least one good exchanged with the destination lord.");

        foreach (var key in paragraph.BaronyGrantsGoodKeys)
        {
          if (!baronyAvailable.Contains(key))
            errors.Add($"{label}: barony cannot grant “{GoodName(key)}” to {addressee.Name}.");
        }

        var lordProduced = new HashSet<string>(KnownLordsCatalog.ProducedGoodKeys(addressee), StringComparer.OrdinalIgnoreCase);
        foreach (var key in paragraph.CounterpartyGrantsGoodKeys)
        {
          if (!lordProduced.Contains(key))
            errors.Add($"{label}: {addressee.Name} does not produce “{GoodName(key)}”.");
        }
      }

      if (map is not null && !string.IsNullOrWhiteSpace(playerSeatNodeId) && endNode is not null)
      {
        var actualLords = RouteLordKeys(treaty);
        if (!MarchMapTradePathfinder.RouteLordSequenceExists(map, playerSeatNodeId, actualLords, blocked))
        {
          errors.Add("Route paragraphs do not match a valid path on the march map. Redesign the route on the March map.");
        }
      }
      else
      {
        var transitKeys = treaty.Paragraphs.Where(p => !p.IsDestination).Select(p => p.LordKey).ToList();
        if (!EasternMarchLordAdjacency.HasPath(borderLordKeys, treaty.CounterpartyLordKey, transitKeys))
          errors.Add("Trade route is not reachable from your border lords (check neighbours / transit).");
      }

      foreach (var key in treaty.Paragraphs.SelectMany(p => p.BaronyGrantsGoodKeys).Distinct(StringComparer.OrdinalIgnoreCase))
      {
        var effective = EffectiveTreaties(allTreaties, treaty);
        var grantees = CountBaronyGrantees(key, effective);
        if (grantees > MaxGranteesPerProducedGood)
          errors.Add($"Barony already shares “{GoodName(key)}”.");
      }

      foreach (var paragraph in treaty.Paragraphs)
      {
        var addressee = KnownLordsCatalog.FindByKey(paragraph.LordKey);
        if (addressee is null)
          continue;
        foreach (var key in paragraph.CounterpartyGrantsGoodKeys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
          var effective = EffectiveTreaties(allTreaties, treaty);
          var grantees = CountLordGrantees(paragraph.LordKey, key, effective);
          if (grantees > MaxGranteesPerProducedGood)
            errors.Add($"{addressee.Name} already shares “{GoodName(key)}”.");
        }
      }

      return errors;
    }

    private static IReadOnlyList<BaronyTradeTreaty> EffectiveTreaties(
      IReadOnlyList<BaronyTradeTreaty> allTreaties,
      BaronyTradeTreaty treaty) =>
      allTreaties
        .Where(t => !string.Equals(t.Id, treaty.Id, StringComparison.OrdinalIgnoreCase))
        .Where(TradeTreatyApproval.IsEffectivelyApproved)
        .Append(treaty)
        .ToList();

    private static int CountBaronyGrantees(string goodKey, IReadOnlyList<BaronyTradeTreaty> treaties)
    {
      var lords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var t in treaties)
      {
        foreach (var p in t.Paragraphs)
        {
          if (p.BaronyGrantsGoodKeys.Any(k => string.Equals(k, goodKey, StringComparison.OrdinalIgnoreCase)))
            lords.Add(p.LordKey);
        }
      }

      return lords.Count;
    }

    private static int CountLordGrantees(string lordKey, string goodKey, IReadOnlyList<BaronyTradeTreaty> treaties)
    {
      var count = 0;
      foreach (var t in treaties)
      {
        if (t.Paragraphs.Any(p =>
              string.Equals(p.LordKey, lordKey, StringComparison.OrdinalIgnoreCase) &&
              p.CounterpartyGrantsGoodKeys.Any(k => string.Equals(k, goodKey, StringComparison.OrdinalIgnoreCase))))
          count++;
      }

      return count;
    }

    private static string GoodName(string key) =>
      TradeGoodsCatalog.Find(key)?.Name ?? key;
  }
}
