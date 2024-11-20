// ChatApp.Server.Domain/Interfaces/IMessageRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;

namespace ChatApp.Server.Domain.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message> GetByIdAsync(Guid id);
        Task<IEnumerable<Message>> GetMessagesByUserAsync(Guid userId, int pageNumber, int pageSize);
        Task<IEnumerable<Message>> GetMessagesByGroupAsync(Guid groupId, int pageNumber, int pageSize);
        Task AddAsync(Message message);
        void Update(Message message);
        void Remove(Message message);
        Task<bool> SaveChangesAsync();
    }
}
