//ChatApp.Server.Domain.Repositories.Interfaces.INotificationRepository.cs
using ChatApp.Server.Domain.Entities;

namespace ChatApp.Server.Domain.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);
        
    }
}