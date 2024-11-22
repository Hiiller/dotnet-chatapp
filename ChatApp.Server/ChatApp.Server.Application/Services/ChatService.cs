using ChatApp.Server.Application.Interfaces;
using System.Threading.Tasks;

namespace ChatApp.Server.Application.Services
{
    public class ChatService : IChatService
    {
        public async Task SendMessageAsync(string chatroom, string username, string message)
        {
            // 在这里实现你的消息发送逻辑，可以通过 SignalR 等方式处理
            await Task.CompletedTask;
        }
    }
}
