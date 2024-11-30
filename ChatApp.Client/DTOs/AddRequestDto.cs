using System;

namespace ChatApp.Client.DTOs;

public class AddRequestDto
{
    public Guid userId { get; set; }
    public string friendName { get; set; }
}