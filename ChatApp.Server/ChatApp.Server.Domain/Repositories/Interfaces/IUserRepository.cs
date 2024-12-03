//ChatApp.Server.Domain.Repositories.Interfaces.IUserRepository.cs

namespace ChatApp.Server.Domain.Repositories.Interfaces
{
    public interface IUserRepository
    {
        //根据id获取用户
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        //获取所有用户
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);

        Task AddFriendAsync(Guid userId, string friendUsername);

        Task<IEnumerable<User>> GetFriendsAsync(Guid userId);
        Task<bool> IsFriendAsync(Guid userId, string friendUsername);
        //更新用户
        Task UpdateAsync(User user);
        //删除用户
        Task DeleteAsync(Guid id);
    }
}
