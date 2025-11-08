//ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.DTOs;
using Shared.Models;
using System;
using System.Threading.Tasks;

namespace ChatApp.Server.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;

        public ChatController(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
            Console.WriteLine("ChatController initialized");
        }
        
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "Server is running", timestamp = DateTime.UtcNow });
        }
        
        /// <summary>
        /// 注册用户
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto registerDto)
        {
            if(string.IsNullOrWhiteSpace(registerDto.Username) || string.IsNullOrWhiteSpace(registerDto.Password))
            {
                var response =  new LoginResponse
                {
                    connectionStatus = false,
                    errorCode = -1
                };
                return BadRequest(response); // 返回 BadRequest 响应()
            }

            try
            {
                Console.WriteLine("Attempting to register user...");
                var user = await _userService.RegisterUserAsync(registerDto.Username, registerDto.Password);
                if (user == null)
                {
                    Console.WriteLine("User already exists.");
                    var response = new LoginResponse
                    {
                        connectionStatus = false,
                        errorCode = -1
                    };
                    return Conflict(response);
                }
                Console.WriteLine("User registered successfully. Attempting to log in...");
                var loginDto = new LoginUserDto
                {
                    Username = registerDto.Username,
                    Password = registerDto.Password
                };
                var loginResponse = await _userService.LoginUserAsync(loginDto.Username, loginDto.Password);
                if (loginResponse.connectionStatus)
                {
                    Console.WriteLine("Login successful.");
                    return Ok(loginResponse);
                }
                
                return Unauthorized(new LoginResponse
                {
                        connectionStatus = false,
                        errorCode = -2
                });
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, new LoginResponse
                {
                    connectionStatus = false,
                     // 服务器异常错误码
                    errorCode = -3
                });
            }
        }
        
        // 用户登录接口
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserDto loginDto)
        {
            var response = await _userService.LoginUserAsync(loginDto.Username, loginDto.Password);
            if (!response.connectionStatus)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }
        
        /// <summary>
        /// 添加好友
        /// </summary>
        [HttpPost("addfriend")]
        public async Task<IActionResult> AddFriend([FromBody] AddRequestDto addRequest)
        {
            if (addRequest == null || string.IsNullOrWhiteSpace(addRequest.friendName) || addRequest.userId == Guid.Empty)
            {
                return BadRequest(); // 请求数据无效
            }
            // 不允许添加自己为好友（用户名相同）
            // 由于这里只拿到 userId 和 friendName，交由服务层最终判定；这里做一个快速阻断

            try
            {
                var response = await _userService.AddFriendAsync(addRequest.userId, addRequest.friendName);

                if (response == null)
                {
                    // 自己为好友或已存在都会返回 null；尽量区分：
                    var isFriend = await _userService.IsFriendAsync(addRequest.userId, addRequest.friendName);
                    if (isFriend)
                    {
                        return Conflict(); // 好友已存在
                    }

                    return BadRequest(); // 用户名不存在或尝试添加自己
                }
                Console.WriteLine("find a friend from db : " + response.friendName);
                return Ok(response); // 添加成功，返回好友对象
            }
            catch (Exception)
            {
                return StatusCode(500); // 服务器错误
            }
        }

        /// <summary>
        /// 获取好友列表
        /// </summary>
        [HttpGet("friends/{userId}")]
        public async Task<IActionResult> GetFriends(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(); // 无效的用户 ID
            }

            try
            {
                var friends = await _userService.GetFriendsAsync(userId);

                if (friends == null || !friends.Any())
                {
                    return Ok(new List<Friend>()); // 返回空的好友列表
                }

                return Ok(friends); // 返回好友列表
            }
            catch (Exception)
            {
                return StatusCode(500); // 服务器错误
            }
        }
        
        
        
        /// <summary>
        /// 获取用户的一条未读消息
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>未读消息列表</returns>
        [HttpGet("recent/{userId}")]
        public async Task<ActionResult<List<MessageDto>>> GetUnreadMessages(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            try
            {
                var messages = await _chatService.GetUnreadMessagesAsync(userId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取用户的已读消息
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns>已读消息列表</returns>
        [HttpGet("read/{userId}")]
        public async Task<ActionResult<List<MessageDto>>> GetReadMessages(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            try
            {
                var messages = await _chatService.GetReadMessagesAsync(userId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        
    
        /// <summary>
        /// 获取两个用户之间的所有私聊消息历史。
        /// </summary>
        [HttpGet("privateMessages/{user1Id}/{user2Id}")]
        public async Task<IActionResult> GetPrivateMessages(Guid user1Id, Guid user2Id)
        {
            try
            {   Console.WriteLine("RecieverId: " + user1Id + " SenderId: " + user2Id);
                var messages = await _chatService.GetPrivateMessagesAsync(user1Id, user2Id);
                if(messages == null || !messages.Any())
                {
                   return NotFound("No messages found.");
                }
                // 标记未读消息为已读
                await _chatService.MarkMessagesAsReadAsync(user1Id, user2Id);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching private messages: {ex.Message}");
            }
        }
        
        [HttpPost("messages")]
        public async Task<IActionResult> PostMessages( [FromBody] MessageDto messagesDto )
        {
            if ( messagesDto.senderId == Guid.Empty || 
                 messagesDto.receiverId == Guid.Empty || 
                 string.IsNullOrWhiteSpace(messagesDto.content)
                 )
            {
                return BadRequest("Invalid user ID or empty messageDto.");
            }

            try
            {
                // 调用服务层保存消息
                await _chatService.SaveOfflineMessageAsync(messagesDto);

                return Ok(messagesDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("messageunread")]
        public async Task<IActionResult> SetMessagesUnread([FromBody] MessageDto messagesDto)
        {
            if (messagesDto.id == Guid.Empty ||
                messagesDto.senderId == Guid.Empty ||
                messagesDto.receiverId == Guid.Empty ||
                string.IsNullOrWhiteSpace(messagesDto.content)
               )
            {
                return BadRequest("Invalid user ID or empty messageDto.");
            }

            try
            {
                // 调用服务层保存消息
                await _chatService.SetMessagetoUnread(messagesDto);

                return Ok(messagesDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        [HttpPost("readmessages")]
        public async Task<IActionResult> PostreadMessages([FromBody] MessageDto messagesDto)
        {
            if (messagesDto.senderId == Guid.Empty ||
                messagesDto.receiverId == Guid.Empty ||
                string.IsNullOrWhiteSpace(messagesDto.content)
               )
            {
                return BadRequest("Invalid user ID or empty messageDto.");
            }

            try
            {
                // 调用服务层保存消息
                await _chatService.SaveOnlineMessageAsync(messagesDto);

                return Ok(messagesDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// 返回用户的最近对话列表，每个对话只显示最新一条消息。
        /// </summary>
        [HttpGet("recentChats/{userId}")]
        public async Task<IActionResult> GetRecentChats(Guid userId)
        {
            try
            {
                var recentChats = await _chatService.GetRecentChatsAsync(userId);
                if (recentChats == null || !recentChats.Any())
                {
                    var notFoundResponse = new NotFoundResponseDto
                    {
                        Status = "empty",
                        Message = "No recent chats found."
                    };
                    return Ok(notFoundResponse);
                }
                return Ok(recentChats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching recent chats: {ex.Message}");
            }
        }
    }
}



