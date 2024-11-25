//ChatApp.Server.Application/Services/ChatService.cs    
using ChatApp.Server.Application.Interfaces;
using System.Threading.Tasks;
using ChatApp.Server.Application.DTOs;
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
        
        public async Task SendMessageAsync(Guid senderId, Guid receiverId, string messageContent)
        {
            var message = new Message(senderId, messageContent, receiverId);
            await _messageRepository.AddAsync(message);
        }
        
        public async Task<IEnumerable<MessageDto>> GetPrivateMessagesAsync(Guid user1Id, Guid user2Id)
        {
            var messages = await _messageRepository.GetMessagesBetweenUsersAsync(user1Id, user2Id);
            return messages.Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                Timestamp = m.Timestamp
            });
            
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

                result.Add(new PrivateChatDto
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
