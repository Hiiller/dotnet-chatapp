//ChatApp.Server.Application/Services/ChatService.cs    
using ChatApp.Server.Application.Interfaces;
using System.Threading.Tasks;

namespace ChatApp.Server.Application.Services
{
    public class ChatService : IChatService
    {
        public async Task SendMessageAsync(string chatroom, string username, string message)
        {
            //TODO: 实现发送消息的逻辑
            await Task.CompletedTask;
        }
    }
}
