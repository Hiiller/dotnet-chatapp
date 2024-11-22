//ChatApp.Client/Services/HubService.cs
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace ChatApp.Client.Services
{
    public interface IHubService
    {
        Task ConnectAsync(string username, string chatroom);
        Task SendMessageAsync(string chatroom, string username, string message);
        event Action<string, string> MessageReceived;
    }

    public class HubService : IHubService
    {
        private HubConnection _connection;

        public event Action<string, string>? MessageReceived;

        public async Task ConnectAsync(string username, string chatroom)
        {
            // 创建 HubConnection 实例并指定 SignalR 服务 URL
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5005/chatHub")  // 注意此 URL 为正确的地址
                .Build();

            // 设置接收消息的事件处理程序
            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                // 触发 MessageReceived 事件并传递接收到的消息
                MessageReceived?.Invoke(user, message);
            });

            // 启动连接
            await _connection.StartAsync();

            // 加入指定的聊天室
            await _connection.InvokeAsync("JoinRoom", username, chatroom);
        }

        // 发送消息到指定聊天室
        public async Task SendMessageAsync(string chatroom, string username, string message)
        {
            // 调用 SignalR 服务的 SendMessage 方法发送消息
            await _connection.InvokeAsync("SendMessage", chatroom, username, message);
        }

        // 断开与 SignalR 服务的连接并退出聊天室
        public async Task DisconnectAsync(string username, string chatroom)
        {
            // 调用 SignalR 服务的 LeaveRoom 方法退出聊天室
            await _connection.InvokeAsync("LeaveRoom", username, chatroom);

            // 停止连接
            await _connection.StopAsync();
        }
    }
}