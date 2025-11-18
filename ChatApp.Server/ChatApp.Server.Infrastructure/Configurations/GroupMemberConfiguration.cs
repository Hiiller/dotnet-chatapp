using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Server.Infrastructure.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.HasKey(gm => gm.Id);
        
        builder.Property(gm => gm.GroupId)
            .IsRequired();
        
        builder.Property(gm => gm.UserId)
            .IsRequired();
        
        builder.Property(gm => gm.Role)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(gm => gm.JoinedAt)
            .IsRequired();
        
        // 配置与 Group 的关系
        builder.HasOne(gm => gm.Group)
            .WithMany()
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // 配置与 User 的关系
        builder.HasOne(gm => gm.User)
            .WithMany()
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // 创建唯一索引，防止重复加入
        builder.HasIndex(gm => new { gm.GroupId, gm.UserId })
            .IsUnique();
    }
}

