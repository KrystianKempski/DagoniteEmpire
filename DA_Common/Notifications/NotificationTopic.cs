using DA_Common.Localization;

namespace DA_Common.Notifications
{
    /// <summary>Push notification categories a device can opt into.</summary>
    public static class NotificationTopic
    {
        /// <summary>New post in a chapter the player takes part in.</summary>
        public const string Posts = "posts";

        /// <summary>Game Master resolved the barony turn.</summary>
        public const string TurnResolved = "turn-resolved";

        /// <summary>New message in a Questions-for-GM thread.</summary>
        public const string GmQuestion = "gm-question";

        /// <summary>Tactical battle advanced to the next turn / phase.</summary>
        public const string BattleTurn = "battle-turn";

        /// <summary>Private chat message.</summary>
        public const string Chat = "chat";

        /// <summary>A letter was delivered in a baron correspondence thread.</summary>
        public const string BaronLetter = "baron-letter";

        public static readonly string[] All =
        {
            Posts,
            TurnResolved,
            GmQuestion,
            BattleTurn,
            Chat,
            BaronLetter,
        };

        public static string? Normalize(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            foreach (var k in All)
            {
                if (string.Equals(k, key.Trim(), StringComparison.OrdinalIgnoreCase))
                    return k;
            }

            return null;
        }

        public static string DisplayName(string? key) => Normalize(key) switch
        {
            Posts => Loc.T("New posts in my chapters"),
            TurnResolved => Loc.T("Barony turn resolved"),
            GmQuestion => Loc.T("Answers to Questions for GM"),
            BattleTurn => Loc.T("My turn in a battle"),
            Chat => Loc.T("Private messages"),
            BaronLetter => Loc.T("Baron letters"),
            _ => Loc.T("Notifications"),
        };
    }
}
