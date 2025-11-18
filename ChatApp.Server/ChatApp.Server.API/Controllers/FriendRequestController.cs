using Microsoft.AspNetCore.Mvc;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.DTOs;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace ChatApp.Server.API.Controllers
{
    [ApiController]
    [Route("api/friendrequests")]
    public class FriendRequestController : ControllerBase
    {
        private readonly IUserService _userService;

        public FriendRequestController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: /api/friendrequests/send
        [HttpPost("send")]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDto dto)
        {
            var request = await _userService.SendFriendRequestAsync(dto.RequesterId, dto.ReceiverUsername);
            if (request == null)
            {
                return BadRequest(new { message = "无法发送好友请求。用户不存在、已是好友或已有待处理的请求。" });
            }
            return Ok(request);
        }

        // GET: /api/friendrequests/pending/{userId}
        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetPendingFriendRequests(Guid userId)
        {
            var requests = await _userService.GetPendingFriendRequestsAsync(userId);
            return Ok(requests);
        }

        // POST: /api/friendrequests/respond
        [HttpPost("respond")]
        public async Task<IActionResult> RespondToFriendRequest([FromBody] RespondToFriendRequestDto dto)
        {
            var success = await _userService.RespondToFriendRequestAsync(dto.RequestId, dto.UserId, dto.Accept);
            if (!success)
            {
                return BadRequest(new { message = "无法处理好友请求。" });
            }
            return Ok(new { message = dto.Accept ? "好友请求已接受" : "好友请求已拒绝" });
        }

        // GET: /api/friendrequests/search?term={searchTerm}
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string term)
        {
            var users = await _userService.SearchUsersByDisplayNameAsync(term);
            var results = users.Select(u => new
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Bio = u.Bio,
                PersonalCode = u.PersonalCode
            });
            return Ok(results);
        }
    }
}

