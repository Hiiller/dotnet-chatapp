using System;

namespace ChatApp.Client.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string PersonalCode { get; set; } = string.Empty;
        public bool HasAvatar { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? DisplayName { get; set; }
        public string? Username { get; set; }
        public string? Bio { get; set; }
        public string? AvatarBase64 { get; set; }
    }

    public class ChangePasswordDto
    {
        public string? OldPassword { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
