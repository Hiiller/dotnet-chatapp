// ChatApp.Server.Domain/Interfaces/IMessageRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;

namespace ChatApp.Server.Domain.Interfaces
{
    public interface IMessageRepository
    {
        /// <summary>
        /// 根据 ID 获取单条消息，如果未找到返回 null。
        /// </summary>
        Task<Message> GetByIdAsync(Guid id);
        
        /// <summary>
        /// 根据用户 ID 获取该用户的消息列表，支持分页。如果没有消息，返回空集合。
        /// </summary>
        Task<IEnumerable<Message>> GetMessagesByUserAsync(Guid userId, int pageNumber, int pageSize);
        
        /// <summary>
        /// 根据群组 ID 获取消息列表，支持分页。如果没有消息，返回空集合。
        /// 第一版只完成私聊功能，所以暂时不实现群聊功能。
        /// </summary>
        Task<IEnumerable<Message>> GetMessagesByGroupAsync(Guid groupId, int pageNumber, int pageSize);
        
        /// <summary>
        /// 添加一条新消息到数据库。
        /// </summary>
        Task AddAsync(Message message);
        
        /// <summary>
        /// 更新一条消息。如果消息不存在，操作无效。
        /// </summary>
        void Update(Message message);
        
        /// <summary>
        /// 删除一条消息。如果消息不存在，操作无效。
        /// </summary>
        void Remove(Message message);
        
        /// <summary>
        /// 保存对数据库的所有更改。如果保存失败，返回 false。
        /// </summary>
        Task<bool> SaveChangesAsync();
    }
}
