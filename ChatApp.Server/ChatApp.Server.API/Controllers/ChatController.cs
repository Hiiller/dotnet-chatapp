//ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.DTOs;
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
            var user = await _userService.RegisterUserAsync(registerDto.Username, registerDto.DisplayName, registerDto.Email, registerDto.Password);
            if (user == null)
            {
                return BadRequest("Username or email already exists.");
            }
            return Ok("User registered successfully.");
        }
        
        // 用户登录接口
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserDto loginDto)
        {
            var user = await _userService.LoginUserAsync(loginDto.Username, loginDto.Password);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            return Ok("Login successful.");
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



