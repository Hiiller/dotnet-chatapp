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

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
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
                await _chatService.SendMessageAsync(messageDto.SenderId, messageDto.ReceiverId ?? Guid.Empty, messageDto.Content);
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
                return Ok(recentChats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching recent chats: {ex.Message}");
            }
        }
    }
}



