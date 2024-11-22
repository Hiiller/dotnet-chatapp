namespace ChatApp.Server.Application.DTOs
{
    public class SendMessageDto
    {
        public string Chatroom { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
    }
}
