using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Domain.Repositories.Interfaces;
using ChatApp.Server.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Infrastructure.Repositories.Implementations;

public class FriendRequestRepository : IFriendRequestRepository
{
    private readonly AppDbContext _context;

    public FriendRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FriendRequest?> GetByIdAsync(Guid id)
    {
        return await _context.FriendRequests
            .Include(fr => fr.Requester)
            .Include(fr => fr.Receiver)
            .FirstOrDefaultAsync(fr => fr.Id == id);
    }

    public async Task<FriendRequest?> GetPendingRequestAsync(Guid requesterId, Guid receiverId)
    {
        return await _context.FriendRequests
            .Include(fr => fr.Requester)
            .Include(fr => fr.Receiver)
            .FirstOrDefaultAsync(fr => 
                fr.RequesterId == requesterId && 
                fr.ReceiverId == receiverId && 
                fr.Status == FriendRequestStatus.Pending);
    }

    public async Task<IEnumerable<FriendRequest>> GetPendingRequestsForReceiverAsync(Guid receiverId)
    {
        return await _context.FriendRequests
            .Include(fr => fr.Requester)
            .Include(fr => fr.Receiver)
            .Where(fr => fr.ReceiverId == receiverId && fr.Status == FriendRequestStatus.Pending)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FriendRequest>> GetPendingRequestsForRequesterAsync(Guid requesterId)
    {
        return await _context.FriendRequests
            .Include(fr => fr.Requester)
            .Include(fr => fr.Receiver)
            .Where(fr => fr.RequesterId == requesterId && fr.Status == FriendRequestStatus.Pending)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(FriendRequest request)
    {
        await _context.FriendRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FriendRequest request)
    {
        _context.FriendRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var request = await _context.FriendRequests.FindAsync(id);
        if (request != null)
        {
            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();
        }
    }
}

