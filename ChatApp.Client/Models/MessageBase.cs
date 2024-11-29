using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using Shared.MessageTypes;

namespace ChatApp.Client.Models
{
    public abstract class MessageBase
    {
        public string AuthorUsername { get; init; }

        // 将对象转换为JSON字符串
        protected string ObjectToString(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        // 将JSON字符串转换为对象
        internal T StringToObject<T>(string jsonString)
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        public ChatRoleType Role { get; init; }

        public bool IsRead { get; set; }

        internal abstract MessagePayload ToMessagePayload();
    }
}
