using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChatApp.Server.Domain.ValueObjects
{
    public class Email : IEquatable<Email>
    {
        public string Address { get; private set; }
        public string Value => Address;

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // 无参构造函数（EF Core 使用）
        private Email() { }

        public Email(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Email address cannot be empty.", nameof(address));

            if (!EmailRegex.IsMatch(address))
                throw new ArgumentException("Invalid email address format.", nameof(address));

            Address = address;
        }

        public override bool Equals(object obj) => Equals(obj as Email);

        public bool Equals(Email other)
        {
            return other != null &&
                   Address.Equals(other.Address, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => Address.ToLowerInvariant().GetHashCode();

        public override string ToString() => Address;
    }
}
