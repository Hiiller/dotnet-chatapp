using System;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using ChatApp.Client.Models;

namespace ChatApp.Client.DTOs;

    public class MessageDto
    {
    public Guid? id { get; set; }
    public Guid senderId { get; set; }
    public Guid? receiverId { get; set; }
    public Guid? groupId{ get; set; }
    public string content { get; set; }
        public string? attachmentUrl { get; set; }
    public DateTime timestamp { get; set; }

    //public int Role { get; set; } = 1;

    // 客户端专用：群聊展示发送者昵称
        public string? senderName { get; set; }

        public string SenderDisplay => string.IsNullOrWhiteSpace(senderName) ? senderId.ToString() : senderName;

    public bool IsRead { get; set; }
    public ChatRoleType ChatRoleType { get; set; } 

    // UI-only bitmap for image messages, ignored in JSON serialization
    [JsonIgnore]
    public Bitmap? attachmentImage { get; set; }

    // UI-only: sender avatar bitmap for displaying next to nickname
    [JsonIgnore]
    public Bitmap? senderAvatar { get; set; }

    }