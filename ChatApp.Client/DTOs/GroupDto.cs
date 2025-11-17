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
    }
}