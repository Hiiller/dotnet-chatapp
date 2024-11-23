using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Server.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public Guid? SenderId { get; private set; }
        public Guid ReceiverId { get; private set; }
        public string Content { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsRead { get; private set; }

        public User Sender { get; private set; }
        public User Receiver { get; private set; }

        public Notification(Guid receiverId, string content, Guid? senderId = null)
        {
            Id = Guid.NewGuid();
            ReceiverId = receiverId;
            Content = string.IsNullOrWhiteSpace(content) ? throw new ArgumentException("Notification content cannot be empty.", nameof(content)) : content;
            SenderId = senderId;
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }

        public void MarkAsRead() => IsRead = true;

        public void UpdateContent(string newContent)
        {
            Content = string.IsNullOrWhiteSpace(newContent) ? throw new ArgumentException("Notification content cannot be empty.", nameof(newContent)) : newContent;
        }
    }
}
