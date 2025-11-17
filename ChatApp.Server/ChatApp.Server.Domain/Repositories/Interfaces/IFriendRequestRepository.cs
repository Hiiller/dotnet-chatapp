using ChatApp.Server.Domain.Entities;

namespace ChatApp.Server.Domain.Repositories.Interfaces;

public interface IFriendRequestRepository
{
    Task<FriendRequest?> GetByIdAsync(Guid id);
    Task<FriendRequest?> GetPendingRequestAsync(Guid requesterId, Guid receiverId);
    Task<IEnumerable<FriendRequest>> GetPendingRequestsForReceiverAsync(Guid receiverId);
    Task<IEnumerable<FriendRequest>> GetPendingRequestsForRequesterAsync(Guid requesterId);
    Task AddAsync(FriendRequest request);
    Task UpdateAsync(FriendRequest request);
    Task DeleteAsync(Guid id);
}

