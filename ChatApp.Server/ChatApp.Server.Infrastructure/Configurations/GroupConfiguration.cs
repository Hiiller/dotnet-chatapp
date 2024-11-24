using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Domain.Entities;

namespace ChatApp.Server.Infrastructure.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                   .IsRequired();

            builder.HasMany(g => g.Messages)
                   .WithOne(m => m.Group)
                   .HasForeignKey(m => m.GroupId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
