//ChatApp.Server.Application/Services/ChatService.cs    
using ChatApp.Server.Application.Interfaces;
using System.Threading.Tasks;
using ChatApp.Server.Application.DTOs;
using ChatApp.Server.Application.Mappers;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Domain.Repositories.Interfaces;

namespace ChatApp.Server.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        
        public ChatService(IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }
        
        public async Task<MessageDto> SaveOnlineMessageAsync(MessageDto messageDto)
        {
            var message = MessageMapper.ToEntity(messageDto);
            message.MarkAsRead();
            await _messageRepository.AddAsync(message);
            var messageResponse = MessageMapper.ToDto(message);
            return messageResponse;
        }
        
        public async Task<MessageDto> SaveOfflineMessageAsync(MessageDto messageDto)
        {
            var message = MessageMapper.ToEntity(messageDto);
            await _messageRepository.AddAsync(message);
            var messageResponse = MessageMapper.ToDto(message);
            return messageResponse;
        }
        
        
        public async Task SetMessagetoUnread(MessageDto messageDto)
        {
            var message = await _messageRepository.GetByIdAsync(messageDto.id.Value);
            message.MarkAsUnread();
            await _messageRepository.UpdateAsync(message);
        }
        public async Task<Dictionary<Guid, string>> GetRecentContactsAsync(Guid userId)
        {
            // 调用仓储层获取最近联系人
            var recentContacts = await _messageRepository.GetRecentContactsByUserIdAsync(userId);

            // 如果没有联系人，返回空字典
            if (recentContacts == null || !recentContacts.Any())
            {
                return new Dictionary<Guid, string>();
            }
            // 直接返回仓储层返回的结果
            return recentContacts;
        }
        
        public async Task<List<MessageDto>> GetUnreadMessagesAsync(Guid userId)
        {
            var messages = await _messageRepository.GetUnreadMessagesByUserIdAsync(userId);
            return messages.Select(m => MessageMapper.ToDto(m)).ToList();
        }
        
        public async Task<List<MessageDto>> GetReadMessagesAsync(Guid userId)
        {
            var messages = await _messageRepository.GetReadMessagesByUserIdAsync(userId);
            return messages.Select(m => MessageMapper.ToDto(m)).ToList();
        }
        
        public async Task<IEnumerable<MessageDto>> GetPrivateMessagesAsync(Guid user1Id, Guid user2Id)
        {   
            var messages = await _messageRepository.GetMessagesBetweenUsersAsync(user1Id, user2Id);
            return messages.Select(m => MessageMapper.ToDto(m));
        }
        
        public async Task MarkMessagesAsReadAsync(Guid receiverId, Guid senderId)
        {
            // 标记消息为已读
            await _messageRepository.MarkMessagesAsReadAsync(receiverId, senderId);
        }


        public async Task<IEnumerable<PrivateChatDto>> GetRecentChatsAsync(Guid userId)
        {
            var recentMessages = await _messageRepository.GetRecentMessagesByUserIdAsync(userId);
            var result = new List<PrivateChatDto>();

            foreach (var message in recentMessages)
            {
                if (message == null)
                {
                    continue; // 跳过 null 消息
                }

                var otherUserId = message.SenderId == userId ? message.ReceiverId : message.SenderId;

                if (otherUserId == null)
                {
                    continue; // 跳过没有对应用户的消息
                }

                var otherUser = await _userRepository.GetByIdAsync(otherUserId.Value);

                if (otherUser == null)
                {
                    continue; // 跳过无法找到用户的情况
                }

                result.Add(
                    new PrivateChatDto
                {
                    UserId = otherUser.Id,
                    DisplayName = otherUser.DisplayName,
                    LastMessageContent = message.Content,
                    LastMessageTimestamp = message.Timestamp
                });
            }

            return result;
        }
        
    }
}
