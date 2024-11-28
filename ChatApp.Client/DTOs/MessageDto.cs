using System;
namespace ChatApp.Client.DTOs;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid? ReceiverId { get; set; }
    public Guid? GroupId{ get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    
}