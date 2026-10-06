namespace DA_DataAccess.Chat
{
    /// <summary>
    /// Builds <see cref="ChatConversation.PairKey"/>. The key is the only thing that decides whether
    /// a thread already exists, so it must be stable and order-independent — building it in one place
    /// keeps "get or create" races harmless: the loser of the race violates the unique index and retries
    /// the read.
    /// </summary>
    public static class ChatPairKey
    {
        /// <summary>Stands in for the Game Master side, which is a role rather than an account.</summary>
        public const string GameMasterToken = "gm";

        public static string Direct(string? userIdA, string? userIdB)
        {
            var a = Side(userIdA);
            var b = Side(userIdB);

            return string.CompareOrdinal(a, b) <= 0 ? $"d:{a}|{b}" : $"d:{b}|{a}";
        }

        public static string Party(int campaignId) => $"p:{campaignId}";

        /// <summary>Null stands for the Game Master role; see <see cref="ChatParticipant.IsGameMasterRole"/>.</summary>
        private static string Side(string? userId) =>
            string.IsNullOrWhiteSpace(userId) ? GameMasterToken : $"u:{userId}";
    }
}
