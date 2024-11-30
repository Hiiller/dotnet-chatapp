using Microsoft.AspNetCore.SignalR.Client;
using Shared.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using ChatApp.Server.Application.DTOs;

namespace ChatApp.Client.Services
{
    public interface IChatService
    {
        Task<LoginResponse> LoginUser(LoginUserDto loginDto);
        Task<LoginResponse> RegisterUser(RegisterUserDto registerDto);
        Task<RecentContactResponse> GetRecentContacts(Guid userId);
    }
    
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
            
            return new LoginResponse{connectionStatus = false, ErrorCode = -2};
            
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
            if (string.IsNullOrWhiteSpace(registerDto.Username) || string.IsNullOrWhiteSpace(registerDto.Password))
            {
                return new LoginResponse
                {
                    connectionStatus = false,
                    ErrorCode = -1 // 错误码：无效的用户名或密码
                };
            }

            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(registerDto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("/api/chat/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    var loginResponse = await JsonSerializer.DeserializeAsync<LoginResponse>(
                        responseStream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return loginResponse ?? new LoginResponse
                    {
                        connectionStatus = false,
                        ErrorCode = -2 // 登录失败错误码
                    };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return new LoginResponse
                    {
                        connectionStatus = false,
                        ErrorCode = -1 // 错误码：注册失败（用户名已存在）
                    };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new LoginResponse
                    {
                        connectionStatus = false,
                        ErrorCode = -2 // 错误码：登录失败
                    };
                }

                return new LoginResponse
                {
                    connectionStatus = false,
                    ErrorCode = -3 // 错误码：其他未知错误
                };
            }
            catch (Exception ex)
            {
                // 捕获客户端与服务器通信异常
                return new LoginResponse
                {
                    connectionStatus = false,
                    ErrorCode = -3 // 错误码：服务器异常
                };
            }
        }

        
        /*
         * 异步方法，用于获得最近联系人。
         * 通过http访问服务器的 GetRecentContacts 方法，传入用户 ID，返回 RecentContactResponse 对象。
         */
        public async Task<RecentContactResponse> GetRecentContacts(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/api/chat/recentContacts/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<RecentContactResponse>(responseStream);
                return result;
            }
            
            return new RecentContactResponse();
        }
        
        private void ProcessLogInResponse(LoginResponse slr)
        {
            if (slr != null && slr.connectionStatus)
            {
                currentUserId = slr.currentUserId; // 保存登录用户的 ID
            }
            else
            {
                currentUserId = Guid.Empty; // 登录失败或无效响应时，重置为默认值
            }
        }
        
       
       
               //事件流处理：
        internal Guid CurrentUser => currentUserId;
       
               //Fields 
        private Guid currentUserId;
              
    }
}
