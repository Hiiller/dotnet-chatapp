using System;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;

namespace ChatApp.Server.Application.Interfaces
{
    public interface IUserService
    {
        //用户注册
        Task<User?> RegisterUserAsync(string username, string displayName, string email, string password);
        //用户登录
        //TODO：修改后续实现加密密码验证
        Task<User?> LoginUserAsync(string username, string password);
    }
}