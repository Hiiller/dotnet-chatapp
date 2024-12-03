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
        // 获取特定用户的未读消息
        public async Task<List<Message>> GetUnreadMessagesByUserIdAsync(Guid userId)
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => g
                    .OrderByDescending(m => m.Timestamp)
                    .First()) // 明确获取第一条记录
                .ToListAsync();
        }
        
        // 获取两个用户之间未读消息，有receiverId和senderIde
        public async Task<List<Message>> GetUnreadMessagesBetweenUsersAsync(Guid receiverId, Guid senderId)
        {
            Console.WriteLine("In GetUnreadMessagesBetweenUsersAsync, receiverId: " + receiverId + ", senderId: " + senderId);
            var messages = await _context.Messages
                .Where(m => m.ReceiverId == receiverId && m.SenderId == senderId && !m.IsRead)
                .ToListAsync();
            Console.WriteLine($"Found {messages.Count} unread messages.");
            return messages;
        }
        
        
        // 标记为已读
        public async Task MarkMessagesAsReadAsync(Guid receiverId, Guid senderId)
        {
            Console.WriteLine("Before getUnreadMessagesBetweenUsersAsync, receiverId: " + receiverId + ", senderId: " + senderId);
            // 查找未读消息
            var unreadMessages = await GetUnreadMessagesBetweenUsersAsync(receiverId, senderId);
            if (unreadMessages == null || !unreadMessages.Any())
            {
                Console.WriteLine("No unread messages to mark as read.");
                return; // 如果没有未读消息，直接返回
            }
            // 更新每条消息的已读状态
            foreach (var message in unreadMessages)
            {
                _context.Entry(message).State = EntityState.Modified;
                message.MarkAsRead();
            }

            // 保存更改到数据库
            //await _context.SaveChangesAsync();
            var affectedRows = await _context.SaveChangesAsync();
            Console.WriteLine($"Marked {affectedRows} messages as read.");
        }
        
        
        // 获取特定用户的全部已读消息
        public async Task<List<Message>> GetReadMessagesByUserIdAsync(Guid userId)
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == userId && m.IsRead) // 查询已读消息
                .OrderBy(m => m.Timestamp) // 按时间排序
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
