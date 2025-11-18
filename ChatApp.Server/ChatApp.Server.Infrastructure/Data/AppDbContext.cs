using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupRequest> GroupRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 应用所有配置类（使用 Fluent API 的配置类）
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // 如果需要手动配置，可以在此处补充
        }
    }
}