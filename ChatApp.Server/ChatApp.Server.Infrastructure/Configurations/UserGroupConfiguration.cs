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
    public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
    {
        public void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            builder.HasKey(ug => new { ug.UserId, ug.GroupId });

            builder.HasOne(ug => ug.User)
                   .WithMany(u => u.UserGroups)
                   .HasForeignKey(ug => ug.UserId);

            builder.HasOne(ug => ug.Group)
                   .WithMany(g => g.UserGroups)
                   .HasForeignKey(ug => ug.GroupId);
        }
    }
}
