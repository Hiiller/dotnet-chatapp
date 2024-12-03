

namespace ChatApp.Server.Application.DTOs
{
    public class UserConnectionMessageDTO : MessageBaseDTO
    {
        public string Content { get; set; }

        public UserConnectionMessageDTO(string username, bool isConnected, string authorUsername, ChatRoleDTO role, bool isRead)
        {
            Content = isConnected ? $"{username} connected" : $"{username} disconnected";
            AuthorUsername = authorUsername;
            Role = role;
            IsRead = isRead;
        }
    }
}
