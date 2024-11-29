using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Server.API.Hubs;
using ChatApp.Server.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Domain.Repositories.Interfaces;
using ChatApp.Server.Infrastructure.Repositories.Implementations;
using ChatApp.Server.Domain.Entities;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 添加 SignalR 服务
builder.Services.AddSignalR();

// 注册 IChatService 和 ChatService
builder.Services.AddScoped<IChatService, ChatService>();
// 注册 IUserService 和 UserService
builder.Services.AddScoped<IUserService, UserService>();
// 注册 Infrastructure 层服务
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// 注册控制器服务
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=chatapp.db")); 

// 注册控制器服务
builder.Services.AddControllers();  // 添加这行代码
var app = builder.Build();

// 配置 SignalR 路由
app.MapHub<ChatHub>("/chatHub");

// 确保 API 控制器路由正确映射
app.MapControllers();

app.Run();