using Shared.MessageTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class LoginResponse
    {
        public Guid currentUserId { get; set; }
        
        public string currentUsername { get; set; }
        public bool connectionStatus { get; set; }
        
        public string ServerUrl { get; set; }

        public LoginResponse()
        {
            currentUserId = Guid.Empty;
            currentUsername = string.Empty;
            connectionStatus = false;
            ServerUrl = string.Empty;
        }
        
    }
    
}
