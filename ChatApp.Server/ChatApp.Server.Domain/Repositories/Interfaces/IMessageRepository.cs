using ChatApp.Server.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Server.Domain.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        //根据Id获取消息
        Task<Message?> GetByIdAsync(Guid id);
        //获取所有消息
        Task<IEnumerable<Message>> GetAllAsync();
        //获取特定用户的信息
        Task<IEnumerable<Message>> GetMessagesByUserIdAsync(Guid userId);
        Task<Dictionary<Guid, string>> GetRecentContactsByUserIdAsync(Guid userId);
        //获取特定用户的全部未读消息
        Task<List<Message>> GetUnreadMessagesByUserIdAsync(Guid userId);
        //获取特定用户的全部已读消息
        Task<List<Message>> GetReadMessagesByUserIdAsync(Guid userId);
        //获取两个用户之间的所有消息
        Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(Guid user1Id, Guid user2Id);
        //获取两个用户之间的所有未读消息
        Task<List<Message>> GetUnreadMessagesBetweenUsersAsync(Guid receiverId, Guid senderId);
        //标记消息为已读
        Task MarkMessagesAsReadAsync(Guid receiverId, Guid senderId);
        //获取用户的最近对话列表
        Task<IEnumerable<Message?>> GetRecentMessagesByUserIdAsync(Guid userId);
        //添加消息
        Task AddAsync(Message message);
        //更新消息
        Task UpdateAsync(Message message);
        //删除消息
        Task DeleteAsync(Guid id);
 
    }
}
