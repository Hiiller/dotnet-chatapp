using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.MessageTypes
{
    public class MessagePayload
    {
        public string Id { get; init; }

        public string Base64Payload { get; init; }

        public MessageType Type { get; init; }


        public string AuthorUsername { get; set; }


        public MessagePayload(string base64Payload, MessageType type)
        {
            Id = Guid.NewGuid().ToString();
            Type = type;
            Base64Payload = base64Payload;
        }

    }
}
