using System.Globalization;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_Common.Localization;
using DA_Common.Notifications;
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
                ChapterPostAdded n => await PlanChapterPost(n, cancellationToken),
                BaronyTurnResolved n => await PlanTurnResolved(n, cancellationToken),
                CampaignChatMessageSent n => await PlanChatMessage(n, cancellationToken),
                GmQuestionPosted n => await PlanGmQuestion(n, cancellationToken),
                BattleTurnAdvanced n => await PlanBattleTurn(n, cancellationToken),
                _ => null,
            };

            if (plan is null || plan.UserIds.Count == 0)
                return 0;

            return await _push.SendToUsers(plan.UserIds, plan.Payload, notification.Topic);
        }

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
                select new { t.Title, t.CorrespondentName, BaronyName = b.Name, t.BaronyId }
            ).FirstOrDefaultAsync(ct);

            if (thread is null)
                return null;

            var url = $"/barony/letters?thread={n.ThreadId}";
            var tag = $"baron-letter-{n.ThreadId}";
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
            using (var ctx = await _db.CreateDbContextAsync(ct))
            {
                baronyName = await ctx.Baronies
                    .AsNoTracking()
                    .Where(b => b.Id == n.BaronyId)
                    .Select(b => b.Name)
                    .FirstOrDefaultAsync(ct);
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
                    Url = "/barony",
                    Tag = $"turn-{n.BaronyId}",
                });
        }

        private async Task<Plan?> PlanGmQuestion(GmQuestionPosted n, CancellationToken ct)
        {
            using var ctx = await _db.CreateDbContextAsync(ct);
            var thread = await ctx.BaronQaThreads
                .AsNoTracking()
                .Where(t => t.Id == n.ThreadId)
                .Select(t => new { t.Title, t.BaronyId })
                .FirstOrDefaultAsync(ct);

            if (thread is null)
                return null;

            var url = $"/barony/notes?tab=qa&thread={n.ThreadId}";
            var tag = $"gm-question-{n.ThreadId}";

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
                });
        }

        private async Task<Plan?> PlanBattleTurn(BattleTurnAdvanced n, CancellationToken ct)
        {
            string? baronyName;
            using (var ctx = await _db.CreateDbContextAsync(ct))
            {
                baronyName = await ctx.Baronies
                    .AsNoTracking()
                    .Where(b => b.Id == n.BaronyId)
                    .Select(b => b.Name)
                    .FirstOrDefaultAsync(ct);
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
                    Url = "/barony/battle-map",
                    Tag = $"battle-{n.BaronyId}",
                });
        }

        private async Task<Plan?> PlanChatMessage(CampaignChatMessageSent n, CancellationToken ct)
        {
            if (n.CampaignId <= 0 || n.SenderCharacterId <= 0)
                return null;

            string? senderName;
            string? senderUserName;
            List<string?> rosterUserNames;
            using (var ctx = await _db.CreateDbContextAsync(ct))
            {
                var sender = await ctx.Characters
                    .AsNoTracking()
                    .Where(c => c.Id == n.SenderCharacterId)
                    .Select(c => new { c.NPCName, c.UserName })
                    .FirstOrDefaultAsync(ct);

                senderName = sender?.NPCName;
                senderUserName = sender?.UserName;
                if (string.Equals(senderName, SD.GameMaster_NPCName, StringComparison.Ordinal))
                    senderName = Loc.T("Game Master");

                if (n.RecipientCharacterId is int peerId)
                {
                    var peerOwner = await ctx.Characters
                        .AsNoTracking()
                        .Where(c => c.Id == peerId)
                        .Select(c => new { c.UserName, c.NPCName })
                        .FirstOrDefaultAsync(ct);

                    if (peerOwner is null)
                        return null;

                    List<string> userIds;
                    if (string.Equals(peerOwner.NPCName, SD.GameMaster_NPCName, StringComparison.Ordinal))
                    {
                        userIds = await _recipients.GameMasterUserIds(ct);
                    }
                    else
                    {
                        userIds = await _recipients.UserIdsForUserNames(new[] { peerOwner.UserName }, ct);
                    }

                    // Exclude the sender's own account when they somehow match.
                    var senderIds = await _recipients.UserIdsForUserNames(new[] { senderUserName }, ct);
                    userIds = userIds.Where(id => !senderIds.Contains(id)).ToList();
                    if (userIds.Count == 0)
                        return null;

                    return new Plan(
                        userIds,
                        new PushNotificationDTO
                        {
                            Title = Loc.T("New private message"),
                            Body = string.IsNullOrWhiteSpace(senderName)
                                ? Loc.T("You have a new message.")
                                : Loc.T("{0} wrote to you.", senderName),
                            Url = $"/?chat={n.CampaignId}:{n.SenderCharacterId}",
                            Tag = $"chat-{n.CampaignId}-{n.SenderCharacterId}",
                        });
                }

                rosterUserNames = await ctx.Campaigns
                    .AsNoTracking()
                    .Where(c => c.Id == n.CampaignId)
                    .SelectMany(c => c.Characters)
                    .Select(c => c.UserName)
                    .ToListAsync(ct);
            }

            var partyIds = await _recipients.UserIdsForUserNames(rosterUserNames, ct);
            var gmIds = await _recipients.GameMasterUserIds(ct);
            var senderAccountIds = await _recipients.UserIdsForUserNames(new[] { senderUserName }, ct);

            var recipients = partyIds
                .Concat(gmIds)
                .Distinct(StringComparer.Ordinal)
                .Where(id => !senderAccountIds.Contains(id))
                .ToList();

            if (recipients.Count == 0)
                return null;

            return new Plan(
                recipients,
                new PushNotificationDTO
                {
                    Title = Loc.T("New private message"),
                    Body = string.IsNullOrWhiteSpace(senderName)
                        ? Loc.T("You have a new message.")
                        : Loc.T("{0} wrote to you.", senderName),
                    Url = $"/?chat={n.CampaignId}:",
                    Tag = $"chat-{n.CampaignId}-party",
                });
        }
    }
}
