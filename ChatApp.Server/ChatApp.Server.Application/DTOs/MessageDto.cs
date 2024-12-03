namespace ChatApp.Server.Application.DTOs;

public class MessageDto
{
    public Guid? id { get; set; }
    public Guid senderId { get; set; }
    public Guid? receiverId { get; set; }
    public Guid? groupId{ get; set; }
    public string content { get; set; }
    public DateTime timestamp { get; set; }
    
}