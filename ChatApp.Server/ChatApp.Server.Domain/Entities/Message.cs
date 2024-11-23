using System;

namespace ChatApp.Server.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; private set; }
        public Guid SenderId { get; private set; }
        public Guid? ReceiverId { get; private set; }
        public Guid? GroupId { get; private set; }
        public string Content { get; private set; }
        public DateTime Timestamp { get; private set; }

        // µº∫Ω Ù–‘
        public User Sender { get; private set; }
        public User Receiver { get; private set; }
        public Group Group { get; private set; }

        public Message(Guid senderId, string content, Guid? receiverId = null, Guid? groupId = null)
        {
            Id = Guid.NewGuid();
            SenderId = senderId;
            ReceiverId = receiverId;
            GroupId = groupId;
            Content = string.IsNullOrWhiteSpace(content) ? throw new ArgumentException("Message content cannot be empty.", nameof(content)) : content;
            Timestamp = DateTime.UtcNow;
        }

        public void UpdateContent(string newContent)
        {
            Content = string.IsNullOrWhiteSpace(newContent) ? throw new ArgumentException("Message content cannot be empty.", nameof(newContent)) : newContent;
        }
    }
}
