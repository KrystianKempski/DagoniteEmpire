namespace DA_Common.Barony
{
  /// <summary>
  /// One treaty paragraph = one seat on the trade route (transit toll or destination).
  /// </summary>
  public sealed class TradeTreatyParagraph
  {
    /// <summary>Lord this paragraph addresses (transit seat or destination).</summary>
    public string LordKey { get; set; } = string.Empty;

    /// <summary>True when this paragraph is the route destination (final counterparty).</summary>
    public bool IsDestination { get; set; }

    /// <summary>
    /// Customs gold/turn for a transit seat (from the March map node default). Ignored (0) for destination.
    /// Not editable when creating/editing a treaty route.
    /// </summary>
    public decimal CustomsGoldPerTurn { get; set; }

    /// <summary>
    /// Optional gold sweetener on the goods exchange with this seat.
    /// Positive = barony pays this lord; negative = barony receives gold from this lord.
    /// </summary>
    public decimal SweetenerGoldPerTurn { get; set; }

    public List<string> BaronyGrantsGoodKeys { get; set; } = new();

    /// <summary>Goods this paragraph's lord grants to the barony.</summary>
    public List<string> CounterpartyGrantsGoodKeys { get; set; } = new();

    /// <summary>
    /// Legacy field (pre node-paragraphs). Migrated into separate paragraphs on load.
    /// </summary>
    public List<TradeTreatyTransitLeg> TransitLegs { get; set; } = new();
  }

  /// <summary>Legacy transit leg stored inside old paragraphs.</summary>
  public sealed class TradeTreatyTransitLeg
  {
    public string LordKey { get; set; } = string.Empty;
    public decimal CustomsGoldPerTurn { get; set; }
  }

  public sealed class BaronyTradeTreaty
  {
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    /// <summary>Final destination lord (same as the destination paragraph's <see cref="TradeTreatyParagraph.LordKey"/>).</summary>
    public string CounterpartyLordKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public List<TradeTreatyParagraph> Paragraphs { get; set; } = new();
  }

  public static class TradeTreatyParagraphLabels
  {
    public static string RoleLabel(TradeTreatyParagraph paragraph) =>
      paragraph.IsDestination ? "Partner" : "Transit";

    public static string AddresseeHeading(TradeTreatyParagraph paragraph)
    {
      var lord = KnownLordsCatalog.FindByKey(paragraph.LordKey);
      if (lord is null)
        return $"{RoleLabel(paragraph)} · unknown lord";

      return $"{RoleLabel(paragraph)} · {lord.Name} {lord.House} ({lord.Holdings})";
    }

    public static string AddresseeShort(TradeTreatyParagraph paragraph)
    {
      var lord = KnownLordsCatalog.FindByKey(paragraph.LordKey);
      if (lord is null)
        return paragraph.LordKey;
      return $"{lord.Name} {lord.House} ({lord.Holdings})";
    }

    /// <summary>Human-readable sweetener line, or null when zero.</summary>
    public static string? SweetenerLabel(TradeTreatyParagraph paragraph)
    {
      if (paragraph.SweetenerGoldPerTurn == 0m)
        return null;

      var amount = Math.Abs(paragraph.SweetenerGoldPerTurn).ToString("0.##");
      return paragraph.SweetenerGoldPerTurn > 0m
        ? $"Sweetener: barony pays {amount} gold/turn"
        : $"Sweetener: barony receives {amount} gold/turn";
    }
  }
}
