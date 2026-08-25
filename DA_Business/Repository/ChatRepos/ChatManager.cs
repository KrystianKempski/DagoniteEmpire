using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DagoniteEmpire.Exceptions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DA_Business.Repository.CharacterReps
{
    public class ChatManager : IChatManager
    {
        private readonly AuthenticationStateProvider _authState;
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatManager(
            IDbContextFactory<ApplicationDbContext> db,
            AuthenticationStateProvider authState,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _authState = authState;
            _userManager = userManager;
        }

        public async Task<List<ChatMessage>> GetConversationAsync(string contactId)
        {
            using var contex = await _db.CreateDbContextAsync();
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
                return new List<ChatMessage>();

            return await contex.ChatMessages
                .AsNoTracking()
                .Where(h => (h.FromUserId == contactId && h.ToUserId == userId)
                            || (h.FromUserId == userId && h.ToUserId == contactId))
                .OrderBy(a => a.CreatedDate)
                .Include(a => a.FromUser)
                .Include(a => a.ToUser)
                .Select(x => new ChatMessage
                {
                    FromUserId = x.FromUserId,
                    Message = x.Message,
                    CreatedDate = x.CreatedDate,
                    Id = x.Id,
                    ToUserId = x.ToUserId,
                    ToUser = x.ToUser,
                    FromUser = x.FromUser,
                    IsRead = x.IsRead,
                })
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserDetailsAsync(string userId)
        {
            return await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == userId);
        }

        public async Task<ApplicationUser?> UpdateUserDetailsAsync(ApplicationUser updatedUser)
        {
            using var contex = await _db.CreateDbContextAsync();
            var user = await contex.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Id == updatedUser.Id);

            if (user is not null)
            {
                contex.Entry(user).CurrentValues.SetValues(updatedUser);
                await contex.SaveChangesAsync();
            }

            return user;
        }

        public async Task<List<ApplicationUser>> GetUsersAsync()
        {
            var userId = await GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
                return new List<ApplicationUser>();

            using var context = await _db.CreateDbContextAsync();

            var unreadCounts = await context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ToUserId == userId && !m.IsRead)
                .GroupBy(m => m.FromUserId)
                .Select(g => new { FromUserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.FromUserId, x => x.Count);

            var users = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.Id != userId)
                .Select(u => new ApplicationUser
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Name = u.Name,
                })
                .ToListAsync();

            foreach (var user in users)
            {
                if (unreadCounts.TryGetValue(user.Id, out var count) && count > 0)
                {
                    user.ShowBadge = true;
                    user.BadgeContent = count;
                }
            }

            return users;
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var userId = await GetCurrentUserIdAsync()
                    ?? throw new InvalidOperationException("Current user is required to save a chat message.");

                message.FromUserId = userId;
                message.CreatedDate = DateTime.UtcNow;
                message.ToUser = await contex.ApplicationUsers
                    .FirstOrDefaultAsync(user => user.Id == message.ToUserId);

                await contex.ChatMessages.AddAsync(message);
                await contex.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new RepositoryErrorException(
                    "Error in " + nameof(SaveMessageAsync), ex);
            }
        }

        /// <summary>
        /// Marks unread messages <em>from</em> <paramref name="contactId"/> <em>to</em> the current user as read.
        /// </summary>
        public async Task MakeMessageRedAsync(string contactId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var userId = await GetCurrentUserIdAsync();
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(contactId))
                    return;

                // Only inbound unread lines (operator-precedence bug used to also match unrelated rows).
                var messages = await contex.ChatMessages
                    .Where(h => h.FromUserId == contactId
                                && h.ToUserId == userId
                                && !h.IsRead)
                    .ToListAsync();

                if (messages.Count == 0)
                    return;

                foreach (var item in messages)
                    item.IsRead = true;

                await contex.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException(
                    "Error in " + nameof(MakeMessageRedAsync), ex);
            }
        }

        private async Task<string?> GetCurrentUserIdAsync()
        {
            var user = (await _authState.GetAuthenticationStateAsync()).User;
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
