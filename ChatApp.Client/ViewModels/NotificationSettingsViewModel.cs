using System;
using System.Reactive;
using ReactiveUI;
using ChatApp.Client.Services;

namespace ChatApp.Client.ViewModels;

public class NotificationSettingsViewModel : ReactiveObject
{
    private readonly Action _closeAction;
    private bool _muteAll;
    private bool _enableSound;

    public NotificationSettingsViewModel(Action closeAction)
    {
        _closeAction = closeAction;

        var snapshot = NotificationSettingsService.GetSnapshot();
        _muteAll = snapshot.MuteAll;
        _enableSound = snapshot.EnableSound;

        CloseCommand = ReactiveCommand.Create(_closeAction);
    }

    public bool MuteAll
    {
        get => _muteAll;
        set
        {
            this.RaiseAndSetIfChanged(ref _muteAll, value);
            Save();
        }
    }

    public bool EnableSound
    {
        get => _enableSound;
        set
        {
            this.RaiseAndSetIfChanged(ref _enableSound, value);
            Save();
        }
    }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    private void Save()
    {
        var updated = new NotificationSettings
        {
            MuteAll = _muteAll,
            EnableSound = _enableSound
        };
        NotificationSettingsService.Update(updated);
    }
}
