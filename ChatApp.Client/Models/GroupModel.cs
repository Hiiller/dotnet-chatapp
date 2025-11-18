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

    public string GroupCode
    {
        get => _groupCode;
        set => this.RaiseAndSetIfChanged(ref _groupCode, value);
    }
    private string _groupCode = string.Empty;

    public string? MemberRole
    {
        get => _memberRole;
        set
        {
            this.RaiseAndSetIfChanged(ref _memberRole, value);
            this.RaisePropertyChanged(nameof(CanManageMembers));
        }
    }
    private string? _memberRole;

    public bool IsMember
    {
        get => _isMember;
        set => this.RaiseAndSetIfChanged(ref _isMember, value);
    }
    private bool _isMember;

    public bool HasPendingRequest
    {
        get => _hasPendingRequest;
        set => this.RaiseAndSetIfChanged(ref _hasPendingRequest, value);
    }
    private bool _hasPendingRequest;

    public Guid CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool CanManageMembers =>
        string.Equals(MemberRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(MemberRole, "Creator", StringComparison.OrdinalIgnoreCase);

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
