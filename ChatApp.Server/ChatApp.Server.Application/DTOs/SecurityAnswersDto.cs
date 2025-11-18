namespace ChatApp.Server.Application.DTOs;

public class SecurityAnswersDto
{
    public string City { get; set; } = string.Empty;
    public string Animal { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
}

public class RecoverPasswordRequestDto : SecurityAnswersDto
{
    public string Username { get; set; } = string.Empty;
}
