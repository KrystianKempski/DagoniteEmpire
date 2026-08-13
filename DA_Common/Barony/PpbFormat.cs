namespace DA_Common.Barony
{
    /// <summary>Display / storage rounding for PPB values (nearest 0.1).</summary>
    public static class PpbFormat
    {
        public const int Digits = 1;

        public static decimal Round(decimal v) => decimal.Round(v, Digits);

        public static string Number(decimal v)
        {
            var r = Round(v);
            return r == decimal.Truncate(r)
                ? ((long)r).ToString()
                : r.ToString("0.#");
        }

        public static string Additive(decimal v)
            => Round(v) == 0m ? "·" : (v > 0 ? "+" : "") + Number(v);

        public static string Percent(decimal v)
            => Round(v) == 0m ? "·" : (v > 0 ? "+" : "") + Number(v) + "%";
    }
}
