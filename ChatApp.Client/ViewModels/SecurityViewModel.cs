using System;
using System.Reactive;
using ReactiveUI;
using ChatApp.Client.Services;
using ChatApp.Client.DTOs;

namespace ChatApp.Client.ViewModels;

public class SecurityViewModel : ReactiveObject
{
    private readonly IChatService _chatService;
    private readonly Guid _userId;

    public SecurityViewModel()
    {
        _chatService = null!; _userId = Guid.Empty;
        SaveCommand = ReactiveCommand.Create(() => { });
        CancelCommand = ReactiveCommand.Create(() => { });
    }

    public SecurityViewModel(Guid userId, IChatService chatService)
    {
        _userId = userId;
        _chatService = chatService;

        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var ok = await _chatService.ChangePassword(_userId, new ChangePasswordDto { OldPassword = OldPassword, NewPassword = NewPassword });
            CloseRequested?.Invoke(ok);
        });

        CancelCommand = ReactiveCommand.Create(() => CloseRequested?.Invoke(false));
    }

    private string? _oldPassword;
    public string? OldPassword { get => _oldPassword; set => this.RaiseAndSetIfChanged(ref _oldPassword, value); }

    private string _newPassword = string.Empty;
    public string NewPassword { get => _newPassword; set => this.RaiseAndSetIfChanged(ref _newPassword, value); }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public event Action<bool>? CloseRequested;
}
