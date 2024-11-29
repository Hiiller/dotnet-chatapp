using System;
using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApp.Client.Helpers; // RelayCommand
using Avalonia.Threading; // Avalonia UI 线程
namespace ChatApp.Client.Models;

public class UserModel
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
}
