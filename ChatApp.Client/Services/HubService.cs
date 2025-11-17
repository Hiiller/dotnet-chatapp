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
        Task SendPrivateAttachmentMessageAsync(Guid senderId, Guid receiverId, string? attachmentUrl, string? messageContent);
        
        Task SetMessageToUnread(MessageDto message); // 设置消息为未读
        Task SendGroupMessageAsync(Guid senderId,Guid groupId, string messageContent); // 群聊消息发送
        Task SendGroupAttachmentMessageAsync(Guid senderId, Guid groupId, string? attachmentUrl, string? messageContent);
        Task JoinGroupAsync(Guid groupId); // 加入群聊
        Task LeaveGroupAsync(Guid groupId); // 离开群聊
        
        Task DisconnectAsync(); // 断开连接
        event Action<MessageDto>? MessageReceived; // 接收私聊消息的事件
        event Action<MessageDto>? GroupMessageReceived; // 接收群聊消息的事件
    }

    public class HubService(string register) : IHubService
    {
        private HubConnection _connection;
        private string register = register;
        private bool isConnected = false;
        public event Action<MessageDto>? MessageReceived;
        public event Action<MessageDto>? GroupMessageReceived;

        // 连接到 SignalR 服务
        public async Task ConnectAsync(Guid userId)
        {
            // 创建 HubConnection 实例并指定 SignalR 服务 URL
            if (isConnected) return;
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

            // 接收群聊消息的事件处理程序
            _connection.On<MessageDto>("ReceiveGroupMessage", message =>
            {
                Console.WriteLine($"Triggering GroupMessageReceived event. receiver:{register}");
                GroupMessageReceived?.Invoke(message);
            });

            // 启动连接
            await _connection.StartAsync();

            // 注册用户
            await _connection.InvokeAsync("RegisterUser", userId);
            isConnected = true;
            Console.WriteLine("HubService connected successfully.");
        }
        
        // 发送私聊消息给指定接收者
        public async Task SendPrivateMessageAsync(Guid senderId,Guid receiverId, string messageContent)
        {
            if (!isConnected)
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

        // 发送带附件的消息（图片URL），content可为空
        public async Task SendPrivateAttachmentMessageAsync(Guid senderId, Guid receiverId, string? attachmentUrl, string? messageContent)
        {
            if (!isConnected)
                throw new InvalidOperationException("The connection to the server is not established.");
            var messageDto = new MessageDto
            {
                senderId = senderId,
                receiverId = receiverId,
                content = messageContent ?? string.Empty,
                attachmentUrl = attachmentUrl,
                timestamp = DateTime.UtcNow
            };
            await _connection.InvokeAsync("SendMessage", messageDto);
        }
        
        public async Task SetMessageToUnread(MessageDto message)
        {
            if (!isConnected)
                throw new InvalidOperationException("The connection to the server is not established.");
            await _connection.InvokeAsync("SetMessageToUnread", message);
            Console.WriteLine("Set a Message as Unread");
        }
        // 发送群组消息（文本）
        public async Task SendGroupMessageAsync(Guid senderId, Guid groupId, string messageContent)
        {
            if (!isConnected)
                throw new InvalidOperationException("The connection to the server is not established.");

            var messageDto = new MessageDto
            {
                senderId = senderId,
                groupId = groupId,
                content = messageContent,
                timestamp = DateTime.UtcNow
            };
            await _connection.InvokeAsync("SendGroupMessage", messageDto);
        }

        // 发送群组消息（附件）
        public async Task SendGroupAttachmentMessageAsync(Guid senderId, Guid groupId, string? attachmentUrl, string? messageContent)
        {
            if (!isConnected)
                throw new InvalidOperationException("The connection to the server is not established.");

            var messageDto = new MessageDto
            {
                senderId = senderId,
                groupId = groupId,
                content = messageContent ?? string.Empty,
                attachmentUrl = attachmentUrl,
                timestamp = DateTime.UtcNow
            };
            await _connection.InvokeAsync("SendGroupMessage", messageDto);
        }

        public async Task JoinGroupAsync(Guid groupId)
        {
            if (!isConnected)
                throw new InvalidOperationException("The connection to the server is not established.");
            await _connection.InvokeAsync("JoinGroup", groupId);
        }

        public async Task LeaveGroupAsync(Guid groupId)
        {
            if (!isConnected)
                throw new InvalidOperationException("The connection to the server is not established.");
            await _connection.InvokeAsync("LeaveGroup", groupId);
        }

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
                    isConnected = false;
                }
            }
        }
    }
}
