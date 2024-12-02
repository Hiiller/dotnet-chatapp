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
        Task<Friend> AddFriend(AddRequestDto addRequestDto);
        Task<List<Friend>> GetFriend(Guid userId);
        Task<List<MessageDto>> GetPrivateMessages(Guid oppo_id , Guid user_id);
        Task<MessageDto> PostMessageToDb(MessageDto message);
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
            var responseStream = await response.Content.ReadAsStreamAsync();
            // Console.WriteLine("login responseStream" + responseStream);
            var result = await JsonSerializer.DeserializeAsync<LoginResponse>(responseStream);
            ProcessLogInResponse(result);
            // Console.WriteLine("Login for user: " + result.currentUsername);
            return result;
           
            
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
        
       
        public async Task<Friend> AddFriend(AddRequestDto addRequestDto)
        {
            var content = new StringContent(JsonSerializer.Serialize(addRequestDto),Encoding.UTF8,"application/json");
            var response = await _httpClient.PostAsync("/api/chat/addfriend", content);
            Console.WriteLine(response);
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                Console.WriteLine("add friend responseStream" + responseStream);
                var result = await JsonSerializer.DeserializeAsync<Friend>(responseStream);
                Console.WriteLine("find friend :" + result.friendName);
                return result;
            }
            Console.WriteLine("fail to add friend");
            return new Friend();
        }
         
        
        public async Task<List<Friend>> GetFriend(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/api/chat/friends/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<Friend>>(responseStream);
                Console.WriteLine("get first friend :" + result[0].friendName);
                return result;
            }
            
            return new List<Friend>();
        }

        public async Task<List<MessageDto>> GetPrivateMessages(Guid oppo_id , Guid user_id)
        {
            var response = await _httpClient.GetAsync($"/api/chat/privateMessages/{user_id}/{oppo_id}");
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<MessageDto>>(responseStream);
                return result;
            }

            return new List<MessageDto>();
        }

        // public async Task<MessageDto> PostMessageToDb(MessageDto message)
        // {
        //     var content = new StringContent(JsonSerializer.Serialize(message),Encoding.UTF8,"application/json");
        //     var response = await _httpClient.PostAsync("/api/chat/messages",content);
        //     if (response.IsSuccessStatusCode)
        //     {
        //         var responseStream = await response.Content.ReadAsStreamAsync();
        //         var result = await JsonSerializer.DeserializeAsync<MessageDto>(responseStream);
        //         return result;
        //     }
        //
        //     return new MessageDto();
        // }
        
        public async Task<MessageDto> PostMessageToDb(MessageDto message)
        {
            try
            {
                // 序列化消息并打印
                var serializedMessage = JsonSerializer.Serialize(message);
                Console.WriteLine($"Serialized Message: {serializedMessage}");

                var content = new StringContent(serializedMessage, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/chat/messages", content);

                // 检查响应状态
                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Body: {responseBody}");

                    // 解析响应
                    var result = JsonSerializer.Deserialize<MessageDto>(responseBody);
                    return result;
                }
                else
                {
                    Console.WriteLine($"Error Response Body: {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
            }

            return new MessageDto();
        }

        
         
        /*
         * 异步方法，用于注销用户，并清空本地存储的消息列表。
         */
        public async Task LogoutAsync()
        {
            await connection.InvokeAsync("Logout");
            
        }

        public async Task<List<MessageDto>> GetRecentMessages(Guid userId)
        {
            var response = await _httpClient.GetAsync($"api/chat/recent/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<MessageDto>>(responseStream);
                return result;
            }
            else
            {
                return new List<MessageDto>();  // 处理错误
            }
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
