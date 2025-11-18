using System.Text;
using System.Text.Json;
using ChatApp.Server.Application.DTOs;
using ChatApp.Server.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Server.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(Guid userId)
        {
            var user = await _userService.GetUserAsync(userId);
            if (user == null) return NotFound();
            var dto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                PersonalCode = user.PersonalCode,
                HasAvatar = user.Avatar != null && user.Avatar.Length > 0
            };
            return Ok(dto);
        }

        [HttpGet("{userId}/avatar")]
        public async Task<IActionResult> GetAvatar(Guid userId)
        {
            var user = await _userService.GetUserAsync(userId);
            if (user == null || user.Avatar == null || user.Avatar.Length == 0) return NotFound();
            // Assume png/jpeg raw bytes stored; content type best-effort
            return File(user.Avatar, "application/octet-stream");
        }

        [HttpPut("{userId}/profile")]
        public async Task<IActionResult> UpdateProfile(Guid userId, [FromBody] UpdateProfileDto update)
        {
            byte[]? avatarBytes = null;
            if (!string.IsNullOrWhiteSpace(update.AvatarBase64))
            {
                try { avatarBytes = Convert.FromBase64String(update.AvatarBase64); }
                catch { return BadRequest("Invalid avatar base64"); }
            }
            var user = await _userService.UpdateProfileAsync(userId, update.Username, update.DisplayName, update.Bio, avatarBytes);
            if (user == null) return Conflict(new { message = "Username conflict or user not found" });
            var dto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                PersonalCode = user.PersonalCode,
                HasAvatar = user.Avatar != null && user.Avatar.Length > 0
            };
            return Ok(dto);
        }

        [HttpPut("{userId}/password")]
        public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordDto change)
        {
            if (string.IsNullOrWhiteSpace(change.NewPassword)) return BadRequest();
            var ok = await _userService.ChangePasswordAsync(userId, change.OldPassword, change.NewPassword);
            if (!ok) return BadRequest();
            return Ok(new { success = true });
        }

        [HttpGet("{userId}/security-answers")]
        public async Task<IActionResult> GetSecurityAnswers(Guid userId)
        {
            var answers = await _userService.GetSecurityAnswersAsync(userId);
            if (answers == null)
            {
                return NotFound();
            }

            return Ok(answers);
        }

        [HttpPut("{userId}/security-answers")]
        public async Task<IActionResult> UpdateSecurityAnswers(Guid userId, [FromBody] SecurityAnswersDto dto)
        {
            var updated = await _userService.UpdateSecurityAnswersAsync(userId, dto);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("recover-password")]
        public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest("Username is required");
            }

            var password = await _userService.RecoverPasswordWithSecurityAnswersAsync(dto.Username, dto);
            if (password == null)
            {
                return BadRequest(new { message = "验证失败" });
            }

            return Ok(new { password });
        }
    }
}
