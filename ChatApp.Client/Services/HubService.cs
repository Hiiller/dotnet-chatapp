using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using ChatApp.Client.DTOs;

namespace ChatApp.Client.Services
{
    public interface IHubService
    {
        Task ConnectAsync(Guid userId); // 用户的连接
        Task SendPrivateMessageAsync(Guid senderId,Guid receiverId, string messageContent); // 私聊消息发送
        
        //Task SendGroupMessageAsync(Guid senderId,Guid groupId, string messageContent); // 群聊消息发送
        //Task JoinGroupAsync(Guid groupId); // 加入群聊
        //Task LeaveGroupAsync(Guid groupId); // 离开群聊
        
        Task DisconnectAsync(); // 断开连接
        event Action<MessageDto>? MessageReceived; // 接收私聊消息的事件
    }

    public class HubService(string register) : IHubService
    {
        private HubConnection _connection;
        private string register = register;
        public event Action<MessageDto>? MessageReceived;

        // 连接到 SignalR 服务
        public async Task ConnectAsync(Guid userId)
        {
            // 创建 HubConnection 实例并指定 SignalR 服务 URL
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5005/chatHub") // SignalR 服务端 URL
                .Build();

            //接收私聊消息的事件处理程序
            _connection.On<MessageDto>("ReceiveMessage", message =>
            {
                // 触发 MessageReceived 事件，将 MessageDto 传递给订阅者
                //Console.WriteLine($"ReceiveMessage{message.content}, senderId:{message.senderId}, receiverId:{message.receiverId}");
                Console.WriteLine($"Triggering MessageReceived event. receiver:{register}");
                //到这里都可以接收到信息
                MessageReceived?.Invoke(message);
            });

            // 启动连接
            await _connection.StartAsync();

            // 注册用户
            await _connection.InvokeAsync("RegisterUser", userId);
        }
        
        // 发送私聊消息给指定接收者
        public async Task SendPrivateMessageAsync(Guid senderId,Guid receiverId, string messageContent)
        {
            if (_connection == null || _connection.State != HubConnectionState.Connected)
                throw new InvalidOperationException("The connection to the server is not established.");
            var messageDto = new MessageDto
            {
                senderId = senderId,
                receiverId = receiverId,
                content = messageContent,
                timestamp = DateTime.UtcNow
            };
            // 调用 SignalR 方法发送私聊消息
            await _connection.InvokeAsync("SendMessage", messageDto);
        }
        
        // // 发送群组消息
        // public async Task SendGroupMessageAsync(Guid senderId, Guid groupId, string messageContent)
        // {
        //     if (_connection == null || _connection.State != HubConnectionState.Connected)
        //         throw new InvalidOperationException("The connection to the server is not established.");
        //
        //     // 创建 MessageDto
        //     var messageDto = new MessageDto
        //     {
        //         SenderId = senderId,  // 发送者 ID
        //         GroupId = groupId,    // 群组 ID
        //         Content = messageContent  // 消息内容
        //     };
        //
        //     // 调用 SignalR 方法发送群组消息
        //     await _connection.InvokeAsync("SendGroupMessage", groupId, messageDto);
        // }

        // 断开连接
        // 断开与 SignalR 连接
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
