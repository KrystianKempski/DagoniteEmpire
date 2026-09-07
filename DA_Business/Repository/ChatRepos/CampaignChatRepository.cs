using DA_Business.Repository.ChatRepos.IRepository;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_Common.Localization;
using DA_Common.Notifications;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.ChatModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.ChatRepos
{
    public class CampaignChatRepository : ICampaignChatRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IGameNotificationQueue _notifications;
        private readonly ICampaignChatBroadcaster _broadcaster;

        public CampaignChatRepository(
            IDbContextFactory<ApplicationDbContext> db,
            IGameNotificationQueue notifications,
            ICampaignChatBroadcaster broadcaster)
        {
            _db = db;
            _notifications = notifications;
            _broadcaster = broadcaster;
        }

        public async Task<IReadOnlyList<CampaignChatContactDTO>> GetContactsAsync(
            int campaignId,
            int myCharacterId,
            string userName,
            bool isAdminOrMg)
        {
            using var ctx = await _db.CreateDbContextAsync();
            await EnsureCanAccessCampaign(ctx, campaignId, myCharacterId, userName, isAdminOrMg);

            var roster = await ctx.Campaigns
                .AsNoTracking()
                .Where(c => c.Id == campaignId)
                .SelectMany(c => c.Characters)
                .Select(c => new { c.Id, c.NPCName, c.ImageUrl, c.UserName })
                .ToListAsync();

            var gm = await ctx.Characters
                .AsNoTracking()
                .Where(c => c.NPCName == SD.GameMaster_NPCName)
                .Select(c => new { c.Id, c.NPCName, c.ImageUrl })
                .FirstOrDefaultAsync();

            var messages = await ctx.CampaignChatMessages
                .AsNoTracking()
                .Where(m => m.CampaignId == campaignId && !m.IsDeleted)
                .Where(m =>
                    m.RecipientCharacterId == null
                    || m.SenderCharacterId == myCharacterId
                    || m.RecipientCharacterId == myCharacterId)
                .Select(m => new
                {
                    m.Id,
                    m.SenderCharacterId,
                    m.RecipientCharacterId,
                    m.Content,
                    m.CreatedDate,
                })
                .ToListAsync();

            var reads = await ctx.CampaignChatReads
                .AsNoTracking()
                .Where(r => r.CampaignId == campaignId && r.CharacterId == myCharacterId)
                .ToListAsync();

            DateTime LastRead(int? peerId)
            {
                var row = reads.FirstOrDefault(r =>
                    peerId is null ? r.PeerCharacterId is null : r.PeerCharacterId == peerId);
                return row?.LastReadDate ?? DateTime.MinValue;
            }

            int UnreadFor(int? peerId, IEnumerable<(int SenderId, int? RecipientId, DateTime Created)> relevant)
            {
                var since = LastRead(peerId);
                return relevant.Count(m => m.Created > since && m.SenderId != myCharacterId);
            }

            var contacts = new List<CampaignChatContactDTO>();

            // Party channel — pinned.
            var partyMsgs = messages
                .Where(m => m.RecipientCharacterId is null)
                .OrderByDescending(m => m.CreatedDate)
                .ToList();
            var lastParty = partyMsgs.FirstOrDefault();
            contacts.Add(new CampaignChatContactDTO
            {
                PeerCharacterId = null,
                DisplayName = Loc.T("Whole party"),
                ImageUrl = null,
                IsPartyChannel = true,
                IsGameMaster = false,
                UnreadCount = UnreadFor(null, partyMsgs.Select(m => (m.SenderCharacterId, m.RecipientCharacterId, m.CreatedDate))),
                LastMessagePreview = Truncate(lastParty?.Content),
                LastMessageDate = lastParty?.CreatedDate,
            });

            foreach (var peer in roster.Where(c => c.Id != myCharacterId).OrderBy(c => c.NPCName))
            {
                var thread = messages
                    .Where(m =>
                        m.RecipientCharacterId != null
                        && ((m.SenderCharacterId == myCharacterId && m.RecipientCharacterId == peer.Id)
                            || (m.SenderCharacterId == peer.Id && m.RecipientCharacterId == myCharacterId)))
                    .OrderByDescending(m => m.CreatedDate)
                    .ToList();
                var last = thread.FirstOrDefault();
                contacts.Add(new CampaignChatContactDTO
                {
                    PeerCharacterId = peer.Id,
                    DisplayName = peer.NPCName ?? peer.UserName,
                    ImageUrl = peer.ImageUrl,
                    IsPartyChannel = false,
                    IsGameMaster = false,
                    UnreadCount = UnreadFor(peer.Id, thread.Select(m => (m.SenderCharacterId, m.RecipientCharacterId, m.CreatedDate))),
                    LastMessagePreview = Truncate(last?.Content),
                    LastMessageDate = last?.CreatedDate,
                });
            }

            if (gm is not null && gm.Id != myCharacterId && roster.All(c => c.Id != gm.Id))
            {
                var thread = messages
                    .Where(m =>
                        m.RecipientCharacterId != null
                        && ((m.SenderCharacterId == myCharacterId && m.RecipientCharacterId == gm.Id)
                            || (m.SenderCharacterId == gm.Id && m.RecipientCharacterId == myCharacterId)))
                    .OrderByDescending(m => m.CreatedDate)
                    .ToList();
                var last = thread.FirstOrDefault();
                contacts.Add(new CampaignChatContactDTO
                {
                    PeerCharacterId = gm.Id,
                    DisplayName = Loc.T("Game Master"),
                    ImageUrl = string.IsNullOrWhiteSpace(gm.ImageUrl) ? SD.GameMaster_Portrait : gm.ImageUrl,
                    IsPartyChannel = false,
                    IsGameMaster = true,
                    UnreadCount = UnreadFor(gm.Id, thread.Select(m => (m.SenderCharacterId, m.RecipientCharacterId, m.CreatedDate))),
                    LastMessagePreview = Truncate(last?.Content),
                    LastMessageDate = last?.CreatedDate,
                });
            }
            else if (gm is not null && roster.Any(c => c.Id == gm.Id))
            {
                var gmContact = contacts.FirstOrDefault(c => c.PeerCharacterId == gm.Id);
                if (gmContact is not null)
                {
                    gmContact.IsGameMaster = true;
                    gmContact.DisplayName = Loc.T("Game Master");
                    if (string.IsNullOrWhiteSpace(gmContact.ImageUrl))
                        gmContact.ImageUrl = SD.GameMaster_Portrait;
                }
            }

            return contacts;
        }

        public async Task<IReadOnlyList<CampaignChatMessageDTO>> GetThreadAsync(
            int campaignId,
            int myCharacterId,
            int? peerCharacterId,
            string userName,
            bool isAdminOrMg,
            int take = 50,
            long? beforeId = null)
        {
            using var ctx = await _db.CreateDbContextAsync();
            await EnsureCanAccessCampaign(ctx, campaignId, myCharacterId, userName, isAdminOrMg);

            var query = ctx.CampaignChatMessages
                .AsNoTracking()
                .Include(m => m.SenderCharacter)
                .Where(m => m.CampaignId == campaignId && !m.IsDeleted);

            if (peerCharacterId is null)
            {
                query = query.Where(m => m.RecipientCharacterId == null);
            }
            else
            {
                var peerId = peerCharacterId.Value;
                query = query.Where(m =>
                    m.RecipientCharacterId != null
                    && ((m.SenderCharacterId == myCharacterId && m.RecipientCharacterId == peerId)
                        || (m.SenderCharacterId == peerId && m.RecipientCharacterId == myCharacterId)));
            }

            if (beforeId is not null)
                query = query.Where(m => m.Id < beforeId.Value);

            var rows = await query
                .OrderByDescending(m => m.Id)
                .Take(Math.Clamp(take, 1, 200))
                .ToListAsync();

            rows.Reverse();
            return rows.Select(m => ToDto(m, myCharacterId)).ToList();
        }

        public async Task<CampaignChatMessageDTO> SendAsync(
            int campaignId,
            int senderCharacterId,
            int? recipientCharacterId,
            string content,
            string userName,
            bool isAdminOrMg)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new RepositoryErrorException(Loc.T("Message cannot be empty."));

            content = content.Trim();
            if (content.Length > 4000)
                content = content[..4000];

            using var ctx = await _db.CreateDbContextAsync();
            await EnsureCanAccessCampaign(ctx, campaignId, senderCharacterId, userName, isAdminOrMg);

            if (recipientCharacterId is not null)
            {
                var recipientOk = await IsValidRecipient(ctx, campaignId, recipientCharacterId.Value, isAdminOrMg);
                if (!recipientOk)
                    throw new RepositoryErrorException(Loc.T("Recipient is not part of this campaign."));
            }

            var entity = new CampaignChatMessage
            {
                CampaignId = campaignId,
                SenderCharacterId = senderCharacterId,
                RecipientCharacterId = recipientCharacterId,
                Content = content,
                CreatedDate = DateTime.Now,
                IsDeleted = false,
            };

            await ctx.CampaignChatMessages.AddAsync(entity);
            await ctx.SaveChangesAsync();

            await ctx.Entry(entity).Reference(e => e.SenderCharacter).LoadAsync();

            var dto = ToDto(entity, senderCharacterId);
            _broadcaster.Publish(dto);
            _notifications.Enqueue(new CampaignChatMessageSent(campaignId, senderCharacterId, recipientCharacterId));
            return dto;
        }

        public async Task MarkReadAsync(
            int campaignId,
            int myCharacterId,
            int? peerCharacterId,
            string userName,
            bool isAdminOrMg)
        {
            using var ctx = await _db.CreateDbContextAsync();
            await EnsureCanAccessCampaign(ctx, campaignId, myCharacterId, userName, isAdminOrMg);

            var existing = await ctx.CampaignChatReads
                .FirstOrDefaultAsync(r =>
                    r.CampaignId == campaignId
                    && r.CharacterId == myCharacterId
                    && (peerCharacterId == null
                        ? r.PeerCharacterId == null
                        : r.PeerCharacterId == peerCharacterId));

            var now = DateTime.Now;
            if (existing is null)
            {
                await ctx.CampaignChatReads.AddAsync(new CampaignChatRead
                {
                    CampaignId = campaignId,
                    CharacterId = myCharacterId,
                    PeerCharacterId = peerCharacterId,
                    LastReadDate = now,
                });
            }
            else
            {
                existing.LastReadDate = now;
            }

            await ctx.SaveChangesAsync();
        }

        public async Task<int> GetUnreadSummaryAsync(
            int myCharacterId,
            string userName,
            bool isAdminOrMg)
        {
            using var ctx = await _db.CreateDbContextAsync();

            var character = await ctx.Characters.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == myCharacterId);
            if (character is null)
                return 0;

            EnsureCanActAsCharacter(character, userName, isAdminOrMg);

            List<int> campaignIds;
            if (isAdminOrMg && character.NPCName == SD.GameMaster_NPCName)
            {
                campaignIds = await ctx.Campaigns
                    .AsNoTracking()
                    .Where(c => !c.IsFinished)
                    .Select(c => c.Id)
                    .ToListAsync();
            }
            else
            {
                campaignIds = await ctx.Campaigns
                    .AsNoTracking()
                    .Where(c => c.Characters.Any(ch => ch.Id == myCharacterId))
                    .Select(c => c.Id)
                    .ToListAsync();
            }

            if (campaignIds.Count == 0)
                return 0;

            var messages = await ctx.CampaignChatMessages
                .AsNoTracking()
                .Where(m => campaignIds.Contains(m.CampaignId) && !m.IsDeleted && m.SenderCharacterId != myCharacterId)
                .Where(m =>
                    m.RecipientCharacterId == null
                    || m.RecipientCharacterId == myCharacterId)
                .Select(m => new { m.CampaignId, m.SenderCharacterId, m.RecipientCharacterId, m.CreatedDate })
                .ToListAsync();

            var reads = await ctx.CampaignChatReads
                .AsNoTracking()
                .Where(r => r.CharacterId == myCharacterId && campaignIds.Contains(r.CampaignId))
                .ToListAsync();

            int unread = 0;
            foreach (var m in messages)
            {
                int? peer = m.RecipientCharacterId is null ? null : m.SenderCharacterId;
                var lastRead = reads.FirstOrDefault(r =>
                    r.CampaignId == m.CampaignId
                    && (peer is null ? r.PeerCharacterId is null : r.PeerCharacterId == peer));
                var since = lastRead?.LastReadDate ?? DateTime.MinValue;
                if (m.CreatedDate > since)
                    unread++;
            }

            return unread;
        }

        private static async Task EnsureCanAccessCampaign(
            ApplicationDbContext ctx,
            int campaignId,
            int characterId,
            string userName,
            bool isAdminOrMg)
        {
            var character = await ctx.Characters.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == characterId)
                ?? throw new RepositoryErrorException(Loc.T("Character not found."));

            EnsureCanActAsCharacter(character, userName, isAdminOrMg);

            var isGmPersona = character.NPCName == SD.GameMaster_NPCName;
            if (isAdminOrMg && isGmPersona)
            {
                var campaignExists = await ctx.Campaigns.AsNoTracking().AnyAsync(c => c.Id == campaignId);
                if (!campaignExists)
                    throw new RepositoryErrorException(Loc.T("Campaign not found."));
                return;
            }

            var inCampaign = await ctx.Campaigns
                .AsNoTracking()
                .Where(c => c.Id == campaignId)
                .SelectMany(c => c.Characters)
                .AnyAsync(c => c.Id == characterId);

            if (!inCampaign)
                throw new RepositoryErrorException(Loc.T("Character is not part of this campaign."));
        }

        private static void EnsureCanActAsCharacter(Character character, string userName, bool isAdminOrMg)
        {
            if (isAdminOrMg)
                return;

            if (!string.Equals(character.UserName, userName, StringComparison.OrdinalIgnoreCase))
                throw new RepositoryErrorException(Loc.T("You cannot chat as this character."));
        }

        private static async Task<bool> IsValidRecipient(
            ApplicationDbContext ctx,
            int campaignId,
            int recipientCharacterId,
            bool isAdminOrMg)
        {
            var isGm = await ctx.Characters.AsNoTracking()
                .AnyAsync(c => c.Id == recipientCharacterId && c.NPCName == SD.GameMaster_NPCName);
            if (isGm)
                return true;

            return await ctx.Campaigns
                .AsNoTracking()
                .Where(c => c.Id == campaignId)
                .SelectMany(c => c.Characters)
                .AnyAsync(c => c.Id == recipientCharacterId);
        }

        private static CampaignChatMessageDTO ToDto(CampaignChatMessage m, int myCharacterId)
        {
            var name = m.SenderCharacter?.NPCName;
            if (string.IsNullOrWhiteSpace(name))
                name = m.SenderCharacter?.UserName ?? string.Empty;
            if (m.SenderCharacter?.NPCName == SD.GameMaster_NPCName)
                name = Loc.T("Game Master");

            var image = m.SenderCharacter?.ImageUrl;
            if (m.SenderCharacter?.NPCName == SD.GameMaster_NPCName && string.IsNullOrWhiteSpace(image))
                image = SD.GameMaster_Portrait;

            return new CampaignChatMessageDTO
            {
                Id = m.Id,
                CampaignId = m.CampaignId,
                SenderCharacterId = m.SenderCharacterId,
                SenderName = name ?? string.Empty,
                SenderImageUrl = image,
                RecipientCharacterId = m.RecipientCharacterId,
                Content = m.Content,
                CreatedDate = m.CreatedDate,
                IsMine = m.SenderCharacterId == myCharacterId,
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
