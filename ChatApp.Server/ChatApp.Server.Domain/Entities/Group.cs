// ChatApp.Server.Domain/Entities/Group.cs
using System;
using System.Collections.Generic;

namespace ChatApp.Server.Domain.Entities
{
    public class Group
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        // 导航属性
        public ICollection<User> Members { get; private set; }
        public ICollection<Message> Messages { get; private set; }

        // 构造函数
        public Group(string name)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));

            Members = new List<User>();
            Messages = new List<Message>();
        }

        // 方法：添加成员
        public void AddMember(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!Members.Contains(user))
            {
                Members.Add(user);
            }
        }

        // 方法：移除成员
        public void RemoveMember(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (Members.Contains(user))
            {
                Members.Remove(user);
            }
        }

        // 方法：更新群组名称
        public void UpdateName(string newName)
        {
            Name = newName ?? throw new ArgumentNullException(nameof(newName));
        }
    }
}
