namespace DA_Common.Barony
{
    /// <summary>CSS classes for PPB table cell polarity (positive / negative / zero).</summary>
    public static class PpbCellStyle
    {
        /// <summary>
        /// Corruption is inverted: increases are bad (red), decreases are good (green).
        /// All other PPB: positive = green, negative = red.
        /// </summary>
        public static string ValueClass(Ppb key, decimal v)
        {
            if (v == 0m)
                return "ppb-cell-zero";

            if (key == Ppb.Corruption)
                return v > 0m ? "ppb-cell-neg" : "ppb-cell-pos";

            return v > 0m ? "ppb-cell-pos" : "ppb-cell-neg";
        }

        /// <summary>Summary chip modifier class (<c>ppb-chip--pos</c> / <c>ppb-chip--neg</c>).</summary>
        public static string ChipClass(Ppb key, decimal v)
        {
            if (v == 0m)
                return string.Empty;

            if (key == Ppb.Corruption)
                return v > 0m ? "ppb-chip--neg" : "ppb-chip--pos";

            return v > 0m ? "ppb-chip--pos" : "ppb-chip--neg";
        }

        /// <summary>Standard polarity when there is no PPB column context (e.g. population).</summary>
        public static string ValueClassStandard(decimal v)
        {
            if (v == 0m)
                return "ppb-cell-zero";
            return v > 0m ? "ppb-cell-pos" : "ppb-cell-neg";
        }
    }
}
