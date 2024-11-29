//ChatApp.Server.API/Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.DTOs;
using ChatApp.Server.API.Controllers;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ChatApp.Server.Application.Mappers;

namespace ChatApp.Server.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        // 用于存储在线用户的 ConnectionId
        private static ConcurrentDictionary<Guid, string> _onlineUsers = new();

        public ChatHub(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
        }

        
        /// <summary>
        /// 注册连接到SignalR的用户
        /// </summary>
        /// <param name="userId">用户 ID</param>
        public void RegisterUser(Guid userId)
        {
            if (!_onlineUsers.ContainsKey(userId))
            {
                _onlineUsers[userId] = Context.ConnectionId;
            }
        }

        /// <summary>
        /// 被动注销到SignalR的用户
        /// </summary>
        /// <param name="userId">用户 ID</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // 根据连接 ID 找到对应的用户 ID
            var userId = _onlineUsers.FirstOrDefault(pair => pair.Value == Context.ConnectionId).Key;

            if (userId != Guid.Empty)
            {
                // 从在线用户列表中安全移除用户
                _onlineUsers.TryRemove(userId, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 通知服务器断开连接
        /// </summary>
        /// <returns></returns>
        public void UnregisterUser()
        {
            var userId = _onlineUsers.FirstOrDefault(pair => pair.Value == Context.ConnectionId).Key;
            if (userId != Guid.Empty)
            {
                _onlineUsers.TryRemove(userId, out _);
            }
        }

        /// <summary>
        /// 从在线用户列表中获取连接 ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private string? GetConnectionId(Guid userId)
        {
            // 从在线用户列表中获取连接 ID
            if (_onlineUsers.TryGetValue(userId, out var connectionId))
            {
                return connectionId;
            }
            return null; // 如果用户不在线，则返回 null
        }

        
        
        /// <summary>
        /// 发送私聊消息
        /// </summary>
        /// <param name="messageDto">消息 DTO</param>
        public async Task SendMessage(MessageDto messageDto)
        {
            if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Content))
            {
                throw new HubException("Invalid message data.");
            }

            // 校验消息是否有效

            if (messageDto.GroupId.HasValue)
            {
                // 群聊消息
                await _chatService.SendMessageAsync(messageDto);
                await Clients.Group(messageDto.GroupId.Value.ToString()).SendAsync("ReceiveGroupMessage", messageDto);
            }
            else if (messageDto.ReceiverId.HasValue)
            {
                // 私聊消息
                await _chatService.SendMessageAsync(messageDto);
                var receiverConnectionId = GetConnectionId(messageDto.ReceiverId.Value);
                if (receiverConnectionId != null)
                {
                    await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", messageDto);
                }
            }
        }

        /// <summary>
        /// 加入群组
        /// </summary>
        /// <param name="groupId">群组 ID</param>
        public async Task JoinGroup(Guid groupId)
        {
            var groupName = groupId.ToString();

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // 通知群组其他成员
            await Clients.Group(groupName).SendAsync("GroupNotification", $"{Context.ConnectionId} has joined the group.");
        }

        /// <summary>
        /// 离开群组
        /// </summary>
        /// <param name="groupId">群组 ID</param>
        public async Task LeaveGroup(Guid groupId)
        {
            var groupName = groupId.ToString();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            // 通知群组其他成员
            await Clients.Group(groupName).SendAsync("GroupNotification", $"{Context.ConnectionId} has left the group.");
        }

        /// <summary>
        /// 发送群组消息
        /// </summary>
        /// <param name="groupId">群组 ID</param>
        /// <param name="messageDto">消息 DTO</param>
        public async Task SendGroupMessage(Guid groupId, MessageDto messageDto)
        {
            if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Content))
            {
                throw new HubException("Invalid message data.");
            }

            var groupName = groupId.ToString();

            // 调用应用服务保存群组消息
            await _chatService.SendMessageAsync(messageDto);

            // 广播消息给群组
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", messageDto);
        }

    }
}
