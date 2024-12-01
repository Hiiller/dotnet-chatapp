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
        //用当ConnectionStatus为false时，currentUsername为错误提示码
        //TODO：约定错误码
        //ErrorCode = -1 ：注册失败，用户名已存在
        //ErrorCode = -2 ：登录失败
        //ErrorCode = -3 ：服务器问题
        public string currentUsername { get; set; }
        public bool connectionStatus { get; set; }
        public int? errorCode { get; set; }

        public LoginResponse()
        {
            currentUserId = Guid.Empty;
            currentUsername = string.Empty;
            connectionStatus = false;
            errorCode = null;
        }
        
    }
    
}
