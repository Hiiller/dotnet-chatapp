//ChatApp.Server.Domain.Repositories.Interfaces.IUserRepository.cs
using ChatApp.Server.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Server.Domain.Repositories.Interfaces
{
    public interface IUserRepository
    {
        //根据id获取用户
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        //获取所有用户
        Task<IEnumerable<User>> GetAllAsync();
        //根据用户类获取用户
        Task AddAsync(User user);
        //更新用户
        Task UpdateAsync(User user);
        //删除用户
        Task DeleteAsync(Guid id);
    }
}
