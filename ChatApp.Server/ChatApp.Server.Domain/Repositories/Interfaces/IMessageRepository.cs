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
        //获取两个用户之间的所有消息
        Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(Guid user1Id, Guid user2Id);
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
