// ChatApp.Server.Domain/Entities/Message.cs
using System;
using System.Text.RegularExpressions;

namespace ChatApp.Server.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; private set; }
        public Guid SenderId { get; private set; }
        public Guid? ReceiverId { get; private set; } // 单聊的接收者
        public Guid? GroupId { get; private set; } // 群聊的群组ID
        public string Content { get; private set; }
        public DateTime Timestamp { get; private set; }

        // 导航属性
        public User Sender { get; private set; }
        public User Receiver { get; private set; }
        public Group Group { get; private set; }

        // 构造函数
        public Message(Guid senderId, string content, Guid? receiverId = null, Guid? groupId = null)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Message content cannot be empty.", nameof(content));

            Id = Guid.NewGuid();
            SenderId = senderId;
            ReceiverId = receiverId;
            GroupId = groupId;
            Content = content;
            Timestamp = DateTime.UtcNow;
        }

        // 方法：更新消息内容
        public void UpdateContent(string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                throw new ArgumentException("Message content cannot be empty.", nameof(newContent));

            Content = newContent;
        }
    }
}
