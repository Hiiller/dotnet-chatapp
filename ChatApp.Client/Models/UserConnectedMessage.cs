using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared.MessageTypes;

namespace ChatApp.Client.Models
{
    public class UserConnectedMessage : MessageBase
    {
        public string Content { get; init; }

        public UserConnectedMessage(string username)
        {
            Content = $"{username} connected";
        }


        internal override MessagePayload ToMessagePayload()
        {
            throw new NotImplementedException();
        }
    }

    public class UserDisconnectedMessage : MessageBase
    {
        public string Content { get; init; }

        public UserDisconnectedMessage(string username)
        {
            Content = $"{username} disconnected";
        }


        internal override MessagePayload ToMessagePayload()
        {
            throw new NotImplementedException();
        }
    }
}
