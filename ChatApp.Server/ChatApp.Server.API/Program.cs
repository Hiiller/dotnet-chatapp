using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Server.API.Hubs;  // ȷ�������� Hubs �����ռ�
var builder = WebApplication.CreateBuilder(args);

// ��� SignalR ����
builder.Services.AddSignalR();

// ע�� IChatService �� ChatService
builder.Services.AddScoped<IChatService, ChatService>();
// ע�����������
builder.Services.AddControllers();  // ������д���
var app = builder.Build();

// ���� SignalR ·��
app.MapHub<ChatHub>("/chatHub");

app.MapControllers();  // ȷ�� API ������·������ȷӳ��

app.Run();
