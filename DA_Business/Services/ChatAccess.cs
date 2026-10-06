using DA_Common;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Services
{
    /// <summary>Who this account is, as far as chat is concerned.</summary>
    /// <param name="CampaignIds">Non-finished campaigns this account reaches; every campaign for a Game Master.</param>
    public sealed record ChatIdentity(
        string UserId,
        string UserName,
        string DisplayName,
        bool IsGameMaster,
        IReadOnlyList<int> CampaignIds)
    {
        public bool HasAnyThread => IsGameMaster || CampaignIds.Count > 0;
    }

    /// <summary>An account you may write to.</summary>
    public sealed record ChatPeer(string UserId, string DisplayName, string? ImageUrl);

    /// <summary>
    /// The single place that answers "who may talk to whom". Chat is account-to-account: two accounts
    /// may write to each other when they share a non-finished campaign through <em>any</em> of their
    /// characters, and anyone may write to the Game Master.
    ///
    /// Every method takes the caller's <see cref="ApplicationDbContext"/> so a repository operation stays
    /// on one connection instead of opening a second context mid-transaction.
    /// </summary>
    public static class ChatAccess
    {
        /// <summary>
        /// Null when the account may not use chat at all: unknown id, or a hidden demo login. Demo
        /// accounts are shared throwaway logins, so letting them chat would leak messages between
        /// strangers trying the demo — the same reason push skips them.
        /// </summary>
        public static async Task<ChatIdentity?> ResolveIdentityAsync(
            ApplicationDbContext ctx,
            string? userId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var account = await ctx.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.Id, u.UserName })
                .FirstOrDefaultAsync(ct);

            if (account is null || SD.IsDemoUserName(account.UserName))
                return null;

            // Name lives on the ApplicationUser subtype, so it needs its own read — see AccountsAsync.
            var name = await ctx.ApplicationUsers
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync(ct);

            var isGm = await IsGameMasterAsync(ctx, account.Id, ct);

            List<int> campaignIds;
            if (isGm)
            {
                campaignIds = await ctx.Campaigns
                    .AsNoTracking()
                    .Where(c => !c.IsFinished)
                    .Select(c => c.Id)
                    .ToListAsync(ct);
            }
            else
            {
                var normalized = Normalize(account.UserName);
                campaignIds = await ctx.Campaigns
                    .AsNoTracking()
                    .Where(c => !c.IsFinished
                        && c.Characters.Any(ch => ch.UserName != null && ch.UserName.ToUpper() == normalized))
                    .Select(c => c.Id)
                    .ToListAsync(ct);
            }

            return new ChatIdentity(
                account.Id,
                account.UserName ?? string.Empty,
                DisplayNameOf(name, account.UserName),
                isGm,
                campaignIds);
        }

        /// <summary>Identity ids holding the Game Master role, demo accounts excluded.</summary>
        public static async Task<List<string>> GameMasterUserIdsAsync(
            ApplicationDbContext ctx,
            CancellationToken ct = default)
        {
            var candidates = await (
                from ur in ctx.UserRoles.AsNoTracking()
                join r in ctx.Roles.AsNoTracking() on ur.RoleId equals r.Id
                join u in ctx.Users.AsNoTracking() on ur.UserId equals u.Id
                where r.Name == SD.Role_GameMaster
                select new { u.Id, u.UserName }
            ).ToListAsync(ct);

            return candidates
                .Where(u => !SD.IsDemoUserName(u.UserName))
                .Select(u => u.Id)
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        public static async Task<bool> IsGameMasterAsync(
            ApplicationDbContext ctx,
            string userId,
            CancellationToken ct = default)
        {
            var ids = await GameMasterUserIdsAsync(ctx, ct);
            return ids.Contains(userId, StringComparer.Ordinal);
        }

        /// <summary>
        /// Accounts <paramref name="me"/> may write to directly, excluding the Game Masters: a GM is only
        /// ever reachable through the role thread, so a GM who also plays a hero does not show up twice.
        /// </summary>
        public static async Task<List<ChatPeer>> ReachablePeersAsync(
            ApplicationDbContext ctx,
            ChatIdentity me,
            CancellationToken ct = default)
        {
            if (me.CampaignIds.Count == 0)
                return new List<ChatPeer>();

            var ownerNames = await ctx.Campaigns
                .AsNoTracking()
                .Where(c => me.CampaignIds.Contains(c.Id))
                .SelectMany(c => c.Characters)
                .Where(ch => ch.NPCName != SD.GameMaster_NPCName && ch.UserName != null)
                .Select(ch => ch.UserName!)
                .Distinct()
                .ToListAsync(ct);

            var normalized = ownerNames
                .Where(n => !string.IsNullOrWhiteSpace(n) && !SD.IsDemoUserName(n))
                .Select(n => n.Trim().ToUpperInvariant())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (normalized.Count == 0)
                return new List<ChatPeer>();

            var gmIds = await GameMasterUserIdsAsync(ctx, ct);

            var ids = await ctx.Users
                .AsNoTracking()
                .Where(u => u.NormalizedUserName != null && normalized.Contains(u.NormalizedUserName))
                .Select(u => u.Id)
                .ToListAsync(ct);

            ids = ids
                .Where(id => id != me.UserId && !gmIds.Contains(id, StringComparer.Ordinal))
                .ToList();

            var accounts = await AccountsAsync(ctx, ids, ct);

            return accounts.Values
                .OrderBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Display name and borrowed portrait for each account id, keyed by id. Used for peer lists and
        /// for message authors alike, so a name renders the same wherever it appears.
        /// </summary>
        public static async Task<Dictionary<string, ChatPeer>> AccountsAsync(
            ApplicationDbContext ctx,
            IEnumerable<string> userIds,
            CancellationToken ct = default)
        {
            var wanted = (userIds ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var result = new Dictionary<string, ChatPeer>(StringComparer.Ordinal);
            if (wanted.Count == 0)
                return result;

            // Identity is mapped as table-per-hierarchy: UserName sits on the IdentityUser root while Name
            // and SelectedCharacterId sit on the ApplicationUser subtype. Reading the root first means a
            // row that predates the subtype still resolves, just without the extras.
            var accounts = await ctx.Users
                .AsNoTracking()
                .Where(u => wanted.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync(ct);

            var extras = (await ctx.ApplicationUsers
                .AsNoTracking()
                .Where(u => wanted.Contains(u.Id))
                .Select(u => new { u.Id, u.Name, u.SelectedCharacterId })
                .ToListAsync(ct))
                .ToDictionary(u => u.Id, StringComparer.Ordinal);

            var portraits = await PortraitsAsync(
                ctx,
                accounts.Select(a => (a.UserName, extras.GetValueOrDefault(a.Id)?.SelectedCharacterId ?? 0)),
                ct);

            foreach (var account in accounts)
            {
                result[account.Id] = new ChatPeer(
                    account.Id,
                    DisplayNameOf(extras.GetValueOrDefault(account.Id)?.Name, account.UserName),
                    portraits.GetValueOrDefault(KeyOf(account.UserName)));
            }

            return result;
        }

        /// <summary>
        /// Everyone who should learn about a new message in <paramref name="conversation"/>, as Identity
        /// ids. Shared by the in-process broadcaster and by push so a message can never reach one channel
        /// and miss the other. The conversation must have its participants loaded.
        /// </summary>
        public static async Task<List<string>> RecipientsAsync(
            ApplicationDbContext ctx,
            ChatConversation conversation,
            string? excludeUserId,
            CancellationToken ct = default)
        {
            var ids = new List<string>();

            if (conversation.Kind == ChatConversationKind.CampaignParty)
            {
                if (conversation.CampaignId is int campaignId)
                    ids.AddRange(await CampaignAccountIdsAsync(ctx, campaignId, ct));

                ids.AddRange(await GameMasterUserIdsAsync(ctx, ct));
            }
            else
            {
                ids.AddRange(conversation.Participants
                    .Where(p => !p.IsGameMasterRole && !string.IsNullOrWhiteSpace(p.UserId))
                    .Select(p => p.UserId!));

                if (conversation.Participants.Any(p => p.IsGameMasterRole))
                    ids.AddRange(await GameMasterUserIdsAsync(ctx, ct));
            }

            return ids
                .Where(id => !string.IsNullOrWhiteSpace(id)
                    && !string.Equals(id, excludeUserId, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>Accounts on the roster of <paramref name="campaignId"/>, demo logins excluded.</summary>
        public static async Task<List<string>> CampaignAccountIdsAsync(
            ApplicationDbContext ctx,
            int campaignId,
            CancellationToken ct = default)
        {
            var ownerNames = await ctx.Campaigns
                .AsNoTracking()
                .Where(c => c.Id == campaignId)
                .SelectMany(c => c.Characters)
                .Where(ch => ch.UserName != null)
                .Select(ch => ch.UserName!)
                .Distinct()
                .ToListAsync(ct);

            var normalized = ownerNames
                .Where(n => !string.IsNullOrWhiteSpace(n) && !SD.IsDemoUserName(n))
                .Select(n => n.Trim().ToUpperInvariant())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (normalized.Count == 0)
                return new List<string>();

            return await ctx.Users
                .AsNoTracking()
                .Where(u => u.NormalizedUserName != null && normalized.Contains(u.NormalizedUserName))
                .Select(u => u.Id)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Avatar for an account, borrowed from its characters — accounts have no portrait of their own.
        /// Prefers the character the player currently has selected. Keyed by upper-cased user name.
        /// </summary>
        public static async Task<Dictionary<string, string?>> PortraitsAsync(
            ApplicationDbContext ctx,
            IEnumerable<(string? UserName, int SelectedCharacterId)> accounts,
            CancellationToken ct = default)
        {
            var list = accounts
                .Where(a => !string.IsNullOrWhiteSpace(a.UserName))
                .ToList();

            var result = new Dictionary<string, string?>(StringComparer.Ordinal);
            if (list.Count == 0)
                return result;

            var names = list.Select(a => a.UserName!.Trim().ToUpperInvariant()).Distinct(StringComparer.Ordinal).ToList();

            var characters = await ctx.Characters
                .AsNoTracking()
                .Where(c => c.UserName != null && names.Contains(c.UserName.ToUpper()))
                .Select(c => new { c.Id, c.UserName, c.ImageUrl })
                .ToListAsync(ct);

            foreach (var account in list)
            {
                var key = KeyOf(account.UserName);
                var mine = characters
                    .Where(c => KeyOf(c.UserName) == key && !string.IsNullOrWhiteSpace(c.ImageUrl))
                    .ToList();

                var preferred = mine.FirstOrDefault(c => c.Id == account.SelectedCharacterId) ?? mine.FirstOrDefault();
                result[key] = preferred?.ImageUrl;
            }

            return result;
        }

        public static string KeyOf(string? userName) =>
            string.IsNullOrWhiteSpace(userName) ? string.Empty : userName.Trim().ToUpperInvariant();

        private static string DisplayNameOf(string? name, string? userName) =>
            !string.IsNullOrWhiteSpace(name) ? name!.Trim()
            : !string.IsNullOrWhiteSpace(userName) ? userName!.Trim()
            : string.Empty;

        private static string Normalize(string? userName) => KeyOf(userName);
    }
}
