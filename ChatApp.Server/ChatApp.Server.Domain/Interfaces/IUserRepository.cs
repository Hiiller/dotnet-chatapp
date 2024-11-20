// ChatApp.Server.Domain/Interfaces/IUserRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;

namespace ChatApp.Server.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        void Update(User user);
        void Remove(User user);
        Task<bool> SaveChangesAsync();
    }
}
