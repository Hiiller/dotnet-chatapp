using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Server.Infrastructure.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(g => g.Id);
        
        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(g => g.GroupCode)
            .IsRequired()
            .HasMaxLength(8);
        
        builder.Property(g => g.CreatorId)
            .IsRequired();
        
        builder.Property(g => g.CreatedAt)
            .IsRequired();

        builder.Property(g => g.Description)
            .HasMaxLength(512);
        
        // 配置与 Creator 的关系
        builder.HasOne(g => g.Creator)
            .WithMany()
            .HasForeignKey(g => g.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // 配置与 Messages 的一对多关系
        builder.HasMany(g => g.Messages)
            .WithOne(m => m.Group)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // 创建唯一索引
        builder.HasIndex(g => g.GroupCode)
            .IsUnique();
    }
}

