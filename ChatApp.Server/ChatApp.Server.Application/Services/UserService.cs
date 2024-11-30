using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Domain.Repositories.Interfaces;
using ChatApp.Server.Domain.ValueObjects;
using ChatApp.Server.Infrastructure.Data;
using Shared.Models;


namespace ChatApp.Server.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        
        // 构造函数，通过依赖注入获取 IUserRepository 实例
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // 用户注册方法
        public async Task<User?> RegisterUserAsync(string username,  string password)
        {
            // 检查用户是否已经存在（根据用户名或电子邮件）
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
            {
                // 如果用户名或电子邮件已存在，返回 null
                return null;
            }

            // 创建新用户（此处未加密密码，实际中应进行加密）
            var user = new User(username, password);

            // 使用 IUserRepository 将新用户添加到数据库
            await _userRepository.AddAsync(user);
            
            // 返回创建的用户
            return user;
        }
        
        
        // 用户登录方法
        public async Task<LoginResponse> LoginUserAsync(string username, string password)
        {
            var response = new LoginResponse();
            // 查找用户
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                // 用户不存在，错误码设置为 -2
                response.ErrorCode = -2;
                response.connectionStatus = false;  
                return response;
            }

            // 简单的明文密码比较
            // 这里只是为了演示，实际中应使用安全的密码哈希算法
            if (user.Password == password)
            {
                // 如果密码匹配，填充 LoginResponse
                response.currentUserId = user.Id;
                response.currentUsername = user.Username;
                response.connectionStatus = true;
            }
            // 密码不匹配，返回 null
            else
            {
                response.ErrorCode = -2;
                response.connectionStatus = false;
            }

            return response;
        }

        public async Task<Friend?> AddFriendAsync(Guid userId, string friendUsername)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null; // 用户不存在
            }

            var friend = await _userRepository.GetByUsernameAsync(friendUsername);
            if (friend == null)
            {
                return null; // 好友用户不存在
            }
            if (await _userRepository.IsFriendAsync(userId, friendUsername))
            {
                return null; // 已经是好友
            }

            await _userRepository.AddFriendAsync(userId, friendUsername);

            return new Friend
            {
                FriendId = friend.Id,
                FriendName = friend.Username
            };
        }
        public async Task<IEnumerable<Friend>> GetFriendsAsync(Guid userId)
        {
            var friends = await _userRepository.GetFriendsAsync(userId);

            return friends.Select(friend => new Friend
            {
                FriendId = friend.Id,
                FriendName = friend.Username
            });
        }

        public async Task<bool> IsFriendAsync(Guid userId, string friendUsername)
        {
            return await _userRepository.IsFriendAsync(userId, friendUsername);
        }

    }
}