namespace DA_Common.Barony
{
    /// <summary>
    /// Fief gold dues:
    /// <list type="bullet">
    /// <item><b>Liege tribute</b> — baron pays senior a share of gross income (Budget expense).</item>
    /// <item><b>Vassal dues</b> — baron keeps only a share of gold from villages on vassal fiefs.</item>
    /// </list>
    /// Both default to 15%.
    /// </summary>
    public static class FiefTributeFormulas
    {
        public const decimal DefaultPercent = 15m;
        public const decimal MinPercent = 0m;
        public const decimal MaxPercent = 100m;

        public static decimal ClampPercent(decimal percent)
            => Math.Clamp(percent, MinPercent, MaxPercent);

        public static decimal ShareFactor(decimal percent)
            => ClampPercent(percent) / 100m;

        /// <summary>Liege dues from gross income (never negative).</summary>
        public static decimal ComputeTribute(decimal grossIncome, decimal liegeTributePercent)
        {
            var baseIncome = Math.Max(0m, grossIncome);
            return PpbFormat.Round(baseIncome * ShareFactor(liegeTributePercent));
        }

        /// <summary>Baron's share of a village's full treasury yield on a vassal fief.</summary>
        public static decimal ApplyVassalShare(decimal fullTreasury, decimal vassalTributePercent)
            => PpbFormat.Round(fullTreasury * ShareFactor(vassalTributePercent));

        public static string ExplainLiege(decimal grossIncome, decimal liegeTributePercent, decimal tribute)
            => $"= Gross income × {ClampPercent(liegeTributePercent):0.#}%\n"
               + $"Gross income (Domain Panel gold income before expenses) = {PpbFormat.Number(grossIncome)}.\n"
               + $"Tribute to senior = {PpbFormat.Number(tribute)} gold.";

        public static string ExplainVassalShare(decimal fullTreasury, decimal vassalTributePercent, decimal kept)
            => $"= Village gold × {ClampPercent(vassalTributePercent):0.#}%\n"
               + $"Full village gold = {PpbFormat.Number(fullTreasury)}; baron keeps {PpbFormat.Number(kept)} "
               + "(vassal fief).";

        public static string LiegeCatalogDescription =>
            "Barons pay their senior a share of gross gold income before expenses. "
            + $"Default {DefaultPercent:0}%; MG can change the rate on Budget.";

        public static string VassalCatalogDescription =>
            "Villages on vassal fiefs (not baron demesne) contribute only this share of their gold to the baron. "
            + $"Default {DefaultPercent:0}%; MG can change the rate on Budget.";

        // Back-compat aliases used by Budget tooltips.
        public static string Explain(decimal grossIncome, decimal liegeTributePercent, decimal tribute)
            => ExplainLiege(grossIncome, liegeTributePercent, tribute);

        public static string CatalogDescription => LiegeCatalogDescription;
    }
}
