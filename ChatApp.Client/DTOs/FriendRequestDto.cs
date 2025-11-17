using System;

namespace ChatApp.Client.DTOs;

public class FriendRequestDto
{
    public Guid Id { get; set; }
    public Guid RequesterId { get; set; }
    public string RequesterUsername { get; set; } = string.Empty;
    public string RequesterDisplayName { get; set; } = string.Empty;
    public Guid ReceiverId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendFriendRequestDto
{
    public Guid RequesterId { get; set; }
    public string ReceiverUsername { get; set; } = string.Empty;
}

public class RespondToFriendRequestDto
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public bool Accept { get; set; }
}

public class SearchUserResultDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
}

