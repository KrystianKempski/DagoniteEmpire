using System.Security.Claims;
using DA_DataAccess.Chat;
using Microsoft.AspNetCore.SignalR;

namespace DA_Models.ChatModels
{
    /// <summary>
    /// Shared SignalR hub: private chat plus chapter/battle live notifications.
    /// Chat payloads go only to the two participants; other events stay broadcast to others.
    /// </summary>
    public class ChatHub : Hub
    {
        public const string HubUrl = "/ChatHub";

        public static string ChatUserGroup(string userId) => $"chat-user:{userId}";

        /// <summary>
        /// Deliver a private chat message only to sender and recipient connections.
        /// </summary>
        public async Task SendMessage(string username, ChatMessage message)
        {
            var callerId = ResolveCallerUserId();
            if (string.IsNullOrEmpty(callerId))
                throw new HubException("Authentication required to send chat messages.");

            if (message is null || string.IsNullOrWhiteSpace(message.ToUserId))
                throw new HubException("Chat message recipient is required.");

            // Never trust FromUserId from the client.
            message.FromUserId = callerId;

            var toId = message.ToUserId.Trim();
            message.ToUserId = toId;

            await Clients.Groups(ChatUserGroup(callerId), ChatUserGroup(toId))
                .SendAsync("SendMessage", username, message);
        }

        /// <summary>
        /// Notify all clients about a new post in a chapter
        /// </summary>
        public async Task NotifyNewPost(int chapterId, int postId)
        {
            await Clients.Others.SendAsync("NewPost", chapterId, postId);
        }

        /// <summary>
        /// Notify all clients about an updated post
        /// </summary>
        public async Task NotifyPostUpdated(int chapterId, int postId)
        {
            await Clients.Others.SendAsync("PostUpdated", chapterId, postId);
        }

        /// <summary>
        /// Notify all clients about a deleted post
        /// </summary>
        public async Task NotifyPostDeleted(int chapterId, int postId)
        {
            await Clients.Others.SendAsync("PostDeleted", chapterId, postId);
        }

        /// <summary>
        /// Notify all clients that the tactical battle map for a chapter changed
        /// </summary>
        public async Task NotifyBattleMapUpdated(int chapterId)
        {
            await Clients.Others.SendAsync("BattleMapUpdated", chapterId);
        }

        /// <summary>
        /// Notify all clients that a barony battle map changed
        /// </summary>
        public async Task NotifyBaronyBattleMapUpdated(int baronyId)
        {
            await Clients.Others.SendAsync("BaronyBattleMapUpdated", baronyId);
        }

        /// <summary>
        /// Explicit join used after connect/reconnect so private chat works even if
        /// <see cref="OnConnectedAsync"/> ran without a resolved user id.
        /// </summary>
        public async Task JoinChatUserGroup()
        {
            var userId = ResolveCallerUserId();
            if (string.IsNullOrEmpty(userId))
                throw new HubException("Authentication required to join chat.");

            await Groups.AddToGroupAsync(Context.ConnectionId, ChatUserGroup(userId));
        }

        public override async Task OnConnectedAsync()
        {
            var userId = ResolveCallerUserId();
            if (!string.IsNullOrEmpty(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, ChatUserGroup(userId));

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = ResolveCallerUserId();
            if (!string.IsNullOrEmpty(userId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatUserGroup(userId));

            await base.OnDisconnectedAsync(exception);
        }

        private string? ResolveCallerUserId()
        {
            if (!string.IsNullOrEmpty(Context.UserIdentifier))
                return Context.UserIdentifier;

            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
