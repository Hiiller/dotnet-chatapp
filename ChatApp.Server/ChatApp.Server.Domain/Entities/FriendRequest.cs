using System.ComponentModel.DataAnnotations;

namespace ChatApp.Server.Domain.Entities;

public class FriendRequest
{
    public Guid Id { get; private set; }
    
    [Required]
    public Guid RequesterId { get; private set; }
    
    [Required]
    public Guid ReceiverId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public FriendRequestStatus Status { get; private set; }
    
    // 导航属性
    public User Requester { get; private set; } = null!;
    public User Receiver { get; private set; } = null!;
    
    // 无参构造函数（EF Core 使用）
    private FriendRequest()
    {
    }
    
    public FriendRequest(Guid requesterId, Guid receiverId)
    {
        Id = Guid.NewGuid();
        RequesterId = requesterId;
        ReceiverId = receiverId;
        CreatedAt = DateTime.UtcNow;
        Status = FriendRequestStatus.Pending;
    }
    
    public void Accept()
    {
        Status = FriendRequestStatus.Accepted;
    }
    
    public void Reject()
    {
        Status = FriendRequestStatus.Rejected;
    }
}

public enum FriendRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

