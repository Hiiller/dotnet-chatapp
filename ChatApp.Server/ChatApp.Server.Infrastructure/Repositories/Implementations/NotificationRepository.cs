using ChatApp.Server.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Domain.Repositories.Interfaces;
using ChatApp.Server.Infrastructure.Data;

namespace ChatApp.Server.Infrastructure.Services
{
    public class NotificationService : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);
        }
    }
}
