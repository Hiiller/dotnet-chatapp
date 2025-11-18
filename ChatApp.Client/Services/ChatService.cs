using Microsoft.AspNetCore.SignalR.Client;
using Shared.MessageTypes;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using Avalonia.Threading;
using ChatApp.Client.DTOs;
using ChatApp.Client.Helpers;
using static ChatApp.Client.Helpers.DebugLogger;

namespace ChatApp.Client.Services
{
    public interface IChatService
    {
        Task<LoginResponse> LoginUser(LoginUserDto loginDto);
        Task<LoginResponse> RegisterUser(RegisterUserDto registerDto);
        Task<Friend> AddFriend(AddRequestDto addRequestDto);
        Task<List<Friend>> GetFriend(Guid userId);
        Task<List<GroupDto>> GetGroups();
        Task<List<GroupDto>> GetGroupsByUser(Guid userId);
        Task<GroupDto?> CreateGroupAsync(string groupName, Guid creatorId);
        Task<GroupDto?> SearchGroupByCodeAsync(string groupCode, Guid? userId = null);
        Task<GroupDetailDto?> GetGroupDetailsAsync(Guid groupId, Guid requesterId);
        Task<GroupJoinRequestDto?> RequestToJoinGroupAsync(Guid groupId, Guid requesterId);
        Task<bool> RespondToGroupRequestAsync(Guid groupId, Guid requestId, Guid approverId, bool accept);
        Task<bool> RemoveGroupMemberAsync(Guid groupId, Guid memberId, Guid requesterId);
        Task<List<MessageDto>> GetPrivateMessages(Guid oppo_id , Guid user_id);
        Task<List<MessageDto>> GetGroupMessages(Guid groupId);
        Task<List<MessageDto>> GetRecentMessages(Guid userId);
        Task<MessageDto> PostMessageToDb(MessageDto message);
        Task<MessageDto> PostreadMessageToDb(MessageDto message);
        Task<MessageDto> SetMessagetoUnread(MessageDto message);
        // Profile
        Task<UserProfileDto?> GetProfile(Guid userId);
        Task<UserProfileDto?> UpdateProfile(Guid userId, UpdateProfileDto update);
        Task<bool> ChangePassword(Guid userId, ChangePasswordDto change);
        Task<byte[]?> GetAvatar(Guid userId);
        // Image upload and retrieval
        Task<string?> UploadImageAsync(string filePath);
        Task<Avalonia.Media.Imaging.Bitmap?> GetImageBitmapAsync(string relativeUrl);
        
        // Friend Requests
        Task<FriendRequestDto?> SendFriendRequestAsync(SendFriendRequestDto dto);
        Task<List<FriendRequestDto>> GetPendingFriendRequestsAsync(Guid userId);
        Task<bool> RespondToFriendRequestAsync(RespondToFriendRequestDto dto);
        Task<List<SearchUserResultDto>> SearchUsersAsync(string searchTerm);
        
        Task LogoutAsync();
    }
    //业务逻辑层，与 SignalR 服务端进行通信
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        // 统一的 JSON 选项，开启属性名大小写不敏感，避免服务端返回 camelCase 时解析失败
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
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

            // Prefer server-provided body even on non-success (401 returns a body in our API)
            LoginResponse? result = null;
            try
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                result = await JsonSerializer.DeserializeAsync<LoginResponse>(responseStream);
            }
            catch { /* ignore deserialization issues */ }

            // Fallback if no body
            if (result == null)
            {
                result = new LoginResponse { connectionStatus = false, errorCode = -2 };
            }

            ProcessLogInResponse(result);
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

        public async Task<MessageDto> SetMessagetoUnread(MessageDto message)
        {
            var content = new StringContent(JsonSerializer.Serialize(message),Encoding.UTF8,"application/json");
            var response = await _httpClient.PostAsync($"/api/chat/messageunread", content);
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<MessageDto>(responseStream);
                return result;
            }

            return new MessageDto();
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

        public async Task<List<GroupDto>> GetGroups()
        {
            var response = await _httpClient.GetAsync($"/api/groups");
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<GroupDto>>(responseStream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new List<GroupDto>();
            }
            return new List<GroupDto>();
        }

        public async Task<List<GroupDto>> GetGroupsByUser(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/api/groups/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<GroupDto>>(responseStream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new List<GroupDto>();
            }
            return new List<GroupDto>();
        }
        
        public async Task<GroupDto?> CreateGroupAsync(string groupName, Guid creatorId)
        {
            try
            {
                var url = "/api/groups";
                var requestBody = new { Name = groupName, CreatorId = creatorId };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _httpClient.PostAsync(url, content);
                
                if (!resp.IsSuccessStatusCode)
                {
                    Log("ChatService", $"CreateGroup failed: {resp.StatusCode}");
                    return null;
                }
                
                var responseContent = await resp.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GroupDto>(responseContent, _jsonOptions);
                return result;
            }
            catch (Exception ex)
            {
                Log("ChatService", $"CreateGroup error: {ex.Message}");
                return null;
            }
        }
        
        public async Task<GroupDto?> SearchGroupByCodeAsync(string groupCode, Guid? userId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(groupCode))
                {
                    return null;
                }
                
                var url = $"/api/groups/search?code={Uri.EscapeDataString(groupCode.Trim())}";
                if (userId.HasValue && userId.Value != Guid.Empty)
                {
                    url += $"&userId={userId.Value}";
                }
                
                var resp = await _httpClient.GetAsync(url);
                
                if (!resp.IsSuccessStatusCode)
                {
                    return null;
                }
                
                var responseContent = await resp.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GroupDto>(responseContent, _jsonOptions);
                return result;
            }
            catch (Exception ex)
            {
                Log("ChatService", $"SearchGroupByCodeAsync error: {ex.Message}");
                return null;
            }
        }
        
        public async Task<GroupDetailDto?> GetGroupDetailsAsync(Guid groupId, Guid requesterId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"/api/groups/{groupId}/details?userId={requesterId}");
                if (!resp.IsSuccessStatusCode)
                {
                    Log("ChatService", $"GetGroupDetailsAsync failed: {resp.StatusCode}");
                    return null;
                }

                var json = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GroupDetailDto>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Log("ChatService", $"GetGroupDetailsAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<GroupJoinRequestDto?> RequestToJoinGroupAsync(Guid groupId, Guid requesterId)
        {
            try
            {
                var payload = new { requesterId };
                var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
                var resp = await _httpClient.PostAsync($"/api/groups/{groupId}/requests", content);

                if (!resp.IsSuccessStatusCode)
                {
                    Log("ChatService", $"RequestToJoinGroupAsync failed: {resp.StatusCode}");
                    return null;
                }

                var json = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GroupJoinRequestDto>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Log("ChatService", $"RequestToJoinGroupAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RespondToGroupRequestAsync(Guid groupId, Guid requestId, Guid approverId, bool accept)
        {
            try
            {
                var payload = new { approverId, accept };
                var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
                var resp = await _httpClient.PostAsync($"/api/groups/{groupId}/requests/{requestId}/respond", content);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log("ChatService", $"RespondToGroupRequestAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveGroupMemberAsync(Guid groupId, Guid memberId, Guid requesterId)
        {
            try
            {
                var resp = await _httpClient.DeleteAsync($"/api/groups/{groupId}/members/{memberId}?requesterId={requesterId}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log("ChatService", $"RemoveGroupMemberAsync error: {ex.Message}");
                return false;
            }
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

        public async Task<List<MessageDto>> GetGroupMessages(Guid groupId)
        {
            var response = await _httpClient.GetAsync($"/api/chat/groupMessages/{groupId}");
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<List<MessageDto>>(responseStream);
                return result ?? new List<MessageDto>();
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

        public async Task<MessageDto> PostreadMessageToDb(MessageDto message)
        {
            try
            {
                // 序列化消息并打印
                var serializedMessage = JsonSerializer.Serialize(message);
                Console.WriteLine($"Serialized Message: {serializedMessage}");

                var content = new StringContent(serializedMessage, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/chat/readmessages", content);

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
                // Only set current user when login succeeded; otherwise clear it
                currentUserId = slr != null && slr.connectionStatus ? slr.currentUserId : Guid.Empty;
            });
        }

        public async Task<UserProfileDto?> GetProfile(Guid userId)
        {
            Log("ChatService", $"GetProfile called for userId: {userId}");
            var url = $"/api/user/{userId}";
            Log("ChatService", $"GET {url}");
            
            var resp = await _httpClient.GetAsync(url);
            Log("ChatService", $"GetProfile response status: {resp.StatusCode}");
            
            if (!resp.IsSuccessStatusCode)
            {
                var errorContent = await resp.Content.ReadAsStringAsync();
                Log("ChatService", $"GetProfile failed! Status: {resp.StatusCode}, Content: {errorContent}");
                return null;
            }
            
            var stream = await resp.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<UserProfileDto>(stream, _jsonOptions);
            
            if (result != null)
            {
                Log("ChatService", "GetProfile deserialized successfully:");
                Log("ChatService", $"  - Username: '{result.Username}'");
                Log("ChatService", $"  - DisplayName: '{result.DisplayName}'");
                Log("ChatService", $"  - Bio: '{result.Bio}'");
            }
            else
            {
                Log("ChatService", "GetProfile deserialized to null!");
            }
            
            return result;
        }

        public async Task<UserProfileDto?> UpdateProfile(Guid userId, UpdateProfileDto update)
        {
            Log("ChatService", $"UpdateProfile called for userId: {userId}");
            Log("ChatService", $"Update data: DisplayName='{update.DisplayName}', Username='{update.Username}', Bio='{update.Bio}'");
            
            var url = $"/api/user/{userId}/profile";
            var json = JsonSerializer.Serialize(update);
            Log("ChatService", $"PUT {url}");
            Log("ChatService", $"Request body: {json}");
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _httpClient.PutAsync(url, content);
            
            Log("ChatService", $"UpdateProfile response status: {resp.StatusCode}");
            
            if (!resp.IsSuccessStatusCode)
            {
                var errorContent = await resp.Content.ReadAsStringAsync();
                Log("ChatService", $"UpdateProfile failed! Status: {resp.StatusCode}, Content: {errorContent}");
                return null;
            }
            
            var responseContent = await resp.Content.ReadAsStringAsync();
            Log("ChatService", $"UpdateProfile response body: {responseContent}");
            
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(responseContent));
            var result = await JsonSerializer.DeserializeAsync<UserProfileDto>(stream, _jsonOptions);
            
            if (result != null)
            {
                Log("ChatService", "UpdateProfile deserialized successfully:");
                Log("ChatService", $"  - Username: '{result.Username}'");
                Log("ChatService", $"  - DisplayName: '{result.DisplayName}'");
                Log("ChatService", $"  - Bio: '{result.Bio}'");
            }
            else
            {
                Log("ChatService", "UpdateProfile deserialized to null!");
            }
            
            return result;
        }

        public async Task<bool> ChangePassword(Guid userId, ChangePasswordDto change)
        {
            var content = new StringContent(JsonSerializer.Serialize(change), Encoding.UTF8, "application/json");
            var resp = await _httpClient.PutAsync($"/api/user/{userId}/password", content);
            return resp.IsSuccessStatusCode;
        }

        public async Task<byte[]?> GetAvatar(Guid userId)
        {
            var resp = await _httpClient.GetAsync($"/api/user/{userId}/avatar");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadAsByteArrayAsync();
        }

        // 暴露服务器基础地址给UI层组合完整URL
        public Uri? BaseAddress => _httpClient.BaseAddress;

        // 下载图片并转换为Avalonia的Bitmap供UI显示
        public async Task<Avalonia.Media.Imaging.Bitmap?> GetImageBitmapAsync(string relativeUrl)
        {
            try
            {
                // 构造绝对地址，确保能正确访问静态文件
                Uri absoluteUri;
                if (Uri.TryCreate(relativeUrl, UriKind.Absolute, out var direct))
                {
                    absoluteUri = direct;
                }
                else if (_httpClient.BaseAddress != null)
                {
                    absoluteUri = new Uri(_httpClient.BaseAddress, relativeUrl);
                }
                else
                {
                    throw new InvalidOperationException("BaseAddress is not set for HttpClient.");
                }

                using var resp = await _httpClient.GetAsync(absoluteUri, HttpCompletionOption.ResponseHeadersRead);
                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"GetImageBitmapAsync HTTP error for '{absoluteUri}': {(int)resp.StatusCode} {resp.ReasonPhrase}");
                    return null;
                }

                var contentType = resp.Content.Headers.ContentType?.MediaType ?? string.Empty;
                if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    // 读取少量文本用于诊断（避免输出大量二进制）
                    var preview = await resp.Content.ReadAsStringAsync();
                    var truncated = preview.Length > 256 ? preview.Substring(0, 256) + "..." : preview;
                    Console.WriteLine($"GetImageBitmapAsync non-image Content-Type='{contentType}' from '{absoluteUri}'. Body preview: {truncated}");
                    return null;
                }

                var bytes = await resp.Content.ReadAsByteArrayAsync();
                if (bytes == null || bytes.Length == 0)
                {
                    Console.WriteLine($"GetImageBitmapAsync empty body for '{absoluteUri}'.");
                    return null;
                }

                try
                {
                    using var ms = new System.IO.MemoryStream(bytes);
                    ms.Position = 0;
                    var bmp = new Avalonia.Media.Imaging.Bitmap(ms);
                    Console.WriteLine($"Downloaded bitmap from '{absoluteUri}' size={bmp.PixelSize.Width}x{bmp.PixelSize.Height}");
                    return bmp;
                }
                catch (Exception imgEx)
                {
                    Console.WriteLine($"Bitmap decode error for '{absoluteUri}': {imgEx.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetImageBitmapAsync failed for '{relativeUrl}': {ex.Message}");
                return null;
            }
        }

        // 上传图片文件，返回相对URL，例如 /uploads/{filename}
        public async Task<string?> UploadImageAsync(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return null;
            using var form = new MultipartFormDataContent();
            await using var fs = System.IO.File.OpenRead(filePath);
            var streamContent = new StreamContent(fs);
            var fileName = System.IO.Path.GetFileName(filePath);
            form.Add(streamContent, "file", fileName);
            var resp = await _httpClient.PostAsync("/api/files", form);
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("url", out var urlProp))
                {
                    return urlProp.GetString();
                }
            }
            catch { }
            return null;
        }
        
        // Friend Requests
        public async Task<FriendRequestDto?> SendFriendRequestAsync(SendFriendRequestDto dto)
        {
            try
            {
                var url = "/api/friendrequests/send";
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _httpClient.PostAsync(url, content);
                
                if (!resp.IsSuccessStatusCode)
                {
                    Log("ChatService", $"SendFriendRequest failed: {resp.StatusCode}");
                    return null;
                }
                
                var responseContent = await resp.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FriendRequestDto>(responseContent, _jsonOptions);
                return result;
            }
            catch (Exception ex)
            {
                Log("ChatService", $"SendFriendRequest error: {ex.Message}");
                return null;
            }
        }
        
        public async Task<List<FriendRequestDto>> GetPendingFriendRequestsAsync(Guid userId)
        {
            try
            {
                var url = $"/api/friendrequests/pending/{userId}";
                var resp = await _httpClient.GetAsync(url);
                
                if (!resp.IsSuccessStatusCode)
                {
                    Log("ChatService", $"GetPendingFriendRequests failed: {resp.StatusCode}");
                    return new List<FriendRequestDto>();
                }
                
                var responseContent = await resp.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<FriendRequestDto>>(responseContent, _jsonOptions);
                return result ?? new List<FriendRequestDto>();
            }
            catch (Exception ex)
            {
                Log("ChatService", $"GetPendingFriendRequests error: {ex.Message}");
                return new List<FriendRequestDto>();
            }
        }
        
        public async Task<bool> RespondToFriendRequestAsync(RespondToFriendRequestDto dto)
        {
            try
            {
                var url = "/api/friendrequests/respond";
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _httpClient.PostAsync(url, content);
                
                if (!resp.IsSuccessStatusCode)
                {
                    Log("ChatService", $"RespondToFriendRequest failed: {resp.StatusCode}");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Log("ChatService", $"RespondToFriendRequest error: {ex.Message}");
                return false;
            }
        }
        
        public async Task<List<SearchUserResultDto>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                var url = $"/api/friendrequests/search?term={Uri.EscapeDataString(searchTerm)}";
                var resp = await _httpClient.GetAsync(url);
                
                if (!resp.IsSuccessStatusCode)
                {
                    Log("ChatService", $"SearchUsers failed: {resp.StatusCode}");
                    return new List<SearchUserResultDto>();
                }
                
                var responseContent = await resp.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<SearchUserResultDto>>(responseContent, _jsonOptions);
                return result ?? new List<SearchUserResultDto>();
            }
            catch (Exception ex)
            {
                Log("ChatService", $"SearchUsers error: {ex.Message}");
                return new List<SearchUserResultDto>();
            }
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
