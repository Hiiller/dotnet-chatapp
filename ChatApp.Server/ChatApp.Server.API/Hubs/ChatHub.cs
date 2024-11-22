using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatApp.Server.API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string chatroom, string username, string message)
        {
            await Clients.Group(chatroom).SendAsync("ReceiveMessage", username, message);
        }

        public async Task JoinRoom(string username,string chatroom)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatroom);
            await Clients.Group(chatroom).SendAsync("ReceiveMessage", $"{username} has joined the room.");
        }

        public async Task LeaveChatroom(string chatroom)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatroom);
        }
    }
}