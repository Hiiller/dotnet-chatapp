using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ChatApp.Client.DTOs;
using ChatApp.Client.Models;
using ChatApp.Client.Services;
using ReactiveUI;

namespace ChatApp.Client.ViewModels;

public class GroupDetailsViewModel : ViewModelBase
{
    private readonly IChatService _chatService;
    private readonly Guid _currentUserId;
    private readonly DispatcherTimer _refreshTimer;

    private bool _isLoading;
    private bool _canEditDescription;
    private bool _isDescriptionDirty;
    private bool _canManageMembers;
    private string _editableDescription = string.Empty;
    private string _lastSavedDescription = string.Empty;

    private GroupModel _group;
    private string _groupName;
    private string _groupCode;
    private string _creatorName = string.Empty;
    private string _statusMessage = string.Empty;
    private int _memberCount;
    private string _groupDescription = string.Empty;
    private string _currentUserRole = "Member";

    public GroupDetailsViewModel(GroupModel group, Guid currentUserId, RoutingState router, IChatService chatService)
        : base(router)
    {
        _group = group;
        _currentUserId = currentUserId;
        _chatService = chatService;
        _groupName = group.Name ?? "Unnamed group";
        _groupCode = string.IsNullOrWhiteSpace(group.GroupCode) ? "Unknown" : group.GroupCode;
        _memberCount = group.MemberCount;
        _groupDescription = group.Description ?? string.Empty;
        _currentUserRole = NormalizeRole(group.MemberRole);

        Members = new ObservableCollection<GroupMemberDto>();
        PendingRequests = new ObservableCollection<GroupJoinRequestDto>();

        BackCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await Router.NavigateBack.Execute();
            return Unit.Default;
        });
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadDetailsAsync);
        RemoveMemberCommand = ReactiveCommand.CreateFromTask<GroupMemberDto>(RemoveMemberAsync);
        AcceptRequestCommand = ReactiveCommand.CreateFromTask<GroupJoinRequestDto>(request => RespondToRequestAsync(request, true));
        RejectRequestCommand = ReactiveCommand.CreateFromTask<GroupJoinRequestDto>(request => RespondToRequestAsync(request, false));
        LeaveGroupCommand = ReactiveCommand.CreateFromTask(LeaveGroupAsync);
        SaveDescriptionCommand = ReactiveCommand.CreateFromTask(
            SaveDescriptionAsync,
            this.WhenAnyValue(x => x.CanEditDescription, x => x.IsDescriptionDirty, (can, dirty) => can && dirty));

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
        _refreshTimer.Tick += async (_, _) => await LoadDetailsAsync();
        _refreshTimer.Start();

        _ = LoadDetailsAsync();
    }

    public GroupModel Group
    {
        get => _group;
        set => this.RaiseAndSetIfChanged(ref _group, value);
    }

    public string GroupName
    {
        get => _groupName;
        set => this.RaiseAndSetIfChanged(ref _groupName, value);
    }

    public string GroupCode
    {
        get => _groupCode;
        set => this.RaiseAndSetIfChanged(ref _groupCode, value);
    }

    public string CreatorName
    {
        get => _creatorName;
        set => this.RaiseAndSetIfChanged(ref _creatorName, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool CanEditDescription
    {
        get => _canEditDescription;
        private set => this.RaiseAndSetIfChanged(ref _canEditDescription, value);
    }

    public bool CanManageMembers
    {
        get => _canManageMembers;
        private set => this.RaiseAndSetIfChanged(ref _canManageMembers, value);
    }

    public string GroupDescription
    {
        get => _groupDescription;
        private set => this.RaiseAndSetIfChanged(ref _groupDescription, value);
    }

    public string CurrentUserRole
    {
        get => _currentUserRole;
        private set => this.RaiseAndSetIfChanged(ref _currentUserRole, value);
    }

    public string EditableDescription
    {
        get => _editableDescription;
        set
        {
            this.RaiseAndSetIfChanged(ref _editableDescription, value);
            IsDescriptionDirty = !_editableDescription.Equals(_lastSavedDescription, StringComparison.Ordinal);
        }
    }

    public bool IsDescriptionDirty
    {
        get => _isDescriptionDirty;
        private set => this.RaiseAndSetIfChanged(ref _isDescriptionDirty, value);
    }

    public bool IsBusy
    {
        get => _isLoading;
        private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public int MemberCount
    {
        get => _memberCount;
        set => this.RaiseAndSetIfChanged(ref _memberCount, value);
    }

    public ObservableCollection<GroupMemberDto> Members { get; }
    public ObservableCollection<GroupJoinRequestDto> PendingRequests { get; }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<GroupMemberDto, Unit> RemoveMemberCommand { get; }
    public ReactiveCommand<GroupJoinRequestDto, Unit> AcceptRequestCommand { get; }
    public ReactiveCommand<GroupJoinRequestDto, Unit> RejectRequestCommand { get; }
    public ReactiveCommand<Unit, Unit> LeaveGroupCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveDescriptionCommand { get; }

    public override void Disappearing()
    {
        base.Disappearing();
        _refreshTimer.Stop();
    }

    private async Task LoadDetailsAsync()
    {
        if (_isLoading || _chatService == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var detail = await _chatService.GetGroupDetailsAsync(Group.Id, _currentUserId);
            if (detail == null)
            {
                StatusMessage = "Unable to load group information";
                return;
            }

            GroupName = detail.Name;
            GroupCode = detail.GroupCode;
            CreatorName = detail.CreatorName;
            MemberCount = detail.MemberCount;
            Group.MemberCount = detail.MemberCount;
            Group.MemberRole = detail.MemberRole;
            Group.Description = detail.Description ?? Group.Description;
            GroupDescription = Group.Description ?? string.Empty;
            CurrentUserRole = NormalizeRole(detail.MemberRole);

            var isCreator = detail.CreatorId == _currentUserId;
            CanEditDescription = isCreator;
            UpdateDescriptionState(detail.Description ?? Group.Description ?? string.Empty);

            StatusMessage = detail.HasPendingRequest ? "Join request submitted, waiting for approval" : string.Empty;
            CanManageMembers = detail.CanManageMembers;

            Members.Clear();
            if (detail.Members != null)
            {
                foreach (var member in detail.Members)
                {
                    Members.Add(member);
                }
            }

            PendingRequests.Clear();
            if (detail.CanManageMembers && detail.PendingRequests != null)
            {
                foreach (var request in detail.PendingRequests)
                {
                    PendingRequests.Add(request);
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateDescriptionState(string description)
    {
        _lastSavedDescription = description;
        _editableDescription = description;
        GroupDescription = description;
        this.RaisePropertyChanged(nameof(EditableDescription));
        IsDescriptionDirty = false;
    }

    private async Task SaveDescriptionAsync()
    {
        if (!CanEditDescription || !IsDescriptionDirty)
        {
            return;
        }

        var success = await _chatService.UpdateGroupDescriptionAsync(Group.Id, _currentUserId, EditableDescription);
        if (success)
        {
            UpdateDescriptionState(EditableDescription);
            Group.Description = EditableDescription;
            GroupDescription = EditableDescription;
            StatusMessage = "Group description updated";
        }
        else
        {
            StatusMessage = "Failed to update group description";
        }
    }

    private async Task RemoveMemberAsync(GroupMemberDto? member)
    {
        if (member == null || member.UserId == Guid.Empty)
        {
            return;
        }

        var success = await _chatService.RemoveGroupMemberAsync(Group.Id, member.UserId, _currentUserId);
        if (success)
        {
            await LoadDetailsAsync();
        }
        else
        {
            StatusMessage = "Failed to remove member";
        }
    }

    private async Task RespondToRequestAsync(GroupJoinRequestDto? request, bool accept)
    {
        if (request == null)
        {
            return;
        }

        var success = await _chatService.RespondToGroupRequestAsync(Group.Id, request.Id, _currentUserId, accept);
        if (success)
        {
            await LoadDetailsAsync();
        }
        else
        {
            StatusMessage = accept ? "Failed to approve request" : "Failed to reject request";
        }
    }

    private async Task LeaveGroupAsync()
    {
        var success = await _chatService.RemoveGroupMemberAsync(Group.Id, _currentUserId, _currentUserId);
        if (success)
        {
            _refreshTimer.Stop();
            StatusMessage = "You have left the group";
            await Router.NavigateBack.Execute();
        }
        else
        {
            StatusMessage = "Failed to leave group";
        }
    }

    private static string NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return "Member";
        }

        var lower = role.Trim().ToLowerInvariant();
        return lower switch
        {
            "creator" => "Creator",
            "admin" => "Admin",
            "owner" => "Owner",
            _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(lower),
        };
    }
}
