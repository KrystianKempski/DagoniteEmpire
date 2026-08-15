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
            => $"= Dochód brutto × {ClampPercent(liegeTributePercent):0.#}%\n"
               + $"Dochód brutto (dochód w złocie z Panelu Domeny przed wydatkami) = {PpbFormat.Number(grossIncome)}.\n"
               + $"Trybut dla suzerena = {PpbFormat.Number(tribute)} złota.";

        public static string ExplainVassalShare(decimal fullTreasury, decimal vassalTributePercent, decimal kept)
            => $"= Złoto wioski × {ClampPercent(vassalTributePercent):0.#}%\n"
               + $"Pełne złoto wioski = {PpbFormat.Number(fullTreasury)}; baron zachowuje {PpbFormat.Number(kept)} "
               + "(lenno wasala).";

        public static string LiegeCatalogDescription =>
            "Baronowie płacą swojemu suzerenowi udział w dochodzie brutto w złocie przed wydatkami. "
            + $"Domyślnie {DefaultPercent:0}%; MG może zmienić stawkę w Budżecie.";

        public static string VassalCatalogDescription =>
            "Wioski na lennach wasali (nie na domenie barona) oddają baronowi tylko ten udział swojego złota. "
            + $"Domyślnie {DefaultPercent:0}%; MG może zmienić stawkę w Budżecie.";

        // Back-compat aliases used by Budget tooltips.
        public static string Explain(decimal grossIncome, decimal liegeTributePercent, decimal tribute)
            => ExplainLiege(grossIncome, liegeTributePercent, tribute);

        public static string CatalogDescription => LiegeCatalogDescription;
    }
}
