using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Security.Claims;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Business.Repository.CharacterReps
{
    public class ChatManager : IChatManager
    {
        private readonly AuthenticationStateProvider _authState;
        private readonly HttpClient _httpClient;
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IJSRuntime _jsRuntime;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatManager(HttpClient httpClient, IDbContextFactory<ApplicationDbContext> db, IJSRuntime jsRuntime, AuthenticationStateProvider authState, UserManager<ApplicationUser> userManager)
        {
            _httpClient = httpClient;
            _db = db;
            _jsRuntime = jsRuntime;
            _authState = authState;
            _userManager = userManager;
        }
        public async Task<List<ChatMessage>> GetConversationAsync(string contactId)
        {
            using var contex = await _db.CreateDbContextAsync();
            var user = (await _authState.GetAuthenticationStateAsync()).User;
            var userId = user.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
            var messages = await contex.ChatMessages
                    .Where(h => (h.FromUserId == contactId && h.ToUserId == userId) || (h.FromUserId == userId && h.ToUserId == contactId))
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
                        FromUser = x.FromUser
                    }).ToListAsync();

            return messages;
        }
        public async Task<ApplicationUser> GetUserDetailsAsync(string userId)
        {
            using var contex = await _db.CreateDbContextAsync();
            // var user = await contex.ApplicationUsers.Where(user => user.Id == userId).FirstOrDefaultAsync();
            var user = _userManager.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();

            return await user;
        }
        public async Task<List<ApplicationUser>> GetUsersAsync()
        {
            using var contex = await _db.CreateDbContextAsync();
            var user = (await _authState.GetAuthenticationStateAsync()).User;

            var userId = user.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
            var allUsers = _userManager.Users.Where(user => user.Id != userId).ToListAsync();

            return await allUsers;
        }
        public async Task SaveMessageAsync(ChatMessage message)
        {
            try
            {

            using var contex = await _db.CreateDbContextAsync();
            var user = (await _authState.GetAuthenticationStateAsync()).User;
            var userId = user.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
            message.FromUserId = userId;
            message.CreatedDate = DateTime.Now;
            message.ToUser = await contex.ApplicationUsers.Where(user => user.Id == message.ToUserId).FirstOrDefaultAsync();
            var result = contex.ChatMessages.AddAsync(message);
            await contex.SaveChangesAsync();

            }
            catch(Exception ex) 
            {
                ;
            }

        }
    }
}
