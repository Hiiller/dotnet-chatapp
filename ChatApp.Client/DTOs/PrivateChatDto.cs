using System;

namespace ChatApp.Client.DTOs;

public class PrivateChatDto
{
    public Guid UserId { get; set; }  // 对方用户ID
    public string DisplayName { get; set; }  // 对方显示名
    public string LastMessageContent { get; set; }  // 最近一条消息内容
    public DateTime LastMessageTimestamp { get; set; }  // 最近一条消息的时间
}