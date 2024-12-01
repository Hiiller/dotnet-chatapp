using System;
namespace ChatApp.Client.DTOs;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid senderId { get; set; }
    public Guid? receiverId { get; set; }
    public Guid? groupId{ get; set; }
    public string content { get; set; }
    public DateTime Timestamp { get; set; }
    
}