// ChatApp.Server.Domain/Entities/User.cs
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ChatApp.Server.Domain.ValueObjects;

namespace ChatApp.Server.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public Email Email { get; private set; }
        //目前不支持电话号码，只考虑电子邮件地址
        //public PhoneNumber PhoneNumber { get; private set; }
        public string DisplayName { get; private set; }
        public string PasswordHash { get; private set; }

        // 导航属性
        public ICollection<Message> MessagesSent { get; private set; }
        public ICollection<Message> MessagesReceived { get; private set; }
        public ICollection<Group> Groups { get; private set; }

        // 构造函数
        public User(string username, Email email, PhoneNumber phoneNumber, string displayName, string passwordHash)
        {
            Id = Guid.NewGuid();
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            //当前不支持电话号码，只考虑电子邮件地址
            //PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));

            MessagesSent = new List<Message>();
            MessagesReceived = new List<Message>();
            Groups = new List<Group>();
        }

        // 方法：更新用户信息
        public void UpdateEmail(Email newEmail)
        {
            Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
        }

        /// <summary>
        /// 目前不支持修改电话号码。
        /// </summary>
        /// <param name="newDisplayName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        //public void UpdatePhoneNumber(PhoneNumber newPhoneNumber)
        //{
        //    PhoneNumber = newPhoneNumber ?? throw new ArgumentNullException(nameof(newPhoneNumber));
        //}
        

        public void UpdateDisplayName(string newDisplayName)
        {
            DisplayName = newDisplayName ?? throw new ArgumentNullException(nameof(newDisplayName));
        }

        public void UpdatePasswordHash(string newPasswordHash)
        {
            PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        }
    }
}
