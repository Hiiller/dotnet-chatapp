using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatApp.Client.DTOs;
using ChatApp.Client.Models;

namespace ChatApp.Client.Services
{
    public interface IChatService
    {
        Task<bool> RegisterUserAsync(string username, string displayName, string email, string password);
        Task<bool> LoginUserAsync(string username, string password);
        Task<bool> SendMessageAsync(Guid senderId, Guid receiverId, string messageContent);
        Task<PrivateChatDto[]> GetRecentChatsAsync(Guid userId);
        Task<MessageDto[]> GetPrivateMessagesAsync(Guid user1Id, Guid user2Id);
    }

    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        
        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 注册用户
        public async Task<bool> RegisterUserAsync(string username, string displayName, string email, string password)
        {
            var registerDto = new RegisterUserDto
            {
                Username = username,
                DisplayName = displayName,
                Email = email,
                Password = password
            };
            
            var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/chat/register", content);
            return response.IsSuccessStatusCode;
        }

        // 登录用户
        public async Task<bool> LoginUserAsync(string username, string password)
        {
            var loginDto = new LoginUserDto
            {
                Username = username,
                Password = password
            };
            
            var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/chat/login", content);
            return response.IsSuccessStatusCode;
        }

        // 发送私聊消息
        public async Task<bool> SendMessageAsync(Guid senderId, Guid receiverId, string messageContent)
        {
            var messageDto = new MessageDto
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = messageContent
            };

            var content = new StringContent(JsonSerializer.Serialize(messageDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/chat/sendMessage", content);
            return response.IsSuccessStatusCode;
        }

        // 获取最近对话
        public async Task<PrivateChatDto[]> GetRecentChatsAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/api/chat/recentChats/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PrivateChatDto[]>(data);
            }
            return Array.Empty<PrivateChatDto>();
        }

        // 获取私聊消息
        public async Task<MessageDto[]> GetPrivateMessagesAsync(Guid user1Id, Guid user2Id)
        {
            var response = await _httpClient.GetAsync($"/api/chat/privateMessages/{user1Id}/{user2Id}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MessageDto[]>(data);
            }
            return Array.Empty<MessageDto>();
        }
    }
}
