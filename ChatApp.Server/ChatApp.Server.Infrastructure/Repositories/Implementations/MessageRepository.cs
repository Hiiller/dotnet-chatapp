    using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Domain.Repositories.Interfaces;
using ChatApp.Server.Infrastructure.Data;

namespace ChatApp.Server.Infrastructure.Repositories.Implementations
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;

        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Message?> GetByIdAsync(Guid id)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Group)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Message>> GetAllAsync()
        {
            return await _context.Messages.ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetMessagesByUserIdAsync(Guid userId)
        {
            return await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .ToListAsync();
        }
        
        public async Task<Dictionary<Guid, string>> GetRecentContactsByUserIdAsync(Guid userId)
        {
            // 确保 userId 有效
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            // 查询所有与该用户相关的消息
            var recentMessages = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.Timestamp) // 按时间排序
                .ToListAsync();

            // 提取与该用户有交互的联系人 ID（排除自己）
            var contactIds = recentMessages
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Where(id => id.HasValue && id != userId) // 确保非空并排除自己
                .Distinct()
                .ToList();

            // 查询用户名并返回结果
            var contacts = await _context.Users
                .Where(u => contactIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            return contacts;
        }
        
        public async Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(Guid user1Id, Guid user2Id)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                            (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Message?>> GetRecentMessagesByUserIdAsync(Guid userId)
        {
            return await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
                .ToListAsync();
        }


        public async Task AddAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Message message)
        {
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
            }
        }
    }
}
