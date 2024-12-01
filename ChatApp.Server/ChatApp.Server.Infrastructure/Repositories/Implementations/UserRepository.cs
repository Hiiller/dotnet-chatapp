//ChatApp.Server.Infrastructure.Repositories.Implementations.UserRepository.cs
using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Infrastructure.Data;
using ChatApp.Server.Domain.Repositories.Interfaces;
using Shared.Models;

namespace ChatApp.Server.Infrastructure.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.SentMessages)
                .Include(u => u.ReceivedMessages)
                //.Include(u => u.UserGroups)
                //.ThenInclude(ug => ug.Group)
                .Include(u => u.Friends)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.SentMessages)
                .Include(u => u.ReceivedMessages)
                //.Include(u => u.UserGroups)
                .ThenInclude(ug => ug.Group)
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        
        
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // 添加好友（通过 userId 和 friendUsername）
        public async Task AddFriendAsync(Guid userId, string friendUsername)
        {
            var user = await GetByIdAsync(userId);
            var friend = await GetByUsernameAsync(friendUsername);

            if (user != null && friend != null && !user.Friends.Contains(friend))
            {
                user.Friends.Add(friend);
                await _context.SaveChangesAsync();
            }
        }
        
        // 获取好友列表（通过 userId）
        public async Task<IEnumerable<User>> GetFriendsAsync(Guid userId)
        {
            var user = await GetByIdAsync(userId);
            return user?.Friends ?? Enumerable.Empty<User>();
        }
        
        // 检查是否是好友（通过 userId 和 friendUsername）
        public async Task<bool> IsFriendAsync(Guid userId, string friendUsername)
        {
            var user = await GetByIdAsync(userId);
            var friend = await GetByUsernameAsync(friendUsername);
            return user != null && friend != null && user.Friends.Contains(friend);
        }
        
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
