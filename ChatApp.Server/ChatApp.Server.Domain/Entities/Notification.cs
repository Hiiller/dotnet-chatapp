using System;

namespace ChatApp.Server.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; private set; } // 唯一标识符
        public Guid SenderId { get; private set; } // 通知的发送者（可选，系统通知可以为 null）
        public Guid ReceiverId { get; private set; } // 通知的接收者
        public string Content { get; private set; } // 通知内容
        public DateTime CreatedAt { get; private set; } // 通知创建时间
        public bool IsRead { get; private set; } // 是否已读状态

        // 导航属性
        public User Sender { get; private set; } // 发送者用户
        public User Receiver { get; private set; } // 接收者用户

        // 构造函数
        public Notification(Guid receiverId, string content, Guid? senderId = null)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Notification content cannot be empty.", nameof(content));

            Id = Guid.NewGuid();
            ReceiverId = receiverId;
            Content = content;
            SenderId = senderId ?? Guid.Empty; // 如果发送者为空，默认设置为系统
            CreatedAt = DateTime.UtcNow;
            IsRead = false; // 默认未读
        }

        // 标记为已读
        public void MarkAsRead()
        {
            IsRead = true;
        }

        // 方法：更新通知内容
        public void UpdateContent(string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                throw new ArgumentException("Notification content cannot be empty.", nameof(newContent));

            Content = newContent;
        }
    }
}