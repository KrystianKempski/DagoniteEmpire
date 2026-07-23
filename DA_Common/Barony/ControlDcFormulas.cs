namespace DA_Common.Barony
{
    /// <summary>
    /// Control DC and loyalty-vs-control unrest checks at turn resolve.
    /// ControlDc = Size + 2×Population + 5.
    /// Loyalty test (when Stability ≤ 0): Loyalty + d20 − ControlDc.
    /// </summary>
    public static class ControlDcFormulas
    {
        public const int FlatBonus = 5;
        public const int PopulationFactor = 2;

        public static int ControlDc(int size, int population)
            => Math.Max(0, size) + PopulationFactor * Math.Max(0, population) + FlatBonus;

        /// <summary>Leading digit of a positive DC (15 → 1, 5 → 5, 100 → 1).</summary>
        public static int FirstDigit(int controlDc)
        {
            var n = Math.Abs(controlDc);
            if (n == 0)
                return 0;
            while (n >= 10)
                n /= 10;
            return n;
        }

        /// <summary>Threshold for Unrest +2: −(2 × first digit of DC).</summary>
        public static int SevereFailThreshold(int controlDc)
            => -(2 * FirstDigit(controlDc));

        public static int TestResult(decimal loyalty, int d20, int controlDc)
            => (int)decimal.Round(loyalty, 0, MidpointRounding.AwayFromZero) + d20 - controlDc;

        /// <summary>
        /// Unrest increase from a loyalty test result.
        /// ≥0 → 0; &lt;0 → +1; ≤ severe threshold → +2.
        /// </summary>
        public static int UnrestDelta(int testResult, int controlDc)
        {
            if (testResult >= 0)
                return 0;
            if (testResult <= SevereFailThreshold(controlDc))
                return 2;
            return 1;
        }

        public static int UnrestDelta(decimal loyalty, int d20, int controlDc)
            => UnrestDelta(TestResult(loyalty, d20, controlDc), controlDc);
    }
}
