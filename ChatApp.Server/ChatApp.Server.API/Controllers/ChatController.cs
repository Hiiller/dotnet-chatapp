//ChatApp.Server/ChatApp.Server.API/Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.DTOs;
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

        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto sendMessageDto)
        {
            if (sendMessageDto == null)
            {
                return BadRequest("Invalid data.");
            }

            await _chatService.SendMessageAsync(sendMessageDto.Chatroom, sendMessageDto.Username, sendMessageDto.Message);
            return Ok("Message sent successfully.");
        }
    }
}
