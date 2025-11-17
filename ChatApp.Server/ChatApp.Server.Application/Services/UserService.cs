using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.DTOs;
using ChatApp.Server.Domain.Repositories.Interfaces;
using ChatApp.Server.Infrastructure.Repositories.Implementations;
using ChatApp.Server.Domain.ValueObjects;
using ChatApp.Server.Infrastructure.Data;
using Shared.Models;


namespace ChatApp.Server.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly AppDbContext _context;
        
        // 构造函数，通过依赖注入获取 IUserRepository 实例
        public UserService(IUserRepository userRepository, IFriendRequestRepository friendRequestRepository, AppDbContext context)
        {
            _userRepository = userRepository;
            _friendRequestRepository = friendRequestRepository;
            _context = context;
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
                response.errorCode = -2;
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
                response.errorCode = -2;
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
            // 不允许添加自己为好友
            if (friend.Id == user.Id || string.Equals(friend.Username, user.Username, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (await _userRepository.IsFriendAsync(userId, friendUsername))
            {
                return null; // 已经是好友
            }

            await _userRepository.AddFriendAsync(userId, friendUsername);

            return new Friend
            {
                friendId = friend.Id,
                friendName = friend.Username
            };
        }
        public async Task<IEnumerable<Friend>> GetFriendsAsync(Guid userId)
        {
            var friends = await _userRepository.GetFriendsAsync(userId);

            return friends.Select(friend => new Friend
            {
                friendId = friend.Id,
                friendName = friend.Username
            });
        }

        public async Task<bool> IsFriendAsync(Guid userId, string friendUsername)
        {
            return await _userRepository.IsFriendAsync(userId, friendUsername);
        }

        public async Task<User?> GetUserAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User?> UpdateProfileAsync(Guid userId, string? username, string? displayName, string? bio, byte[]? avatarBytes)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(username) && !string.Equals(username, user.Username, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _userRepository.GetByUsernameAsync(username);
                if (exists != null && exists.Id != user.Id)
                {
                    return null; // 用户名冲突
                }
                user.UpdateUsername(username);
            }
            if (!string.IsNullOrWhiteSpace(displayName))
                user.UpdateDisplayName(displayName);
            user.UpdateBio(bio);
            if (avatarBytes != null && avatarBytes.Length > 0)
                user.UpdateAvatar(avatarBytes);

            await _userRepository.UpdateAsync(user);
            return user;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string? oldPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;
            // 明文示例：如提供旧密码则校验
            if (!string.IsNullOrEmpty(oldPassword) && user.Password != oldPassword)
            {
                return false;
            }
            user.UpdatePassword(newPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }
        
        public async Task<FriendRequestDto?> SendFriendRequestAsync(Guid requesterId, string receiverUsername)
        {
            var requester = await _userRepository.GetByIdAsync(requesterId);
            if (requester == null) return null;
            
            var receiver = await _userRepository.GetByUsernameAsync(receiverUsername);
            if (receiver == null) return null;
            
            // 不能添加自己为好友
            if (requester.Id == receiver.Id) return null;
            
            // 检查是否已经是好友
            if (await _userRepository.IsFriendAsync(requesterId, receiverUsername)) return null;
            
            // 检查是否已有待处理的请求
            var existingRequest = await _friendRequestRepository.GetPendingRequestAsync(requesterId, receiver.Id);
            if (existingRequest != null) return null;
            
            // 创建新的好友请求
            var request = new FriendRequest(requesterId, receiver.Id);
            await _friendRequestRepository.AddAsync(request);
            
            return new FriendRequestDto
            {
                Id = request.Id,
                RequesterId = requester.Id,
                RequesterUsername = requester.Username,
                RequesterDisplayName = requester.DisplayName,
                ReceiverId = receiver.Id,
                CreatedAt = request.CreatedAt
            };
        }
        
        public async Task<IEnumerable<FriendRequestDto>> GetPendingFriendRequestsAsync(Guid userId)
        {
            var requests = await _friendRequestRepository.GetPendingRequestsForReceiverAsync(userId);
            return requests.Select(r => new FriendRequestDto
            {
                Id = r.Id,
                RequesterId = r.RequesterId,
                RequesterUsername = r.Requester.Username,
                RequesterDisplayName = r.Requester.DisplayName,
                ReceiverId = r.ReceiverId,
                CreatedAt = r.CreatedAt
            });
        }
        
        public async Task<bool> RespondToFriendRequestAsync(Guid requestId, Guid userId, bool accept)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);
            if (request == null || request.ReceiverId != userId || request.Status != FriendRequestStatus.Pending)
            {
                return false;
            }
            
            if (accept)
            {
                request.Accept();
                // 添加好友关系
                await _userRepository.AddFriendAsync(request.RequesterId, request.Receiver.Username);
                await _userRepository.AddFriendAsync(request.ReceiverId, request.Requester.Username);
            }
            else
            {
                request.Reject();
            }
            
            await _friendRequestRepository.UpdateAsync(request);
            return true;
        }
        
        public async Task<IEnumerable<User>> SearchUsersByDisplayNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<User>();
            
            var allUsers = await _userRepository.GetAllAsync();
            return allUsers.Where(u => 
                u.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
    }
}
