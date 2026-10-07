namespace DA_Common.Notifications
{
    /// <summary>
    /// A game event worth pushing to a player's phone. Raised on the request thread that saved
    /// the change and handled later by a background worker, so a descriptor carries ids only —
    /// the worker resolves the recipients and builds the text itself.
    /// </summary>
    public abstract record GameNotification
    {
        /// <summary>
        /// UI culture of the request that raised the event. The worker runs outside request
        /// localization, so without this the text would always fall back to English.
        /// </summary>
        public string? CultureName { get; init; }

        /// <summary>Category used to filter devices that opted out of this kind of event.</summary>
        public abstract string Topic { get; }
    }

    /// <summary>A new post was added to a chapter.</summary>
    /// <param name="ChapterId">Chapter that received the post.</param>
    /// <param name="PostId">The post itself, so every entry alerts separately on iOS.</param>
    /// <param name="AuthorCharacterId">Author, excluded from the recipients.</param>
    public sealed record ChapterPostAdded(int ChapterId, int PostId, int AuthorCharacterId) : GameNotification
    {
        public override string Topic => NotificationTopic.Posts;
    }

    /// <summary>The Game Master resolved a barony turn.</summary>
    /// <param name="BaronyId">Barony whose turn advanced.</param>
    /// <param name="TurnNumber">Turn number after resolving.</param>
    public sealed record BaronyTurnResolved(int BaronyId, int TurnNumber) : GameNotification
    {
        public override string Topic => NotificationTopic.TurnResolved;
    }

    /// <summary>A message was posted in a Questions-for-GM thread.</summary>
    /// <param name="ThreadId">Thread that received the message.</param>
    /// <param name="MessageId">The message itself, so every entry in a thread alerts separately.</param>
    /// <param name="FromGameMaster">True when the GM answered, false when a player asked.</param>
    public sealed record GmQuestionPosted(int ThreadId, int MessageId, bool FromGameMaster) : GameNotification
    {
        public override string Topic => NotificationTopic.GmQuestion;
    }

    /// <summary>A tactical battle advanced and it is now someone else's move.</summary>
    /// <param name="BaronyId">Barony the battle belongs to.</param>
    /// <param name="Round">Battle round after advancing.</param>
    /// <param name="SubPhase">Battle sub-phase, see <c>BaronyBattleSubPhases</c>.</param>
    /// <param name="ActiveUnitLabel">Unit whose move it is, when a single unit is up.</param>
    /// <param name="ActiveUnitIsEnemy">
    /// True when the unit belongs to the enemy, which means the Game Master moves it rather than
    /// the baron.
    /// </param>
    public sealed record BattleTurnAdvanced(
        int BaronyId,
        int Round,
        string SubPhase,
        string? ActiveUnitLabel,
        bool ActiveUnitIsEnemy) : GameNotification
    {
        public override string Topic => NotificationTopic.BattleTurn;
    }

    /// <summary>A chat message was posted. Chat is account-to-account; characters play no part in it.</summary>
    /// <param name="ConversationId">Thread that received the message.</param>
    /// <param name="MessageId">
    /// The message itself. Part of the push tag so every message alerts separately — iOS silently
    /// replaces a notification that reuses a tag, which used to swallow follow-up messages.
    /// </param>
    /// <param name="SenderUserId">Author account, excluded from the recipients.</param>
    public sealed record ChatMessagePosted(
        long ConversationId,
        long MessageId,
        string SenderUserId) : GameNotification
    {
        public override string Topic => NotificationTopic.Chat;
    }

    /// <summary>A letter was delivered in a baron correspondence thread.</summary>
    /// <param name="ThreadId">Thread the letter belongs to.</param>
    /// <param name="MessageId">The letter itself, so every letter in a thread alerts separately.</param>
    /// <param name="IsInbound">
    /// True when a correspondent wrote to the baron (notify the baron), false when the baron
    /// wrote out (notify the Game Master, who plays the correspondents).
    /// </param>
    public sealed record BaronLetterDelivered(int ThreadId, int MessageId, bool IsInbound) : GameNotification
    {
        public override string Topic => NotificationTopic.BaronLetter;
    }

    /// <summary>A spoken turn was posted in an Audience Hall / Audiences thread.</summary>
    /// <param name="AudienceId">Audience that received the exchange.</param>
    /// <param name="ExchangeId">The spoken turn itself, so every entry alerts separately.</param>
    /// <param name="IsFromPetitioner">
    /// True when the GM side spoke (petitioner / NPC / Game Master) — notify the baron.
    /// False when the baron spoke — notify the Game Master.
    /// </param>
    public sealed record BaronAudienceExchangePosted(
        int AudienceId,
        int ExchangeId,
        bool IsFromPetitioner) : GameNotification
    {
        public override string Topic => NotificationTopic.BaronAudience;
    }
}
