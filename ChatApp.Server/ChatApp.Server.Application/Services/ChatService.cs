using ChatApp.Server.Application.Interfaces;
using System.Threading.Tasks;

namespace ChatApp.Server.Application.Services
{
    public class ChatService : IChatService
    {
        public async Task SendMessageAsync(string chatroom, string username, string message)
        {
            // ������ʵ�������Ϣ�����߼�������ͨ�� SignalR �ȷ�ʽ����
            await Task.CompletedTask;
        }
    }
}
