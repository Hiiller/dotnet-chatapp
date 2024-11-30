using System;
using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.Client.Helpers; // RelayCommand
using Avalonia.Threading; // Avalonia UI 线程
namespace ChatApp.Client.Models;

public class UserModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
//public string DisplayName { get; set; } = string.Empty;
    public ICommand ButtonCommand { get; set; } = new RelayCommand(_ => { });
    
}
