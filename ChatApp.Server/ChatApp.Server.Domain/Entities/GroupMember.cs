using System;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Server.Domain.Entities;

public class GroupMember
{
    public Guid Id { get; private set; }
    
    [Required]
    public Guid GroupId { get; private set; }
    
    [Required]
    public Guid UserId { get; private set; }
    
    public GroupMemberRole Role { get; private set; }
    
    public DateTime JoinedAt { get; private set; }
    
    // 导航属性
    public Group Group { get; private set; } = null!;
    public User User { get; private set; } = null!;
    
    // 无参构造函数（EF Core 使用）
    private GroupMember()
    {
    }
    
    public GroupMember(Guid groupId, Guid userId, GroupMemberRole role = GroupMemberRole.Member)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }
    
    public void PromoteToAdmin()
    {
        Role = GroupMemberRole.Admin;
    }
    
    public void DemoteToMember()
    {
        Role = GroupMemberRole.Member;
    }
}

public enum GroupMemberRole
{
    Member = 0,
    Admin = 1,
    Creator = 2
}

