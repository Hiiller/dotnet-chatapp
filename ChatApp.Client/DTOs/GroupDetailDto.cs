using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChatApp.Client.DTOs;

public class GroupDetailDto : GroupDto
{
    [JsonPropertyName("creatorName")]
    public string CreatorName { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("currentUserRole")]
    public string? CurrentUserRole { get; set; }
    
    [JsonPropertyName("canManageMembers")]
    public bool CanManageMembers { get; set; }
    
    [JsonPropertyName("members")]
    public List<GroupMemberDto> Members { get; set; } = new();
    
    [JsonPropertyName("pendingRequests")]
    public List<GroupJoinRequestDto> PendingRequests { get; set; } = new();
}

public class GroupMemberDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("personalCode")]
    public string PersonalCode { get; set; } = string.Empty;
    
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    [JsonPropertyName("joinedAt")]
    public DateTime JoinedAt { get; set; }
}

public class GroupJoinRequestDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("requesterId")]
    public Guid RequesterId { get; set; }
    
    [JsonPropertyName("requesterUsername")]
    public string RequesterUsername { get; set; } = string.Empty;
    
    [JsonPropertyName("requesterDisplayName")]
    public string RequesterDisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("requesterPersonalCode")]
    public string RequesterPersonalCode { get; set; } = string.Empty;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
