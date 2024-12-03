

namespace ChatApp.Server.Application.DTOs
{
    public class TextMessageDTO : MessageBaseDTO
    {
        public string Content { get; set; }

        public TextMessageDTO(string content, string authorUsername, ChatRoleDTO role, bool isRead)
        {
            Content = content;
            AuthorUsername = authorUsername;
            Role = role;
            IsRead = isRead;
        }
    }
}
