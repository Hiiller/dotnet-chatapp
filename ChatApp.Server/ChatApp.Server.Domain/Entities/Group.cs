using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatApp.Server.Domain.Entities
{
    public class Group
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string GroupCode { get; private set; } // 8位随机群组码
        public Guid CreatorId { get; private set; } // 群创建者ID
        public DateTime CreatedAt { get; private set; }
        public string Description { get; private set; } = string.Empty;

        // 导航属性
        public ICollection<Message> Messages { get; private set; }
        public User Creator { get; private set; } = null!;

        private Group()
        {
            Messages = new List<Message>();
        }

        public Group(string name, Guid creatorId)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CreatorId = creatorId;
            CreatedAt = DateTime.UtcNow;
            GroupCode = GenerateGroupCode();
            Messages = new List<Message>();
        }

        public void UpdateName(string newName)
        {
            Name = newName ?? throw new ArgumentNullException(nameof(newName));
        }

        public void UpdateDescription(string? description)
        {
            Description = (description ?? string.Empty).Trim();
        }

        private static string GenerateGroupCode()
        {
            // 生成8位随机数字和字母组合
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}
