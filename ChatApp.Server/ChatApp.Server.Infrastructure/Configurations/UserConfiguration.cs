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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Address)
                     .IsRequired()
                     .HasColumnName("Email"); // 映射到数据库的 Email 列
            });

            builder.HasMany(u => u.UserGroups)
                   .WithOne(ug => ug.User)
                   .HasForeignKey(ug => ug.UserId);
        }
    }
}
