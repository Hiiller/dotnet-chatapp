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
        Task<Message?> GetByIdAsync(Guid id);
        Task<IEnumerable<Message>> GetAllAsync();
        Task<IEnumerable<Message>> GetMessagesByUserIdAsync(Guid userId);
        Task AddAsync(Message message);
        Task UpdateAsync(Message message);
        Task DeleteAsync(Guid id);
    }
}
