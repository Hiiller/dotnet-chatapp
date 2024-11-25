//ChatApp.Client/Services/HubService.cs
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace ChatApp.Client.Services
{
    public interface IHubService
    {
        Task ConnectAsync(Guid userId); // 用户的连接
        Task SendPrivateMessageAsync(Guid receiverId, string messageContent); // 私聊消息发送
        event Action<Guid, string, string> MessageReceived; // 包括发送者 ID、发送者名称和消息内容
    }

    public class HubService : IHubService
    {
        private HubConnection _connection;

        public event Action<Guid, string, string>? MessageReceived;

        public async Task ConnectAsync(Guid userId)
        {
            // 创建 HubConnection 实例并指定 SignalR 服务 URL
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5005/chatHub") // 服务器端 SignalR 的 URL
                .Build();

            // 设置接收私聊消息的事件处理程序
            _connection.On<Guid, string, string>("ReceivePrivateMessage", (senderId, senderName, messageContent) =>
            {
                // 触发 MessageReceived 事件，将消息内容传递给订阅者
                MessageReceived?.Invoke(senderId, senderName, messageContent);
            });

            // 启动连接
            await _connection.StartAsync();

            // 调用服务器端 RegisterUser 方法注册用户
            await _connection.InvokeAsync("RegisterUser", userId);
        }

        // 发送私聊消息给指定接收者
        public async Task SendPrivateMessageAsync(Guid receiverId, string messageContent)
        {
            if (_connection == null || _connection.State != HubConnectionState.Connected)
                throw new InvalidOperationException("The connection to the server is not established.");

            // 调用服务器端的 SendPrivateMessage 方法发送私聊消息
            await _connection.InvokeAsync("SendPrivateMessage", receiverId, messageContent);
        }

        // 断开与服务器的连接
        public async Task DisconnectAsync()
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
            {
                try
                {
                    await _connection.InvokeAsync("UnregisterUser"); // 主动通知服务器
                    await _connection.StopAsync(); // 停止连接
                }
                finally
                {
                    await _connection.DisposeAsync(); // 释放资源
                }
            }
        }


    }
}