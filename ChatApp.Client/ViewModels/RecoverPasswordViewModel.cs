using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ChatApp.Client.DTOs;
using ChatApp.Client.Services;
using ReactiveUI;

namespace ChatApp.Client.ViewModels;

public class RecoverPasswordViewModel : ViewModelBase
{
    private readonly IChatService _chatService;
    private string _username = string.Empty;
    private string _city = string.Empty;
    private string _animal = string.Empty;
    private string _parent = string.Empty;
    private string _statusMessage = "回答下列安全问题以找回密码";
    private string? _recoveredPassword;
    private bool _isBusy;

    public RecoverPasswordViewModel(RoutingState router, string serverUrl)
        : base(router)
    {
        _chatService = new ChatService(new HttpClient { BaseAddress = new Uri(serverUrl) });
        SubmitCommand = ReactiveCommand.CreateFromTask(SubmitAsync, this.WhenAnyValue(x => x.IsBusy, busy => !busy));
        BackCommand = ReactiveCommand.CreateFromObservable(
            () => Router.NavigateBack.Execute().Select(_ => Unit.Default));
    }

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    public string City
    {
        get => _city;
        set => this.RaiseAndSetIfChanged(ref _city, value);
    }

    public string Animal
    {
        get => _animal;
        set => this.RaiseAndSetIfChanged(ref _animal, value);
    }

    public string ParentName
    {
        get => _parent;
        set => this.RaiseAndSetIfChanged(ref _parent, value);
    }

    public string? RecoveredPassword
    {
        get => _recoveredPassword;
        private set => this.RaiseAndSetIfChanged(ref _recoveredPassword, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public ReactiveCommand<Unit, Unit> SubmitCommand { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            StatusMessage = "请先输入用户名";
            return;
        }

        try
        {
            IsBusy = true;
            RecoveredPassword = null;
            StatusMessage = "正在验证...";

            var dto = new RecoverPasswordRequestDto
            {
                Username = Username,
                City = City,
                Animal = Animal,
                ParentName = ParentName
            };

            var password = await _chatService.RecoverPasswordAsync(dto);
            if (string.IsNullOrWhiteSpace(password))
            {
                StatusMessage = "验证失败，请检查答案是否正确";
            }
            else
            {
                RecoveredPassword = password;
                StatusMessage = "验证通过，以下是您的密码：";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"发生错误：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
