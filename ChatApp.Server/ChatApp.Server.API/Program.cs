using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.Services;
using ChatApp.Server.API.Hubs;
using ChatApp.Server.Infrastructure.Data;
using ChatApp.Server.Domain.Repositories.Interfaces;
using ChatApp.Server.Infrastructure.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 添加日志
builder.Logging.AddConsole();

// 添加服务
builder.Services.AddControllers(); // 注册控制器服务

// 注册应用服务
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IUserService, UserService>();

// 注册仓储层
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 配置数据库上下文
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=chatapp.db")); // 使用 SQLite 数据库

// 添加 SignalR 服务
builder.Services.AddSignalR();

var app = builder.Build();

// 配置中间件
app.UseRouting();

// 映射控制器路由
app.MapControllers();

// 映射 SignalR Hub 路由
app.MapHub<ChatHub>("/chatHub");

// 打印路由信息
app.UseEndpoints(endpoints =>
{
    foreach (var endpoint in endpoints.DataSources.SelectMany(ds => ds.Endpoints))
    {
        Console.WriteLine($"Mapped Endpoint: {endpoint.DisplayName}");
    }
});

app.Run();