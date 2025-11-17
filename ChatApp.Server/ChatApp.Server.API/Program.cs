using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.Services;
using ChatApp.Server.API.Hubs;
using ChatApp.Server.Infrastructure.Data;
using ChatApp.Server.Domain.Repositories.Interfaces;
using ChatApp.Server.Infrastructure.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using System.IO;

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
builder.Services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();

// 配置数据库上下文
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=chatapp.db")); // 使用 SQLite 新数据库，避免 EnsureCreated 旧库无法迁移问题

// 添加 SignalR 服务
builder.Services.AddSignalR();

var app = builder.Build();

// 配置中间件
app.UseRouting();

// 启用静态文件托管 (wwwroot)
app.UseStaticFiles();

// 映射控制器路由
app.MapControllers();

// 映射 SignalR Hub 路由
app.MapHub<ChatHub>("/chatHub");

// 如需打印路由信息，请在最小主机模型下通过 EndpointDataSource 枚举
foreach (var dataSource in app.Services.GetServices<Microsoft.AspNetCore.Routing.EndpointDataSource>())
{
    foreach (var endpoint in dataSource.Endpoints)
    {
        Console.WriteLine($"Mapped Endpoint: {endpoint.DisplayName}");
    }
}

// 确保 wwwroot/uploads 存在
var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(Path.Combine(wwwroot, "uploads"));

// 应用数据库迁移（首次运行将创建数据库并应用架构变更）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed built-in groups if none exist
    if (!db.Set<ChatApp.Server.Domain.Entities.Group>().Any())
    {
        db.Add(new ChatApp.Server.Domain.Entities.Group("产品讨论组"));
        db.Add(new ChatApp.Server.Domain.Entities.Group("设计灵感库"));
        db.Add(new ChatApp.Server.Domain.Entities.Group("周末出游群"));
        await db.SaveChangesAsync();
        Console.WriteLine("Seeded default groups.");
    }
}

app.Run();