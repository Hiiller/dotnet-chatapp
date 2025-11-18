using System;
using System.Text.Json.Serialization;

namespace ChatApp.Client.DTOs
{
    public class GroupDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("groupCode")]
        public string GroupCode { get; set; } = string.Empty;
        
        [JsonPropertyName("creatorId")]
        public Guid CreatorId { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("memberCount")]
        public int MemberCount { get; set; }
    }
}