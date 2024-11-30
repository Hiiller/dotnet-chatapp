using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Infrastructure.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            // 配置主键
            builder.HasKey(m => m.Id);

            // 配置 Content 属性
            builder.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(500); // 消息内容最大长度为 500

            // 配置 Timestamp 属性
            builder.Property(m => m.Timestamp)
                .IsRequired();

            // 配置 Sender 和 Message 的关系
            builder.HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Cascade); // 删除用户时，级联删除其发送的消息

            // 配置 Receiver 和 Message 的关系
            builder.HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // 删除用户时，保留其接收的消息

            // 配置 Group 和 Message 的关系
            builder.HasOne(m => m.Group)
                .WithMany()
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.SetNull); // 删除群组时，消息的 GroupId 设置为 NULL
        }
    }
}