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
            await Clients.All.SendAsync("NewPost", chapterId, postId);
        }

        /// <summary>
        /// Notify all clients about an updated post
        /// </summary>
        public async Task NotifyPostUpdated(int chapterId, int postId)
        {
            await Clients.All.SendAsync("PostUpdated", chapterId, postId);
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
