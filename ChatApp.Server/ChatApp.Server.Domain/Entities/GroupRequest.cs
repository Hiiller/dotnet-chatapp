using System;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Server.Domain.Entities;

public class GroupRequest
{
    public Guid Id { get; private set; }
    
    [Required]
    public Guid RequesterId { get; private set; }
    
    [Required]
    public Guid GroupId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public GroupRequestStatus Status { get; private set; }
    
    // 导航属性
    public User Requester { get; private set; } = null!;
    public Group Group { get; private set; } = null!;
    
    // 无参构造函数（EF Core 使用）
    private GroupRequest()
    {
    }
    
    public GroupRequest(Guid requesterId, Guid groupId)
    {
        Id = Guid.NewGuid();
        RequesterId = requesterId;
        GroupId = groupId;
        CreatedAt = DateTime.UtcNow;
        Status = GroupRequestStatus.Pending;
    }
    
    public void Accept()
    {
        Status = GroupRequestStatus.Accepted;
    }
    
    public void Reject()
    {
        Status = GroupRequestStatus.Rejected;
    }
}

public enum GroupRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

