using System;
using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApp.Client.Helpers; // RelayCommand
using Avalonia.Threading; // Avalonia UI 线程

namespace ChatApp.Client.Models;

public class MessageModel
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string SenderName { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}
