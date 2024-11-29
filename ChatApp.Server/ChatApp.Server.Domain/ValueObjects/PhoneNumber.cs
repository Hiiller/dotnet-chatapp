using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChatApp.Server.Domain.ValueObjects
{
    public class PhoneNumber : IEquatable<PhoneNumber>
    {
        public string Number { get; private set; }

        private static readonly Regex PhoneRegex = new Regex(
            @"^\+?[1-9]\d{1,14}$", // 简单的E.164格式验证
            RegexOptions.Compiled);

        public PhoneNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Phone number cannot be empty.", nameof(number));

            if (!PhoneRegex.IsMatch(number))
                throw new ArgumentException("Invalid phone number format.", nameof(number));

            Number = number;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PhoneNumber);
        }

        public bool Equals(PhoneNumber other)
        {
            return other != null &&
                   Number.Equals(other.Number);
        }

        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }

        public override string ToString()
        {
            return Number;
        }
    }
}
