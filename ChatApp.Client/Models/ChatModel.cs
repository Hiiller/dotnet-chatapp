using System;
using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApp.Client.Helpers; // RelayCommand
using Avalonia.Threading; // Avalonia UI 线程

namespace ChatApp.Client.Models;

public class ChatModel
{
    public Guid UserId { get; set; } // 对方用户ID
    public string ChatName { get; set; }  // 对方用户名
    public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
}
