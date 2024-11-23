using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Server.API.Hubs;
using ChatApp.Server.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// 添加 SignalR 服务
builder.Services.AddSignalR();

// 注册 IChatService 和 ChatService
builder.Services.AddScoped<IChatService, ChatService>();

// 配置 SQLite 数据库连接
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=chatapp.db"));  // 使用 SQLite 文件作为数据源

// 注册控制器服务
builder.Services.AddControllers();  // 添加这行代码
var app = builder.Build();

// 配置 SignalR 路由
app.MapHub<ChatHub>("/chatHub");

app.MapControllers();  // 确保 API 控制器路由能正确映射

app.Run();
