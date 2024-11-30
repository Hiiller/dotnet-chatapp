//ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.DTOs;
using Shared.Models;
using System;
using System.Threading.Tasks;

namespace ChatApp.Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;

        public ChatController(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
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
                var user = await _userService.RegisterUserAsync(registerDto.Username, registerDto.Password);
                if (user == null)
                {
                    var response = new LoginResponse
                    {
                        connectionStatus = false,
                        ErrorCode = -1
                    };
                    return Conflict(response);
                }

                var loginDto = new LoginUserDto
                {
                    Username = registerDto.Username,
                    Password = registerDto.Password
                };
                var loginResponse = await _userService.LoginUserAsync(loginDto.Username, loginDto.Password);
                if (loginResponse.connectionStatus)
                {
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
                    NewMsgs = recentContacts ?? new Dictionary<Guid, string>() // 确保 NewMsgs 不为 null
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



