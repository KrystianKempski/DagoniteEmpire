using DA_DataAccess.Chat;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ChatModels
{
    public class ChatHub : Hub
    {
        public const string HubUrl = "/ChatHub";

        public async Task SendMessage(string username, ChatMessage message)
        {
           // Console.WriteLine($"{username} sent message");

            await Clients.All.SendAsync("SendMessage", username, message);
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

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} hub connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }
    }
}
