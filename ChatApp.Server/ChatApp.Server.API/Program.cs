using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Server.API.Hubs;
using ChatApp.Server.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// ��� SignalR ����
builder.Services.AddSignalR();

// ע�� IChatService �� ChatService
builder.Services.AddScoped<IChatService, ChatService>();

// ���� SQLite ���ݿ�����
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=chatapp.db"));  // ʹ�� SQLite �ļ���Ϊ����Դ

// ע�����������
builder.Services.AddControllers();  // ������д���
var app = builder.Build();

// ���� SignalR ·��
app.MapHub<ChatHub>("/chatHub");

app.MapControllers();  // ȷ�� API ������·������ȷӳ��

app.Run();
