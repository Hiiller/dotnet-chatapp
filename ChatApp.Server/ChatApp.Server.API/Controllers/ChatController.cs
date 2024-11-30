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
                    ErrorCode = -1
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
                        ErrorCode = -1
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
                        ErrorCode = -2
                });
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, new LoginResponse
                {
                    connectionStatus = false,
                     // 服务器异常错误码
                    ErrorCode = -3
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

            try
            {
                var friend = await _userService.AddFriendAsync(addRequest.userId, addRequest.friendName);

                if (friend == null)
                {
                    var isFriend = await _userService.IsFriendAsync(addRequest.userId, addRequest.friendName);
                    if (isFriend)
                    {
                        return Conflict(); // 好友已存在
                    }

                    return NotFound(); // 好友用户名不存在
                }

                return Ok(friend); // 添加成功，返回好友对象
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
        
        
        
        //获取最近联系人对话列表
        [HttpGet("recentContacts/{userId}")]
        public async Task<ActionResult<RecentContactResponse>> GetRecentContacts(Guid userId)
        {
            // 验证 userId
            if (userId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid user ID." });
            }

            try
            {
                // 调用服务层获取最近联系人
                var recentContacts = await _chatService.GetRecentContactsAsync(userId);

                // 构造返回结果，即使没有联系人，也返回空的 RecentContactResponse
                var response = new RecentContactResponse
                {
                    UserId = userId,
                    Contacts = recentContacts ?? new Dictionary<Guid, string>() // 确保 NewMsgs 不为 null
                };

                // 始终返回 200 OK
                return Ok(response);
            }
            catch (Exception ex)
            {
                // 捕获异常并返回错误信息
                return StatusCode(500, new { message = "An error occurred while retrieving recent contacts.", error = ex.Message });
            }
        }

        
        /// <summary>
        /// 发送私聊消息。
        /// </summary>
        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
        {
            if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Content))
            {
                return BadRequest("Invalid message data.");
            }

            try
            {
                await _chatService.SendMessageAsync(messageDto);
                return Ok("Message sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while sending the message: {ex.Message}");
            }
        }
    
        /// <summary>
        /// 获取两个用户之间的所有私聊消息历史。
        /// </summary>
        [HttpGet("privateMessages/{user1Id}/{user2Id}")]
        public async Task<IActionResult> GetPrivateMessages(Guid user1Id, Guid user2Id)
        {
            try
            {
                var messages = await _chatService.GetPrivateMessagesAsync(user1Id, user2Id);
                if(messages == null || !messages.Any())
                {
                   var notFoundResponse = new NotFoundResponseDto
                   {
                       Status = "empty",
                       Message = "No messages found."
                   };
                   return NotFound(notFoundResponse);
                }
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching private messages: {ex.Message}");
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



