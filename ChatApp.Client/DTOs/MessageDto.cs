﻿using System;
using ChatApp.Client.Models;

namespace ChatApp.Client.DTOs;

public class MessageDto
{
    public Guid? id { get; set; }
    public Guid senderId { get; set; }
    public Guid receiverId { get; set; }
    public Guid? groupId{ get; set; }
    public string content { get; set; }
    public DateTime timestamp { get; set; }

    //public int Role { get; set; } = 1;

    public bool IsRead { get; set; }
    public ChatRoleType ChatRoleType { get; set; } 

}