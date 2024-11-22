using System;

namespace ChatApp.Server.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }  // 消息唯一标识符
        public string Sender { get; set; }  // 发送消息的用户
        public string Content { get; set; }  // 消息内容
        public string Chatroom { get; set; }  // 消息所属的聊天室
        public DateTime Timestamp { get; set; }  // 消息时间戳
    }
}