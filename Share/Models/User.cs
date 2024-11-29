using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class User
    {
        public string Id { get; init; }

        public string UserName { get; set; }

        /*
         * 类型为 ImageUrls，表示用户的头像图像。
         * ImageUrls 类用于存储与图像相关的 URL（原图、缩略图、小图标等），
         * 通过该属性用户可以设置或获取其头像。
         */
        //public ImageUrls Avatar { get; set; }

        public List<Connection> ActiveConnections { get; set; }

        public PassCode PassCode { get; init; }

        public User()
        {
            Id = Guid.NewGuid().ToString();
            ActiveConnections = new List<Connection>();
        }

        public User(string id, string username, PassCode passCode) : this()
        {
            Id = id;
            UserName = username;
            PassCode = passCode;
        }
    }

    public class Connection
    {
        public string Id { get; set; }

        public bool Active { get; set; }
    }
}
