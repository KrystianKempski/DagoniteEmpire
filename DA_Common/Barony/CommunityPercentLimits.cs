namespace DA_Common.Barony
{
    /// <summary>
    /// Caps for Community Penalties and Bonuses percent modifiers.
    /// Per-source penalties cannot go below −40%; the section Σ cannot go below −80%.
    /// </summary>
    public static class CommunityPercentLimits
    {
        public const decimal PerSourcePenaltyFloor = -40m;
        public const decimal SectionPenaltyFloor = -80m;

        /// <summary>Clamp a single-source percent penalty (non-negative values unchanged).</summary>
        public static decimal ClampSourcePenalty(decimal percent) =>
            percent >= 0m ? percent : Math.Max(percent, PerSourcePenaltyFloor);

        /// <summary>
        /// Clamp each PPB key so the community section percent sum is never worse than
        /// <see cref="SectionPenaltyFloor"/> (e.g. −120% → −80%).
        /// </summary>
        public static PpbVector CapSectionPenaltySum(PpbVector? sum)
        {
            var result = (sum ?? new PpbVector()).Clone();
            result.EnsureSize();
            foreach (var info in PpbCatalog.All)
            {
                if (result[info.Key] < SectionPenaltyFloor)
                    result[info.Key] = SectionPenaltyFloor;
            }
            return result;
        }
    }
}
