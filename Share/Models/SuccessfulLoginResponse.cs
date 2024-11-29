using Shared.MessageTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class SuccessfulLoginResponse
    {
        public User User { get; set; }

        public List<MessagePayload> PreviousMessages { get; set; }

        public List<User> Users { get; set; }

        public AccessToken AccessToken { get; init; }
    }
}
