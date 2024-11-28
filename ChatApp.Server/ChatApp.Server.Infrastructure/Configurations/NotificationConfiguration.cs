using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Server.Infrastructure.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            // 配置主键
            builder.HasKey(n => n.Id);

            // 配置字段约束
            builder.Property(n => n.Content)
                   .IsRequired();

            builder.Property(n => n.CreatedAt)
                   .IsRequired();

            // 配置外键关系
            builder.HasOne(n => n.Sender)
                   .WithMany() // 假设 Notification 不需要反向导航属性
                   .HasForeignKey(n => n.SenderId)
                   .OnDelete(DeleteBehavior.Restrict); // 删除时禁止级联删除发送者

            builder.HasOne(n => n.Receiver)
                   .WithMany() // 令 Notification 不需要反向导航属性
                   .HasForeignKey(n => n.ReceiverId)
                   .OnDelete(DeleteBehavior.Cascade); // 删除接收者时级联删除通知
        }
    }
}
