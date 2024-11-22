//ChatApp.Server.Application.Interfaces.IChatService.cs
using System.Threading.Tasks;

namespace ChatApp.Server.Application.Interfaces
{
    public interface IChatService
    {
        Task SendMessageAsync(string chatroom, string username, string message);
    }
}
