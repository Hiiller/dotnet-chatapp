using ChatApp.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Domain;

namespace ChatApp.Server.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Group> Groups { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.HasKey(ug => new { ug.UserId, ug.GroupId }); // 定义复合主键

                entity.HasOne(ug => ug.User)
                    .WithMany(u => u.UserGroups)
                    .HasForeignKey(ug => ug.UserId);

                entity.HasOne(ug => ug.Group)
                    .WithMany(g => g.UserGroups)
                    .HasForeignKey(ug => ug.GroupId);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).IsRequired();
                entity.HasMany(g => g.Messages)
                      .WithOne(m => m.Group)
                      .HasForeignKey(m => m.GroupId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Content).IsRequired().HasMaxLength(500);
                entity.HasOne(m => m.Sender).WithMany(u => u.SentMessages).HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(m => m.Receiver).WithMany(u => u.ReceivedMessages).HasForeignKey(m => m.ReceiverId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Content).IsRequired();
                entity.Property(n => n.CreatedAt).IsRequired();
                entity.HasOne(n => n.Sender).WithMany().HasForeignKey(n => n.SenderId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(n => n.Receiver).WithMany().HasForeignKey(n => n.ReceiverId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.DisplayName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                // 配置 Email 作为值对象
                entity.OwnsOne(u => u.Email, email =>
                {
                    email.Property(e => e.Address)
                        .IsRequired()
                        .HasColumnName("Email"); // 映射到数据库的 Email 列
                });

                // 配置导航属性（可选）
                entity.HasMany(u => u.UserGroups)
                    .WithOne(ug => ug.User)
                    .HasForeignKey(ug => ug.UserId);
            });
        }
    }
}
