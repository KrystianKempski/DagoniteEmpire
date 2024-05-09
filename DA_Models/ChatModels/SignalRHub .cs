using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA_DataAccess.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DA_Models.ChatModels
{
    public class SignalRHub : Hub
    {
       // public const string HubUrl = "/chat";

        //public async Task Broadcast(string username, string message)
        //{
        //    await Clients.All.SendAsync("Broadcast", username, message);
        //}

        //public override Task OnConnectedAsync()
        //{
        //    Console.WriteLine($"{Context.ConnectionId} connected");
        //    return base.OnConnectedAsync();
        //}

        //public override async Task OnDisconnectedAsync(Exception e)
        //{
        //    Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
        //    await base.OnDisconnectedAsync(e);
        //}
        //public async Task SendMessageAsync( string userName, ChatMessage message)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", userName, message);
        //}
        //public async Task ChatNotificationAsync(string message, string receiverUserId, string senderUserId)
        //{
        //    await Clients.All.SendAsync("ReceiveChatNotification", message, receiverUserId, senderUserId);
        //}
    }
}
