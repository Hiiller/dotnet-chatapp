using ReactiveUI;
using System;
using System.Windows.Input;
using ChatApp.Client.Helpers;

namespace ChatApp.Client.Models;

public class UserModel : ReactiveObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }
    private string _username = string.Empty;

    public string AvatarInitials
    {
        get => _avatarInitials;
        set => this.RaiseAndSetIfChanged(ref _avatarInitials, value);
    }
    private string _avatarInitials = string.Empty;

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    private string _statusMessage = "Available";

    public string LastMessagePreview
    {
        get => _lastMessagePreview;
        set => this.RaiseAndSetIfChanged(ref _lastMessagePreview, value);
    }
    private string _lastMessagePreview = string.Empty;

    public bool IsOnline
    {
        get => _isOnline;
        set => this.RaiseAndSetIfChanged(ref _isOnline, value);
    }
    private bool _isOnline = true;

    public ICommand ButtonCommand { get; set; } = new RelayCommand(_ => { });

    public ICommand ProfileCommand
    {
        get => _profileCommand;
        set => this.RaiseAndSetIfChanged(ref _profileCommand, value);
    }
    private ICommand _profileCommand = new RelayCommand(_ => { });

    private string _backgroundColor = "#0078D7";
    public string BackgroundColor
    {
        get => _backgroundColor;
        set => this.RaiseAndSetIfChanged(ref _backgroundColor, value);
    }
}