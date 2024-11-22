using System;

namespace ChatApp.Server.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public string Chatroom { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
