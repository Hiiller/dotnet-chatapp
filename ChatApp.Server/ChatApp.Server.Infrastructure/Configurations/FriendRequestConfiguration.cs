using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Server.Infrastructure.Configurations;

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.HasKey(fr => fr.Id);
        
        builder.Property(fr => fr.RequesterId)
            .IsRequired();
        
        builder.Property(fr => fr.ReceiverId)
            .IsRequired();
        
        builder.Property(fr => fr.CreatedAt)
            .IsRequired();
        
        builder.Property(fr => fr.Status)
            .IsRequired()
            .HasConversion<int>();
        
        // 配置与Requester的关系
        builder.HasOne(fr => fr.Requester)
            .WithMany()
            .HasForeignKey(fr => fr.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // 配置与Receiver的关系
        builder.HasOne(fr => fr.Receiver)
            .WithMany()
            .HasForeignKey(fr => fr.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // 创建唯一索引，防止重复请求
        builder.HasIndex(fr => new { fr.RequesterId, fr.ReceiverId })
            .IsUnique();
    }
}

