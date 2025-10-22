using ReactiveUI;
using System.Windows.Input;

namespace ChatApp.Client.Models;

public class SettingOptionModel : ReactiveObject
{
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    private string _title = string.Empty;

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
    private string _description = string.Empty;

    public ICommand Command
    {
        get => _command;
        set => this.RaiseAndSetIfChanged(ref _command, value);
    }
    private ICommand _command = ReactiveCommand.Create(() => { });
}