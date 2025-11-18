using System;
using System.Reactive;
using System.Threading.Tasks;
using ChatApp.Client.DTOs;
using ChatApp.Client.Services;
using ReactiveUI;

namespace ChatApp.Client.ViewModels;

public class PrivacySettingsViewModel : ReactiveObject
{
    private readonly Guid _userId;
    private readonly IChatService _chatService;
    private readonly Action _closeAction;
    private string _cityAnswer = string.Empty;
    private string _animalAnswer = string.Empty;
    private string _parentAnswer = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public PrivacySettingsViewModel(Guid userId, IChatService chatService, Action closeAction)
    {
        _userId = userId;
        _chatService = chatService;
        _closeAction = closeAction;

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync, this.WhenAnyValue(x => x.IsBusy, busy => !busy));
        CloseCommand = ReactiveCommand.Create(_closeAction);

        _ = LoadAsync();
    }

    public string CityAnswer
    {
        get => _cityAnswer;
        set => this.RaiseAndSetIfChanged(ref _cityAnswer, value);
    }

    public string AnimalAnswer
    {
        get => _animalAnswer;
        set => this.RaiseAndSetIfChanged(ref _animalAnswer, value);
    }

    public string ParentAnswer
    {
        get => _parentAnswer;
        set => this.RaiseAndSetIfChanged(ref _parentAnswer, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            var result = await _chatService.GetSecurityAnswersAsync(_userId);
            if (result != null)
            {
                CityAnswer = result.City;
                AnimalAnswer = result.Animal;
                ParentAnswer = result.ParentName;
            }
            StatusMessage = "安全答案仅用于密码找回，请妥善保管";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            var payload = new SecurityAnswersDto
            {
                City = CityAnswer,
                Animal = AnimalAnswer,
                ParentName = ParentAnswer
            };
            var success = await _chatService.UpdateSecurityAnswersAsync(_userId, payload);
            StatusMessage = success ? "安全问题已更新" : "保存失败，请稍后再试";
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
