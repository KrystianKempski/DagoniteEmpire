using System.Globalization;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_Common.Localization;
using DA_Common.Notifications;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;
using DA_Models.NotificationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DA_Business.Services
{
    /// <inheritdoc cref="IGameNotificationDispatcher"/>
    public class GameNotificationDispatcher : IGameNotificationDispatcher
    {
        /// <summary>Recipients plus the text to show them. Null plan means "nobody to tell".</summary>
        private sealed record Plan(List<string> UserIds, PushNotificationDTO Payload);

        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IPushNotificationService _push;
        private readonly NotificationRecipientLookup _recipients;
        private readonly ILogger<GameNotificationDispatcher> _logger;

        public GameNotificationDispatcher(
            IDbContextFactory<ApplicationDbContext> db,
            IPushNotificationService push,
            NotificationRecipientLookup recipients,
            ILogger<GameNotificationDispatcher> logger)
        {
            _db = db;
            _push = push;
            _recipients = recipients;
            _logger = logger;
        }

        public async Task<int> Dispatch(
            GameNotification notification,
            CancellationToken cancellationToken = default)
        {
            if (notification is null || !_push.IsConfigured)
                return 0;

            ApplyCulture(notification.CultureName);

            var plan = notification switch
            {
                BaronLetterDelivered n => await PlanBaronLetter(n, cancellationToken),
                BaronAudienceExchangePosted n => await PlanBaronAudience(n, cancellationToken),
                ChapterPostAdded n => await PlanChapterPost(n, cancellationToken),
                BaronyTurnResolved n => await PlanTurnResolved(n, cancellationToken),
                ChatMessagePosted n => await PlanChatMessage(n, cancellationToken),
                GmQuestionPosted n => await PlanGmQuestion(n, cancellationToken),
                BattleTurnAdvanced n => await PlanBattleTurn(n, cancellationToken),
                _ => null,
            };

            if (plan is null || plan.UserIds.Count == 0)
                return 0;

            return await _push.SendToUsers(plan.UserIds, plan.Payload, notification.Topic);
        }

        /// <summary>
        /// Barony pages resolve their barony from the selected character, so a link that does not name
        /// one opens whichever barony the reader browsed last — and silently drops the thread id. Adding
        /// the baron character lets the app switch to the right barony before rendering. Matters for a
        /// Game Master running several baronies and for a player holding more than one baron character.
        /// </summary>
        private static string BaronyUrl(string path, int characterId) =>
            characterId <= 0
                ? path
                : $"{path}{(path.Contains('?') ? '&' : '?')}character={characterId}";

        /// <summary>
        /// The worker thread has no request culture, so restore the one captured when the event
        /// was raised. Unknown names are ignored rather than fatal.
        /// </summary>
        private void ApplyCulture(string? cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                return;

            try
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                _logger.LogDebug("Unknown notification culture {Culture}.", cultureName);
            }
        }

        private async Task<Plan?> PlanBaronLetter(
            BaronLetterDelivered n,
            CancellationToken ct)
        {
            using var ctx = await _db.CreateDbContextAsync(ct);
            var thread = await (
                from t in ctx.BaronLetterThreads.AsNoTracking()
                join b in ctx.Baronies.AsNoTracking() on t.BaronyId equals b.Id
                where t.Id == n.ThreadId
                select new
                {
                    t.Title,
                    t.CorrespondentName,
                    BaronyName = b.Name,
                    t.BaronyId,
                    b.CharacterId,
                }
            ).FirstOrDefaultAsync(ct);

            if (thread is null)
                return null;

            var url = BaronyUrl($"/barony/letters?thread={n.ThreadId}", thread.CharacterId);
            // A tag per letter, with the thread as ThreadKey — see PlanChatMessage for why.
            var tag = $"baron-letter-{n.ThreadId}-{n.MessageId}";
            var threadKey = $"baron-letter-{n.ThreadId}";
            var correspondent = string.IsNullOrWhiteSpace(thread.CorrespondentName)
                ? Loc.T("a correspondent")
                : thread.CorrespondentName;

            if (n.IsInbound)
            {
                // A correspondent answered — only the baron who owns the thread cares.
                var baronId = await _recipients.UserIdForBarony(thread.BaronyId, ct);
                if (baronId is null)
                    return null;

                return new Plan(
                    new List<string> { baronId },
                    new PushNotificationDTO
                    {
                        Title = Loc.T("New letter from {0}", correspondent),
                        Body = thread.Title,
                        Url = url,
                        Tag = tag,
                        ThreadKey = threadKey,
                    });
            }

            // The baron wrote out; the Game Master plays every correspondent.
            var gmIds = await _recipients.GameMasterUserIds(ct);
            if (gmIds.Count == 0)
                return null;

            return new Plan(
                gmIds,
                new PushNotificationDTO
                {
                    Title = Loc.T("Letter from the baron of {0}", thread.BaronyName),
                    Body = $"{correspondent} — {thread.Title}",
                    Url = url,
                    Tag = tag,
                    ThreadKey = threadKey,
                });
        }

        private async Task<Plan?> PlanBaronAudience(
            BaronAudienceExchangePosted n,
            CancellationToken ct)
        {
            using var ctx = await _db.CreateDbContextAsync(ct);
            var audience = await (
                from a in ctx.BaronAudiences.AsNoTracking()
                join b in ctx.Baronies.AsNoTracking() on a.BaronyId equals b.Id
                where a.Id == n.AudienceId
                select new
                {
                    a.Title,
                    a.PetitionerName,
                    BaronyName = b.Name,
                    a.BaronyId,
                    b.CharacterId,
                }
            ).FirstOrDefaultAsync(ct);

            if (audience is null)
                return null;

            var url = BaronyUrl($"/barony/audience-hall?audience={n.AudienceId}", audience.CharacterId);
            var tag = $"baron-audience-{n.AudienceId}-{n.ExchangeId}";
            var threadKey = $"baron-audience-{n.AudienceId}";
            var petitioner = string.IsNullOrWhiteSpace(audience.PetitionerName)
                ? Loc.T("a petitioner")
                : audience.PetitionerName;
            var title = string.IsNullOrWhiteSpace(audience.Title)
                ? Loc.T("Untitled audience")
                : audience.Title;

            if (n.IsFromPetitioner)
            {
                // GM / petitioner spoke — only the baron who owns the seat cares.
                var baronId = await _recipients.UserIdForBarony(audience.BaronyId, ct);
                if (baronId is null)
                    return null;

                return new Plan(
                    new List<string> { baronId },
                    new PushNotificationDTO
                    {
                        Title = Loc.T("Audience reply from {0}", petitioner),
                        Body = title,
                        Url = url,
                        Tag = tag,
                        ThreadKey = threadKey,
                    });
            }

            // The baron answered; the Game Master plays every petitioner / NPC.
            var gmIds = await _recipients.GameMasterUserIds(ct);
            if (gmIds.Count == 0)
                return null;

            return new Plan(
                gmIds,
                new PushNotificationDTO
                {
                    Title = Loc.T("Audience reply from the baron of {0}", audience.BaronyName),
                    Body = $"{petitioner} — {title}",
                    Url = url,
                    Tag = tag,
                    ThreadKey = threadKey,
                });
        }

        private async Task<Plan?> PlanChapterPost(ChapterPostAdded n, CancellationToken ct)
        {
            string? chapterName;
            string? authorName;
            List<string?> ownerNames;

            using (var ctx = await _db.CreateDbContextAsync(ct))
            {
                var chapter = await ctx.Chapters
                    .AsNoTracking()
                    .Where(c => c.Id == n.ChapterId)
                    .Select(c => new
                    {
                        c.Name,
                        Participants = c.Characters
                            .Where(ch => ch.Id != n.AuthorCharacterId)
                            .Select(ch => ch.UserName)
                            .ToList(),
                    })
                    .FirstOrDefaultAsync(ct);

                if (chapter is null)
                    return null;

                chapterName = chapter.Name;
                ownerNames = chapter.Participants.Cast<string?>().ToList();

                authorName = await ctx.Characters
                    .AsNoTracking()
                    .Where(c => c.Id == n.AuthorCharacterId)
                    .Select(c => c.NPCName)
                    .FirstOrDefaultAsync(ct);
            }

            // Several characters of one player in the same chapter must not mean several pushes.
            var userIds = await _recipients.UserIdsForUserNames(ownerNames, ct);
            if (userIds.Count == 0)
                return null;

            return new Plan(
                userIds,
                new PushNotificationDTO
                {
                    Title = string.IsNullOrWhiteSpace(chapterName)
                        ? Loc.T("New post")
                        : Loc.T("New post in {0}", chapterName),
                    Body = string.IsNullOrWhiteSpace(authorName)
                        ? Loc.T("Someone added a new post.")
                        : Loc.T("{0} added a new post.", authorName),
                    Url = $"/chapter/{n.ChapterId}",
                    Tag = $"chapter-{n.ChapterId}",
                });
        }

        private async Task<Plan?> PlanTurnResolved(BaronyTurnResolved n, CancellationToken ct)
        {
            string? baronyName;
            int characterId;
            using (var ctx = await _db.CreateDbContextAsync(ct))
            {
                var barony = await ctx.Baronies
                    .AsNoTracking()
                    .Where(b => b.Id == n.BaronyId)
                    .Select(b => new { b.Name, b.CharacterId })
                    .FirstOrDefaultAsync(ct);

                baronyName = barony?.Name;
                characterId = barony?.CharacterId ?? 0;
            }

            if (baronyName is null)
                return null;

            var baronId = await _recipients.UserIdForBarony(n.BaronyId, ct);
            if (baronId is null)
                return null;

            return new Plan(
                new List<string> { baronId },
                new PushNotificationDTO
                {
                    Title = Loc.T("Turn {0} resolved", n.TurnNumber),
                    Body = Loc.T("{0} is ready for your orders.", baronyName),
                    Url = BaronyUrl("/barony", characterId),
                    // One entry per barony — a newer turn replaces the previous, which is the point here.
                    Tag = $"turn-{n.BaronyId}",
                });
        }

        private async Task<Plan?> PlanGmQuestion(GmQuestionPosted n, CancellationToken ct)
        {
            using var ctx = await _db.CreateDbContextAsync(ct);
            var thread = await (
                from t in ctx.BaronQaThreads.AsNoTracking()
                join b in ctx.Baronies.AsNoTracking() on t.BaronyId equals b.Id
                where t.Id == n.ThreadId
                select new { t.Title, t.BaronyId, b.CharacterId }
            ).FirstOrDefaultAsync(ct);

            if (thread is null)
                return null;

            var url = BaronyUrl($"/barony/notes?tab=qa&thread={n.ThreadId}", thread.CharacterId);
            var tag = $"gm-question-{n.ThreadId}-{n.MessageId}";
            var threadKey = $"gm-question-{n.ThreadId}";

            if (n.FromGameMaster)
            {
                // The GM answered — only the baron who owns the thread is waiting for it.
                var baronId = await _recipients.UserIdForBarony(thread.BaronyId, ct);
                if (baronId is null)
                    return null;

                return new Plan(
                    new List<string> { baronId },
                    new PushNotificationDTO
                    {
                        Title = Loc.T("The Game Master answered"),
                        Body = thread.Title,
                        Url = url,
                        Tag = tag,
                        ThreadKey = threadKey,
                    });
            }

            var gmIds = await _recipients.GameMasterUserIds(ct);
            if (gmIds.Count == 0)
                return null;

            return new Plan(
                gmIds,
                new PushNotificationDTO
                {
                    Title = Loc.T("New question for the Game Master"),
                    Body = thread.Title,
                    Url = url,
                    Tag = tag,
                    ThreadKey = threadKey,
                });
        }

        private async Task<Plan?> PlanBattleTurn(BattleTurnAdvanced n, CancellationToken ct)
        {
            string? baronyName;
            int characterId;
            using (var ctx = await _db.CreateDbContextAsync(ct))
            {
                var barony = await ctx.Baronies
                    .AsNoTracking()
                    .Where(b => b.Id == n.BaronyId)
                    .Select(b => new { b.Name, b.CharacterId })
                    .FirstOrDefaultAsync(ct);

                baronyName = barony?.Name;
                characterId = barony?.CharacterId ?? 0;
            }

            if (baronyName is null)
                return null;

            // Enemy units are moved by the Game Master, the baron's own units by the player.
            List<string> userIds;
            if (n.ActiveUnitIsEnemy)
            {
                userIds = await _recipients.GameMasterUserIds(ct);
            }
            else
            {
                var baronId = await _recipients.UserIdForBarony(n.BaronyId, ct);
                userIds = baronId is null ? new List<string>() : new List<string> { baronId };
            }

            if (userIds.Count == 0)
                return null;

            var body = string.IsNullOrWhiteSpace(n.ActiveUnitLabel)
                ? Loc.T("{0} — round {1}.", baronyName, n.Round)
                : Loc.T("{0} — round {1}.", n.ActiveUnitLabel, n.Round);

            return new Plan(
                userIds,
                new PushNotificationDTO
                {
                    Title = n.SubPhase == BaronyBattleSubPhases.AttackPlanning
                        ? Loc.T("Assign attack orders")
                        : Loc.T("Your move in the battle"),
                    Body = body,
                    // One notification per battle — a newer one replaces the previous.
                    Url = BaronyUrl("/barony/battle-map", characterId),
                    Tag = $"battle-{n.BaronyId}",
                });
        }

        private async Task<Plan?> PlanChatMessage(ChatMessagePosted n, CancellationToken ct)
        {
            if (n.ConversationId <= 0 || string.IsNullOrWhiteSpace(n.SenderUserId))
                return null;

            using var ctx = await _db.CreateDbContextAsync(ct);

            var conversation = await ctx.ChatConversations
                .AsNoTracking()
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == n.ConversationId, ct);

            if (conversation is null)
                return null;

            // Same resolution the in-app broadcaster uses, so a message cannot reach one channel and
            // miss the other.
            var recipients = await ChatAccess.RecipientsAsync(ctx, conversation, n.SenderUserId, ct);
            if (recipients.Count == 0)
                return null;

            var sentAsGameMaster = await ctx.ChatMessages
                .AsNoTracking()
                .Where(m => m.Id == n.MessageId)
                .Select(m => m.SentAsGameMaster)
                .FirstOrDefaultAsync(ct);

            string senderName;
            if (sentAsGameMaster)
            {
                senderName = Loc.T("Game Master");
            }
            else
            {
                var accounts = await ChatAccess.AccountsAsync(ctx, new[] { n.SenderUserId }, ct);
                senderName = accounts.GetValueOrDefault(n.SenderUserId)?.DisplayName ?? string.Empty;
            }

            string title;
            string body;
            if (conversation.Kind == ChatConversationKind.CampaignParty)
            {
                var campaignName = await ctx.Campaigns
                    .AsNoTracking()
                    .Where(c => c.Id == conversation.CampaignId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync(ct);

                title = string.IsNullOrWhiteSpace(campaignName)
                    ? Loc.T("New message in the party chat")
                    : Loc.T("New message in {0}", campaignName);
                body = string.IsNullOrWhiteSpace(senderName)
                    ? Loc.T("Someone wrote to the party.")
                    : Loc.T("{0} wrote to the party.", senderName);
            }
            else
            {
                title = Loc.T("New private message");
                body = string.IsNullOrWhiteSpace(senderName)
                    ? Loc.T("You have a new message.")
                    : Loc.T("{0} wrote to you.", senderName);
            }

            return new Plan(
                recipients,
                new PushNotificationDTO
                {
                    Title = title,
                    Body = body,
                    Url = $"/?chat={conversation.Id}",
                    // A tag per message, because every message matters and iOS replaces a reused tag
                    // without alerting. ThreadKey lets the worker clear the thread's earlier entries.
                    Tag = $"chat-{conversation.Id}-{n.MessageId}",
                    ThreadKey = $"chat-{conversation.Id}",
                });
        }
    }
}
