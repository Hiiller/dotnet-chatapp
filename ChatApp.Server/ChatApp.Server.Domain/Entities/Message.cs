using System;
using System.ComponentModel.DataAnnotations;

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
        
        public bool IsRead { get; private set; }

        // Navigation properties
        public User Sender { get; private set; }
        public User Receiver { get; private set; }
        public Group Group { get; private set; }

        // 构造函数
        // 添加无参构造函数
        public Message()
        {
        }
        public Message(Guid senderId, string content, Guid? receiverId = null, Guid? groupId = null, DateTime? timestamp = null, bool isRead = false)
        {
            Id = Guid.NewGuid();  // 自动生成 Id
            SenderId = senderId;
            ReceiverId = receiverId;
            GroupId = groupId;
            Content = string.IsNullOrWhiteSpace(content) ? throw new ArgumentException("Message content cannot be empty.", nameof(content)) : content;
            Timestamp = timestamp ?? DateTime.UtcNow;  // 如果没有传递 timestamp，则使用当前 UTC 时间
            IsRead = false;
        }
        
        // 提供公共方法修改 IsRead
        public void MarkAsRead()
        {
            IsRead = true;
            Console.WriteLine("Message has been marked as read.");
        }
        
        public void MarkAsUnread()
        {
            IsRead = false;
            Console.WriteLine("Message has been marked as unread.");
        }
    }
}
