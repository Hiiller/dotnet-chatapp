using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Domain.Repositories.Interfaces;
using ChatApp.Server.Domain.ValueObjects;
using ChatApp.Server.Infrastructure.Data;


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
        public async Task<User?> RegisterUserAsync(string username, string displayName, string email, string password)
        {
            // 检查用户是否已经存在（根据用户名或电子邮件）
            var existingUser = await _userRepository.GetAllAsync();
            if (existingUser != null)
            {
                // 如果用户名或电子邮件已存在，返回 null
                return null;
            }

            // 创建新用户（此处未加密密码，实际中应进行加密）
            var user = new User(username, new Email(email), displayName, password);

            // 使用 IUserRepository 将新用户添加到数据库
            await _userRepository.AddAsync(user);
            
            // 返回创建的用户
            return user;
        }
        
        
        // 用户登录方法
        public async Task<User?> LoginUserAsync(string username, string password)
        {
            // 查找用户
            var user = await _userRepository.GetByIdAsync(Guid.NewGuid());
            if (user == null)
            {
                // 如果找不到用户，返回 null
                return null;
            }

            // 简单的明文密码比较
            // 这里只是为了演示，实际中应使用安全的密码哈希算法
            if (user.PasswordHash == password)
            {
                // 如果密码匹配，返回用户
                return user;
            }

            // 密码不匹配，返回 null
            return null;
        }


    }
}