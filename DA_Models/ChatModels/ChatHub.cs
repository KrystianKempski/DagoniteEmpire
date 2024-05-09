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
