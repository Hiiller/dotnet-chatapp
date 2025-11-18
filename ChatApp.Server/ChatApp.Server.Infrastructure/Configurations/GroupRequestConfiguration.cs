using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Server.Infrastructure.Configurations;

public class GroupRequestConfiguration : IEntityTypeConfiguration<GroupRequest>
{
    public void Configure(EntityTypeBuilder<GroupRequest> builder)
    {
        builder.HasKey(gr => gr.Id);
        
        builder.Property(gr => gr.RequesterId)
            .IsRequired();
        
        builder.Property(gr => gr.GroupId)
            .IsRequired();
        
        builder.Property(gr => gr.CreatedAt)
            .IsRequired();
        
        builder.Property(gr => gr.Status)
            .IsRequired()
            .HasConversion<int>();
        
        // 配置与 Requester 的关系
        builder.HasOne(gr => gr.Requester)
            .WithMany()
            .HasForeignKey(gr => gr.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // 配置与 Group 的关系
        builder.HasOne(gr => gr.Group)
            .WithMany()
            .HasForeignKey(gr => gr.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // 创建唯一索引，防止重复请求
        builder.HasIndex(gr => new { gr.RequesterId, gr.GroupId, gr.Status });
    }
}

