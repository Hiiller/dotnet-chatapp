namespace ChatApp.Server.Application.DTOs;

public class RegisterUserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string City { get; set; } = string.Empty;
    public string Animal { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
}
