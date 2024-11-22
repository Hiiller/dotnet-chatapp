//ChatApp.Server.API/Program.cs
// 确保使用正确的 Hubs 命名空间
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Server.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 添加 SignalR 服务
builder.Services.AddSignalR();

// 注册 IChatService 和 ChatService
builder.Services.AddScoped<IChatService, ChatService>();

// 注册控制器服务
builder.Services.AddControllers();

var app = builder.Build();

// 映射 SignalR 路由
app.MapHub<ChatHub>("/chatHub");

// 确保 API 控制器路由正确映射
app.MapControllers();

app.Run();