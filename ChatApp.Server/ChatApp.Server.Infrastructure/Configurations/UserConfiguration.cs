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
        }
    }
}