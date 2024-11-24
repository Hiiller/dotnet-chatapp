using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Domain;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Server.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        [Required]
        public DbSet<Group> Groups { get; set; }

        [Required]
        public DbSet<Message> Messages { get; set; }

        [Required]
        public DbSet<Notification> Notifications { get; set; }

        [Required]
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 应用所有配置类
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        }
    }
}
