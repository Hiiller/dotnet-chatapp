
namespace ChatApp.Server.Application.DTOs
{
    public abstract class MessageBaseDTO
    {
        public string AuthorUsername { get; set; }
        public ChatRoleDTO Role { get; set; }
        public bool IsRead { get; set; }
    }
}
