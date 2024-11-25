using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ChatApp.Server.Domain.ValueObjects;

namespace ChatApp.Server.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }

        [Required]
        public string Username { get; private set; }

        [Required]
        public Email Email { get; private set; }

        [Required]
        public string DisplayName { get; private set; }

        [Required]
        public string PasswordHash { get; private set; }

        // 导航属性
        public ICollection<UserGroup> UserGroups { get; private set; }
        public ICollection<Message> SentMessages { get; private set; }
        public ICollection<Message> ReceivedMessages { get; private set; }

        public IEnumerable<Group> Groups => UserGroups.Select(ug => ug.Group);

        // 无参构造函数（EF Core使用）
        private User()
        {
            UserGroups = new List<UserGroup>();
            SentMessages = new List<Message>();
            ReceivedMessages = new List<Message>();
        }

        // 提供外部使用的构造函数
        public User(string username, Email email, string displayName, string passwordHash) : this()
        {
            Id = Guid.NewGuid();
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        }

        public void UpdateEmail(Email newEmail) => Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
        public void UpdateDisplayName(string newDisplayName) => DisplayName = newDisplayName ?? throw new ArgumentNullException(nameof(newDisplayName));
        public void UpdatePasswordHash(string newPasswordHash) => PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));

    }
}
