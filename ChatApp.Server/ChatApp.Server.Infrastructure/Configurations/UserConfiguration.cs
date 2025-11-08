using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // 设置主键
            builder.HasKey(u => u.Id);

            // 配置 Username 属性
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            // 配置 DisplayName 属性
            builder.Property(u => u.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            // 配置 Password 属性
            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(256); // 假设最大密码长度为 256，根据实际需求调整

            // 个人简介（个性签名）
            builder.Property(u => u.Bio)
                .HasMaxLength(512)
                .IsRequired(false);

            // 头像（二进制存储）
            builder.Property(u => u.Avatar)
                .HasColumnType("BLOB")
                .IsRequired(false);

            // 个人长代码
            builder.Property(u => u.PersonalCode)
                .IsRequired(false)
                .HasMaxLength(64);

            // 配置与 SentMessages 的一对多关系
            builder.HasMany(u => u.SentMessages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Cascade); // 删除用户时级联删除发送的消息

            // 配置与 ReceivedMessages 的一对多关系
            builder.HasMany(u => u.ReceivedMessages)
                .WithOne(m => m.Receiver)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // 删除用户时不影响接收到的消息
            
            // 配置好友关系（多对多，自引用）
            builder.HasMany(u => u.Friends)
                .WithMany() // 不需要导航到自身的反向属性
                .UsingEntity<Dictionary<string, object>>(
                    "UserFriends", // 中间表名
                    b => b.HasOne<User>() // 配置外键到好友
                        .WithMany()
                        .HasForeignKey("FriendId")
                        .OnDelete(DeleteBehavior.Restrict),
                    b => b.HasOne<User>() // 配置外键到用户
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                );
            
        }
    }
}
