using ReactiveUI;
using System;
using System.Windows.Input;

namespace ChatApp.Client.Models;

public class GroupModel : ReactiveObject
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    private string _name = string.Empty;

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
    private string _description = string.Empty;

    public int MemberCount
    {
        get => _memberCount;
        set => this.RaiseAndSetIfChanged(ref _memberCount, value);
    }
    private int _memberCount;

    public bool IsPinned
    {
        get => _isPinned;
        set => this.RaiseAndSetIfChanged(ref _isPinned, value);
    }
    private bool _isPinned;

    public ICommand OpenCommand
    {
        get => _openCommand;
        set => this.RaiseAndSetIfChanged(ref _openCommand, value);
    }
    private ICommand _openCommand = ReactiveCommand.Create(() => { });
}