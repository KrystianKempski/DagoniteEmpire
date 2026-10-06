using DA_Business.Repository.ChatRepos.IRepository;
using DA_Business.Services;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_Common.Localization;
using DA_Common.Notifications;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.ChatModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.ChatRepos
{
    /// <inheritdoc cref="IChatRepository"/>
    public class ChatRepository : IChatRepository
    {
        private const int MaxMessageLength = 4000;

        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IGameNotificationQueue _notifications;
        private readonly IChatBroadcaster _broadcaster;

        public ChatRepository(
            IDbContextFactory<ApplicationDbContext> db,
            IGameNotificationQueue notifications,
            IChatBroadcaster broadcaster)
        {
            _db = db;
            _notifications = notifications;
            _broadcaster = broadcaster;
        }

        public async Task<IReadOnlyList<ChatContactDTO>> GetContactsAsync(string userId)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var me = await ChatAccess.ResolveIdentityAsync(ctx, userId);
            if (me is null || !me.HasAnyThread)
                return Array.Empty<ChatContactDTO>();

            var conversations = await MyConversationsAsync(ctx, me);
            var convIds = conversations.Select(c => c.Id).ToList();

            var unread = await UnreadPerConversationAsync(ctx, me.UserId, convIds);
            var last = await LastMessagePerConversationAsync(ctx, convIds);

            var campaignNames = await ctx.Campaigns
                .AsNoTracking()
                .Where(c => me.CampaignIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            var contacts = new List<ChatContactDTO>();

            // Party channels first — they are the campaign rooms and should not drift down the list.
            foreach (var campaign in campaignNames.OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                var conversation = conversations.FirstOrDefault(c =>
                    c.Kind == ChatConversationKind.CampaignParty && c.CampaignId == campaign.Id);

                contacts.Add(Describe(
                    conversation,
                    unread,
                    last,
                    isParty: true,
                    campaignId: campaign.Id,
                    peerUserId: null,
                    peerIsGameMaster: false,
                    displayName: campaign.Name,
                    imageUrl: null));
            }

            var directContacts = new List<ChatContactDTO>();
            var matchedConversationIds = new HashSet<long>();

            if (!me.IsGameMaster)
            {
                var gmThread = conversations.FirstOrDefault(c =>
                    c.Kind == ChatConversationKind.Direct && c.Participants.Any(p => p.IsGameMasterRole));
                if (gmThread is not null)
                    matchedConversationIds.Add(gmThread.Id);

                directContacts.Add(Describe(
                    gmThread,
                    unread,
                    last,
                    isParty: false,
                    campaignId: null,
                    peerUserId: null,
                    peerIsGameMaster: true,
                    displayName: Loc.T("Game Master"),
                    imageUrl: SD.GameMaster_Portrait));
            }

            foreach (var peer in await ChatAccess.ReachablePeersAsync(ctx, me))
            {
                var conversation = conversations.FirstOrDefault(c =>
                    c.Kind == ChatConversationKind.Direct
                    && c.Participants.Any(p => p.UserId == peer.UserId));
                if (conversation is not null)
                    matchedConversationIds.Add(conversation.Id);

                directContacts.Add(Describe(
                    conversation,
                    unread,
                    last,
                    isParty: false,
                    campaignId: null,
                    peerUserId: peer.UserId,
                    peerIsGameMaster: false,
                    displayName: peer.DisplayName,
                    imageUrl: peer.ImageUrl));
            }

            // A peer who left every shared campaign is no longer reachable, but an existing thread must
            // not silently disappear along with its history.
            var orphans = conversations
                .Where(c => c.Kind == ChatConversationKind.Direct && !matchedConversationIds.Contains(c.Id))
                .ToList();

            if (orphans.Count > 0)
            {
                var otherSideIds = orphans
                    .SelectMany(c => c.Participants)
                    .Where(p => !p.IsGameMasterRole
                        && !string.IsNullOrWhiteSpace(p.UserId)
                        && p.UserId != me.UserId)
                    .Select(p => p.UserId!)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                var accounts = await ChatAccess.AccountsAsync(ctx, otherSideIds);

                foreach (var conversation in orphans)
                {
                    var other = conversation.Participants
                        .FirstOrDefault(p => !p.IsGameMasterRole
                            && !string.IsNullOrWhiteSpace(p.UserId)
                            && p.UserId != me.UserId);

                    if (other?.UserId is null || !accounts.TryGetValue(other.UserId, out var account))
                        continue;

                    directContacts.Add(Describe(
                        conversation,
                        unread,
                        last,
                        isParty: false,
                        campaignId: null,
                        peerUserId: account.UserId,
                        peerIsGameMaster: false,
                        displayName: account.DisplayName,
                        imageUrl: account.ImageUrl));
                }
            }

            contacts.AddRange(directContacts
                .OrderByDescending(c => c.LastMessageUtc ?? DateTime.MinValue)
                .ThenBy(c => c.DisplayName, StringComparer.CurrentCultureIgnoreCase));

            return contacts;
        }

        public async Task<IReadOnlyList<ChatMessageDTO>> GetThreadAsync(
            long conversationId,
            string userId,
            int take = 50,
            long? beforeId = null)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var me = await ChatAccess.ResolveIdentityAsync(ctx, userId);
            if (me is null)
                return Array.Empty<ChatMessageDTO>();

            await LoadAccessibleConversationAsync(ctx, conversationId, me);

            var query = ctx.ChatMessages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted);

            if (beforeId is not null)
                query = query.Where(m => m.Id < beforeId.Value);

            var rows = await query
                .OrderByDescending(m => m.Id)
                .Take(Math.Clamp(take, 1, 200))
                .ToListAsync();

            rows.Reverse();

            var authors = await ChatAccess.AccountsAsync(ctx, rows.Select(r => r.SenderUserId));
            return rows.Select(m => ToDto(m, me.UserId, authors)).ToList();
        }

        public async Task<ChatConversationInfoDTO?> GetConversationInfoAsync(long conversationId, string userId)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var me = await ChatAccess.ResolveIdentityAsync(ctx, userId);
            if (me is null)
                return null;

            var conversation = await LoadAccessibleConversationAsync(ctx, conversationId, me);

            if (conversation.Kind == ChatConversationKind.CampaignParty)
            {
                var name = await ctx.Campaigns
                    .AsNoTracking()
                    .Where(c => c.Id == conversation.CampaignId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();

                return new ChatConversationInfoDTO
                {
                    ConversationId = conversation.Id,
                    IsPartyChannel = true,
                    DisplayName = name ?? Loc.T("Chat"),
                };
            }

            var otherSide = conversation.Participants
                .FirstOrDefault(p => p.IsGameMasterRole || p.UserId != me.UserId);

            if (otherSide is null || otherSide.IsGameMasterRole)
            {
                return new ChatConversationInfoDTO
                {
                    ConversationId = conversation.Id,
                    DisplayName = Loc.T("Game Master"),
                    ImageUrl = SD.GameMaster_Portrait,
                };
            }

            var accounts = await ChatAccess.AccountsAsync(ctx, new[] { otherSide.UserId! });
            var account = accounts.GetValueOrDefault(otherSide.UserId!);

            return new ChatConversationInfoDTO
            {
                ConversationId = conversation.Id,
                DisplayName = account?.DisplayName ?? Loc.T("Chat"),
                ImageUrl = account?.ImageUrl,
            };
        }

        public async Task<long> GetOrCreateDirectAsync(string userId, string? peerUserId, bool peerIsGameMaster)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var me = await ChatAccess.ResolveIdentityAsync(ctx, userId)
                ?? throw new RepositoryErrorException(Loc.T("You cannot use chat."));

            string? playerSideId;

            if (me.IsGameMaster)
            {
                // A GM always writes from the role side, so the thread is keyed by the player — otherwise
                // handing the campaign to another GM would start a second thread with the same player.
                if (string.IsNullOrWhiteSpace(peerUserId) || peerIsGameMaster)
                    throw new RepositoryErrorException(Loc.T("Pick a player to write to."));

                playerSideId = peerUserId;
            }
            else if (peerIsGameMaster)
            {
                playerSideId = me.UserId;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(peerUserId))
                    throw new RepositoryErrorException(Loc.T("Pick a player to write to."));

                var reachable = await ChatAccess.ReachablePeersAsync(ctx, me);
                if (reachable.All(p => p.UserId != peerUserId))
                    throw new RepositoryErrorException(Loc.T("You share no campaign with this player."));

                return await GetOrCreateAsync(
                    ctx,
                    ChatPairKey.Direct(me.UserId, peerUserId),
                    () => NewDirect(me.UserId, peerUserId, withGameMaster: false));
            }

            return await GetOrCreateAsync(
                ctx,
                ChatPairKey.Direct(playerSideId, null),
                () => NewDirect(playerSideId, null, withGameMaster: true));
        }

        public async Task<long> GetOrCreatePartyAsync(string userId, int campaignId)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var me = await ChatAccess.ResolveIdentityAsync(ctx, userId)
                ?? throw new RepositoryErrorException(Loc.T("You cannot use chat."));

            if (!me.CampaignIds.Contains(campaignId))
                throw new RepositoryErrorException(Loc.T("You are not part of this campaign."));

            return await GetOrCreateAsync(
                ctx,
                ChatPairKey.Party(campaignId),
                () => new ChatConversation
                {
                    Kind = ChatConversationKind.CampaignParty,
                    CampaignId = campaignId,
                    CreatedUtc = DateTime.UtcNow,
                    LastMessageUtc = DateTime.UtcNow,
                });
        }

        public async Task<ChatMessageDTO> SendAsync(long conversationId, string senderUserId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new RepositoryErrorException(Loc.T("Message cannot be empty."));

            content = content.Trim();
            if (content.Length > MaxMessageLength)
                content = content[..MaxMessageLength];

            using var ctx = await _db.CreateDbContextAsync();
            var me = await ChatAccess.ResolveIdentityAsync(ctx, senderUserId)
                ?? throw new RepositoryErrorException(Loc.T("You cannot use chat."));

            var conversation = await LoadAccessibleConversationAsync(ctx, conversationId, me);

            var now = DateTime.UtcNow;
            var entity = new ChatMessage
            {
                ConversationId = conversation.Id,
                SenderUserId = me.UserId,
                // Game Masters always speak as the Game Master; that is the whole point of the role.
                SentAsGameMaster = me.IsGameMaster,
                Content = content,
                CreatedUtc = now,
                IsDeleted = false,
            };

            await ctx.ChatMessages.AddAsync(entity);

            var tracked = await ctx.ChatConversations.FirstAsync(c => c.Id == conversation.Id);
            tracked.LastMessageUtc = now;

            await ctx.SaveChangesAsync();

            var authors = await ChatAccess.AccountsAsync(ctx, new[] { me.UserId });
            var dto = ToDto(entity, me.UserId, authors);

            var recipients = await ChatAccess.RecipientsAsync(ctx, conversation, me.UserId);
            _broadcaster.Publish(dto, recipients);
            _notifications.Enqueue(new ChatMessagePosted(conversation.Id, entity.Id, me.UserId));

            return dto;
        }

        public async Task MarkReadAsync(long conversationId, string userId)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var me = await ChatAccess.ResolveIdentityAsync(ctx, userId);
            if (me is null)
                return;

            await LoadAccessibleConversationAsync(ctx, conversationId, me);

            var existing = await ctx.ChatReadStates
                .FirstOrDefaultAsync(r => r.ConversationId == conversationId && r.UserId == me.UserId);

            var now = DateTime.UtcNow;
            if (existing is null)
            {
                await ctx.ChatReadStates.AddAsync(new ChatReadState
                {
                    ConversationId = conversationId,
                    UserId = me.UserId,
                    LastReadUtc = now,
                });
            }
            else
            {
                existing.LastReadUtc = now;
            }

            try
            {
                await ctx.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Two circuits of the same account opened the thread at once; either watermark will do.
            }
        }

        public async Task<int> GetUnreadTotalAsync(string userId)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var me = await ChatAccess.ResolveIdentityAsync(ctx, userId);
            if (me is null)
                return 0;

            var conversations = await MyConversationsAsync(ctx, me);
            var convIds = conversations.Select(c => c.Id).ToList();
            if (convIds.Count == 0)
                return 0;

            return await UnreadQuery(ctx, me.UserId, convIds).CountAsync();
        }

        /// <summary>
        /// Threads this account takes part in: direct threads it is a participant of, every direct thread
        /// held by the Game Master role when it holds that role, and the party channel of each of its
        /// campaigns.
        /// </summary>
        private static async Task<List<ChatConversation>> MyConversationsAsync(
            ApplicationDbContext ctx,
            ChatIdentity me)
        {
            var directIds = await ctx.ChatParticipants
                .AsNoTracking()
                .Where(p => p.UserId == me.UserId)
                .Select(p => p.ConversationId)
                .ToListAsync();

            if (me.IsGameMaster)
            {
                var roleIds = await ctx.ChatParticipants
                    .AsNoTracking()
                    .Where(p => p.IsGameMasterRole)
                    .Select(p => p.ConversationId)
                    .ToListAsync();

                directIds = directIds.Concat(roleIds).Distinct().ToList();
            }

            var campaignIds = me.CampaignIds.ToList();

            return await ctx.ChatConversations
                .AsNoTracking()
                .Include(c => c.Participants)
                .Where(c => directIds.Contains(c.Id)
                    || (c.Kind == ChatConversationKind.CampaignParty
                        && c.CampaignId != null
                        && campaignIds.Contains(c.CampaignId.Value)))
                .ToListAsync();
        }

        /// <summary>
        /// Messages the account has not seen: newer than its watermark for that thread and not its own.
        /// Expressed as a single correlated query so neither the badge nor the list pulls message rows
        /// into memory just to count them.
        /// </summary>
        private static IQueryable<ChatMessage> UnreadQuery(
            ApplicationDbContext ctx,
            string userId,
            List<long> conversationIds) =>
            ctx.ChatMessages
                .AsNoTracking()
                .Where(m => conversationIds.Contains(m.ConversationId)
                    && !m.IsDeleted
                    && m.SenderUserId != userId
                    && !ctx.ChatReadStates.Any(r =>
                        r.ConversationId == m.ConversationId
                        && r.UserId == userId
                        && r.LastReadUtc >= m.CreatedUtc));

        private static async Task<Dictionary<long, int>> UnreadPerConversationAsync(
            ApplicationDbContext ctx,
            string userId,
            List<long> conversationIds)
        {
            if (conversationIds.Count == 0)
                return new Dictionary<long, int>();

            var rows = await UnreadQuery(ctx, userId, conversationIds)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Count = g.Count() })
                .ToListAsync();

            return rows.ToDictionary(r => r.ConversationId, r => r.Count);
        }

        private static async Task<Dictionary<long, (string Content, DateTime CreatedUtc)>> LastMessagePerConversationAsync(
            ApplicationDbContext ctx,
            List<long> conversationIds)
        {
            if (conversationIds.Count == 0)
                return new Dictionary<long, (string, DateTime)>();

            var lastIds = await ctx.ChatMessages
                .AsNoTracking()
                .Where(m => conversationIds.Contains(m.ConversationId) && !m.IsDeleted)
                .GroupBy(m => m.ConversationId)
                .Select(g => g.Max(m => m.Id))
                .ToListAsync();

            var rows = await ctx.ChatMessages
                .AsNoTracking()
                .Where(m => lastIds.Contains(m.Id))
                .Select(m => new { m.ConversationId, m.Content, m.CreatedUtc })
                .ToListAsync();

            return rows.ToDictionary(r => r.ConversationId, r => (r.Content, r.CreatedUtc));
        }

        /// <summary>
        /// Loads the thread and refuses it when the caller has no business reading it: party channels are
        /// open to the campaign roster and the Game Masters, direct threads to their two sides.
        /// </summary>
        private static async Task<ChatConversation> LoadAccessibleConversationAsync(
            ApplicationDbContext ctx,
            long conversationId,
            ChatIdentity me)
        {
            var conversation = await ctx.ChatConversations
                .AsNoTracking()
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == conversationId)
                ?? throw new RepositoryErrorException(Loc.T("Conversation not found."));

            var allowed = conversation.Kind switch
            {
                ChatConversationKind.CampaignParty =>
                    conversation.CampaignId is int id && me.CampaignIds.Contains(id),
                _ =>
                    conversation.Participants.Any(p => p.UserId == me.UserId)
                    || (me.IsGameMaster && conversation.Participants.Any(p => p.IsGameMasterRole)),
            };

            if (!allowed)
                throw new RepositoryErrorException(Loc.T("You cannot open this conversation."));

            return conversation;
        }

        /// <summary>
        /// Finds the thread by its pair key or creates it. The unique index on the key is what makes two
        /// racing callers safe: the loser's insert fails and it reads the winner's row.
        /// </summary>
        private static async Task<long> GetOrCreateAsync(
            ApplicationDbContext ctx,
            string pairKey,
            Func<ChatConversation> build)
        {
            var existing = await ctx.ChatConversations
                .AsNoTracking()
                .Where(c => c.PairKey == pairKey)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (existing != 0)
                return existing;

            var conversation = build();
            conversation.PairKey = pairKey;

            try
            {
                await ctx.ChatConversations.AddAsync(conversation);
                await ctx.SaveChangesAsync();
                return conversation.Id;
            }
            catch (DbUpdateException)
            {
                ctx.ChangeTracker.Clear();
                return await ctx.ChatConversations
                    .AsNoTracking()
                    .Where(c => c.PairKey == pairKey)
                    .Select(c => c.Id)
                    .FirstAsync();
            }
        }

        private static ChatConversation NewDirect(string? sideA, string? sideB, bool withGameMaster)
        {
            var now = DateTime.UtcNow;
            var conversation = new ChatConversation
            {
                Kind = ChatConversationKind.Direct,
                CreatedUtc = now,
                LastMessageUtc = now,
            };

            if (!string.IsNullOrWhiteSpace(sideA))
                conversation.Participants.Add(new ChatParticipant { UserId = sideA });

            if (!string.IsNullOrWhiteSpace(sideB))
                conversation.Participants.Add(new ChatParticipant { UserId = sideB });

            if (withGameMaster)
                conversation.Participants.Add(new ChatParticipant { IsGameMasterRole = true });

            return conversation;
        }

        private static ChatContactDTO Describe(
            ChatConversation? conversation,
            Dictionary<long, int> unread,
            Dictionary<long, (string Content, DateTime CreatedUtc)> last,
            bool isParty,
            int? campaignId,
            string? peerUserId,
            bool peerIsGameMaster,
            string displayName,
            string? imageUrl)
        {
            var contact = new ChatContactDTO
            {
                ConversationId = conversation?.Id,
                IsPartyChannel = isParty,
                CampaignId = campaignId,
                PeerUserId = peerUserId,
                PeerIsGameMaster = peerIsGameMaster,
                DisplayName = displayName,
                ImageUrl = imageUrl,
            };

            if (conversation is null)
                return contact;

            contact.UnreadCount = unread.GetValueOrDefault(conversation.Id);

            if (last.TryGetValue(conversation.Id, out var lastMessage))
            {
                contact.LastMessagePreview = Truncate(lastMessage.Content);
                contact.LastMessageUtc = lastMessage.CreatedUtc;
            }

            return contact;
        }

        private static ChatMessageDTO ToDto(
            ChatMessage message,
            string readerUserId,
            Dictionary<string, ChatPeer> authors)
        {
            var author = authors.GetValueOrDefault(message.SenderUserId);

            var name = message.SentAsGameMaster
                ? Loc.T("Game Master")
                : author?.DisplayName ?? string.Empty;

            var image = message.SentAsGameMaster
                ? SD.GameMaster_Portrait
                : author?.ImageUrl;

            return new ChatMessageDTO
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderUserId = message.SenderUserId,
                SenderName = name,
                SenderImageUrl = image,
                SentAsGameMaster = message.SentAsGameMaster,
                Content = message.Content,
                CreatedUtc = message.CreatedUtc,
                IsMine = string.Equals(message.SenderUserId, readerUserId, StringComparison.Ordinal),
            };
        }

        private static string? Truncate(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            content = content.Trim();
            return content.Length <= 80 ? content : content[..77] + "...";
        }
    }
}
