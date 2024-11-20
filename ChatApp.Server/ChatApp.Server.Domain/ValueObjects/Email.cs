// ChatApp.Server.Domain/ValueObjects/Email.cs
using System;
using System.Text.RegularExpressions;

namespace ChatApp.Server.Domain.ValueObjects
{
    public class Email : IEquatable<Email>
    {
        public string Address { get; private set; }

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Email(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Email address cannot be empty.", nameof(address));

            if (!EmailRegex.IsMatch(address))
                throw new ArgumentException("Invalid email address format.", nameof(address));

            Address = address;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Email);
        }

        public bool Equals(Email other)
        {
            return other != null &&
                   Address.Equals(other.Address, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Address.ToLowerInvariant().GetHashCode();
        }

        public override string ToString()
        {
            return Address;
        }
    }
}
