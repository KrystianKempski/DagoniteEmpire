namespace DA_Common.Barony
{
    public readonly struct BaronLetterStatus
    {
        public const string Draft = "Draft";
        public const string Sent = "Sent";

        public static readonly string[] All = { Draft, Sent };
    }

    /// <summary>Where the correspondent lives (flavor / travel context).</summary>
    public readonly struct BaronLetterReplyRegion
    {
        public const string EasternMarch = "Eastern March";
        public const string Empire = "Empire";
        public const string Other = "Other";

        public static readonly string[] All = { EasternMarch, Empire, Other };
    }

    public static class BaronLetterRules
    {
        /// <summary>
        /// Max letters the baron may receive from the same correspondent in one turn.
        /// Outbound letters from the baron are unlimited.
        /// </summary>
        public const int MaxLettersFromSameCorrespondentPerTurn = 3;

        /// <summary>
        /// A delivered inbound message counts as one received letter from that correspondent this turn.
        /// </summary>
        public static bool CountsAsReceivedThisTurn(
            bool isDraft,
            bool isInbound,
            int turnNumber,
            int currentTurn)
        {
            if (isDraft || currentTurn <= 0 || !isInbound)
                return false;

            return turnNumber == currentTurn;
        }

        public static bool SameCorrespondent(
            int? relationIdA,
            string? nameA,
            int? relationIdB,
            string? nameB)
        {
            if (relationIdA is int a && relationIdB is int b && a == b)
                return true;

            var na = (nameA ?? "").Trim();
            var nb = (nameB ?? "").Trim();
            return !string.IsNullOrEmpty(na)
                && string.Equals(na, nb, StringComparison.OrdinalIgnoreCase);
        }
    }
}
