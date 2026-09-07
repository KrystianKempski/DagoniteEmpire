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
    /// <param name="AuthorCharacterId">Author, excluded from the recipients.</param>
    public sealed record ChapterPostAdded(int ChapterId, int AuthorCharacterId) : GameNotification
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
    /// <param name="FromGameMaster">True when the GM answered, false when a player asked.</param>
    public sealed record GmQuestionPosted(int ThreadId, bool FromGameMaster) : GameNotification
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

    /// <summary>A campaign chat message was sent.</summary>
    /// <param name="CampaignId">Campaign the message belongs to.</param>
    /// <param name="SenderCharacterId">Author character, excluded from recipients.</param>
    /// <param name="RecipientCharacterId">
    /// Direct addressee; null means the party channel (all campaign players + GM).
    /// </param>
    public sealed record CampaignChatMessageSent(
        int CampaignId,
        int SenderCharacterId,
        int? RecipientCharacterId) : GameNotification
    {
        public override string Topic => NotificationTopic.Chat;
    }

    /// <summary>A letter was delivered in a baron correspondence thread.</summary>
    /// <param name="ThreadId">Thread the letter belongs to.</param>
    /// <param name="IsInbound">
    /// True when a correspondent wrote to the baron (notify the baron), false when the baron
    /// wrote out (notify the Game Master, who plays the correspondents).
    /// </param>
    public sealed record BaronLetterDelivered(int ThreadId, bool IsInbound) : GameNotification
    {
        public override string Topic => NotificationTopic.BaronLetter;
    }
}
