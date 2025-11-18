using System.Text.Json.Serialization;

namespace ChatApp.Client.DTOs;

public class SecurityAnswersDto
{
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("animal")]
    public string Animal { get; set; } = string.Empty;

    [JsonPropertyName("parentName")]
    public string ParentName { get; set; } = string.Empty;
}

public class RecoverPasswordRequestDto : SecurityAnswersDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
}
