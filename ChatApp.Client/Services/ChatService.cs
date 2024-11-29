using Microsoft.AspNetCore.SignalR.Client;
using Shared.MessageTypes;
using Shared.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using ChatApp.Client.DTOs;

namespace ChatApp.Client.Services
{
    public interface IChatService
    {
        Task<LoginResponse> LoginUser(LoginUserDto loginDto);
        Task<LoginResponse> RegisterUser(RegisterUserDto registerDto);
        Task SendMessageAsync(Guid senderId, Guid receiverId, string message);
        Task<PrivateChatDto[]> GetRecentChatsAsync(Guid userId);
        Task LogoutAsync();
    }
    //业务逻辑层，与 SignalR 服务端进行通信
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        
        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /*
         * 异步方法，用于通过 SignalR 调用服务器的 Login 方法，传入用户名和密码进行登录。
         * 登录后会调用 ProcessLogInResponse 方法处理登录响应。
         */
        public async Task<LoginResponse> LoginUser(LoginUserDto loginDto)
        {
            var content = new StringContent(JsonSerializer.Serialize(loginDto),Encoding.UTF8,"application/json");
            var response = await _httpClient.PostAsync("/api/chat/login",content);
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<LoginResponse>(responseStream);
                ProcessLogInResponse(result);
                return result;
            }
            
            return new LoginResponse();
            
        }

        /*
        * 异步方法，用于注册并登录用户。
        * 先通过 RegisterAndLogIn 调用服务器方法，传入用户名和密码，然后处理登录响应。
        */
        // public async Task<SuccessfulLoginResponse> RegisterAndLogIn(string username, string passcode)
        // {
        //     var result = await connection.InvokeAsync<SuccessfulLoginResponse>("RegisterAndLogIn", username, passcode);
        //     ProcessLogInResponse(result);
        //     return result;
        // }
        public async Task<LoginResponse> RegisterUser(RegisterUserDto registerDto)
        {
            var content = new StringContent(JsonSerializer.Serialize(registerDto),Encoding.UTF8,"application/json");
            var response = await _httpClient.PostAsync("/api/chat/login",content);
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<LoginResponse>(responseStream);
                ProcessLogInResponse(result);
                return result;
            }
            
            return new LoginResponse();
        }

        /*
         * 异步方法，用于将消息发送到 SignalR 服务器。
         * 如果消息对象中没有指定 AuthorUsername，则使用当前用户的用户名。然后将消息通过 InvokeAsync 方法发送到服务器。
         */
        public async Task SendMessageAsync(MessagePayload message)
        {
            if (string.IsNullOrEmpty(message.AuthorUsername))
                message.AuthorUsername = CurrentUser.UserName;

            await connection.InvokeAsync("SendMessage", message);
        }

        /*
         * 异步方法，用于注销用户，并清空本地存储的消息列表。
         */
        public async Task LogoutAsync()
        {
            await connection.InvokeAsync("Logout");
            Messages.Clear();
        }


        /*
         * 用于处理登录响应的私有方法。
         * 如果登录成功，返回一个 SuccessfulLoginResponse 对象，该对象包含用户信息和之前的消息。
         * 方法将用户信息保存到 currentUser 属性中，并将之前的消息添加到 Messages 列表中。
         */
        private void ProcessLogInResponse(LoginResponse slr)
        {
            currentUserId = slr.currentUserId;
        }


        //事件流处理：
        internal Guid CurrentUser => currentUserId;

        //Fields 
        private Guid currentUserId;
        private HubConnection connection;
        //向订阅者发出关于消息接收、用户登录和用户注销的通知：
        private Subject<MessagePayload> newMessageReceivedSubject = new Subject<MessagePayload>();
        private Subject<string> participantLoggedOutSubject = new Subject<string>();
        private Subject<string> participantLoggedInSubject = new Subject<string>();
    }
}
