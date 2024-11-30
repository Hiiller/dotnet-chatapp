using System;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;
using Shared.Models;

namespace ChatApp.Server.Application.Interfaces
{
    public interface IUserService
    {
        //用户注册
        Task<User?> RegisterUserAsync(string username,string password);
        //用户登录
        //TODO：修改后续实现加密密码验证
        Task<LoginResponse> LoginUserAsync(string username, string password);
        // 添加好友
        Task<Friend> AddFriendAsync(Guid userId, string friendUsername);

        // 获取好友列表
        Task<IEnumerable<Friend>> GetFriendsAsync(Guid userId);

        // 检查是否是好友
        Task<bool> IsFriendAsync(Guid userId, string friendUsername);
    }
}