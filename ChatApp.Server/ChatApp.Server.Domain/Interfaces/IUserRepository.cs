// ChatApp.Server.Domain/Interfaces/IUserRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;

namespace ChatApp.Server.Domain.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// 根据用户 ID 获取用户，如果未找到返回 null。
        /// </summary>
        Task<User?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// 根据用户名获取用户，如果未找到返回 null。
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);
        
        /// <summary>
        /// 根据电子邮件地址获取用户，如果未找到返回 null。
        /// </summary>
        Task<User?> GetByEmailAsync(string email);
        
        /// <summary>
        /// 获取所有用户的列表，如果没有用户返回空集合。
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync();
        
        /// <summary>
        /// 添加一个新的用户。
        /// </summary>
        Task AddAsync(User user);
        
        /// <summary>
        /// 更新用户信息。如果用户不存在，操作无效。
        /// </summary>
        void Update(User user);
        
        /// <summary>
        /// 删除用户。如果用户不存在，操作无效。
        /// </summary>
        void Remove(User user);
        
        /// <summary>
        /// 保存所有对数据库的更改。如果保存失败，返回 false。
        /// </summary>
        Task<bool> SaveChangesAsync();
    }
}
