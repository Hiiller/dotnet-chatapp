using Microsoft.AspNetCore.SignalR.Client;
using Shared.MessageTypes;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using Avalonia.Threading;
using ChatApp.Client.DTOs;

namespace ChatApp.Client.Services
{
    public interface IChatService
    {
        Task<LoginResponse> LoginUser(LoginUserDto loginDto);
        Task<LoginResponse> RegisterUser(RegisterUserDto registerDto);
        Task<RecentContactResponse> GetRecentContacts(Guid userId);
        Task<Friend> AddFriend(AddRequestDto addRequestDto);
        Task<List<Friend>> GetFriend(Guid userId);
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
            var response = await _httpClient.PostAsync("/api/chat/register",content);
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
         * 异步方法，用于获得最近联系人。
         * 通过http访问服务器的 GetRecentContacts 方法，传入用户 ID，返回 RecentContactResponse 对象。
         */
        public async Task<RecentContactResponse> GetRecentContacts(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/api/chat/getrecentcontacts/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<RecentContactResponse>(responseStream);
                return result;
            }
            
            return new RecentContactResponse();
        }
        /*
         *
         *
         */

        public async Task<Friend> AddFriend(AddRequestDto addRequestDto)
        {
            var content = new StringContent(JsonSerializer.Serialize(addRequestDto),Encoding.UTF8,"application/json");
            var response = await _httpClient.PostAsync("/api/chat/addfriend", content);
            if (response.IsSuccessStatusCode)
            {   
                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response JSON: {responseString}");
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<Friend>(responseStream);
                Console.WriteLine($"FriendId: {result.friendId}, FriendName: {result.friendName}");
                return result;
            }

            return new Friend();
        }
         
        
        public async Task<List<Friend>> GetFriend(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/api/chat/friends/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<Friend>>(responseStream);
                return result;
            }
            
            return new List<Friend>();
        }
         
        /*
         * 异步方法，用于注销用户，并清空本地存储的消息列表。
         */
        public async Task LogoutAsync()
        {
            await connection.InvokeAsync("Logout");
            
        }


        /*
         * 用于处理登录响应的私有方法。
         * 如果登录成功，返回一个 SuccessfulLoginResponse 对象，该对象包含用户信息和之前的消息。
         * 方法将用户信息保存到 currentUser 属性中，并将之前的消息添加到 Messages 列表中。
         */
        private void ProcessLogInResponse(LoginResponse slr)
        {
            Dispatcher.UIThread.Post(() =>
            {
                currentUserId = slr.currentUserId;
            });
        }
        
        /*
         * 
         */


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
