using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ChatApp.Client.DTOs;
using ChatApp.Client.Models;
using ChatApp.Client.Services;
using ReactiveUI;

namespace ChatApp.Client.ViewModels;

public class GroupDetailsViewModel : ViewModelBase
{
    private readonly IChatService _chatService;
    private readonly Guid _currentUserId;
    private GroupModel _group;
    private string _groupName;
    private string _groupDescription;
    private string _groupCode;
    private string _creatorName = string.Empty;
    private string? _currentUserRole;
    private string _statusMessage = string.Empty;
    private bool _canManageMembers;
    private int _memberCount;
    private bool _isBusy;

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

    public string GroupDescription
    {
        get => _groupDescription;
        set => this.RaiseAndSetIfChanged(ref _groupDescription, value);
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

    public string? CurrentUserRole
    {
        get => _currentUserRole;
        set => this.RaiseAndSetIfChanged(ref _currentUserRole, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool CanManageMembers
    {
        get => _canManageMembers;
        set => this.RaiseAndSetIfChanged(ref _canManageMembers, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public int MemberCount
    {
        get => _memberCount;
        set => this.RaiseAndSetIfChanged(ref _memberCount, value);
    }

    public Guid CurrentUserId => _currentUserId;

    public ObservableCollection<GroupMemberDto> Members { get; }
    public ObservableCollection<GroupJoinRequestDto> PendingRequests { get; }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<GroupMemberDto, Unit> RemoveMemberCommand { get; }
    public ReactiveCommand<GroupJoinRequestDto, Unit> AcceptRequestCommand { get; }
    public ReactiveCommand<GroupJoinRequestDto, Unit> RejectRequestCommand { get; }
    public ReactiveCommand<Unit, Unit> LeaveGroupCommand { get; }
    public ReactiveCommand<Unit, Unit> EditGroupCommand { get; }
    public ReactiveCommand<Unit, Unit> AddMemberCommand { get; }

    public GroupDetailsViewModel(GroupModel group, Guid currentUserId, RoutingState router, IChatService chatService)
        : base(router)
    {
        _group = group;
        _currentUserId = currentUserId;
        _chatService = chatService;
        _groupName = group.Name ?? "未命名群组";
        _groupDescription = string.IsNullOrWhiteSpace(group.Description) ? "暂无描述" : group.Description;
        _groupCode = string.IsNullOrWhiteSpace(group.GroupCode) ? "未知" : group.GroupCode;
        _memberCount = group.MemberCount;

        Members = new ObservableCollection<GroupMemberDto>();
        PendingRequests = new ObservableCollection<GroupJoinRequestDto>();

        BackCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await Router.NavigateBack.Execute();
        });
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadDetailsAsync);
        RemoveMemberCommand = ReactiveCommand.CreateFromTask<GroupMemberDto>(RemoveMemberAsync);
        AcceptRequestCommand = ReactiveCommand.CreateFromTask<GroupJoinRequestDto>(request => RespondToRequestAsync(request, true));
        RejectRequestCommand = ReactiveCommand.CreateFromTask<GroupJoinRequestDto>(request => RespondToRequestAsync(request, false));
        LeaveGroupCommand = ReactiveCommand.CreateFromTask(LeaveGroupAsync);
        EditGroupCommand = ReactiveCommand.Create(() => { });
        AddMemberCommand = ReactiveCommand.Create(() => { });

        _ = LoadDetailsAsync();
    }

    private async Task LoadDetailsAsync()
    {
        if (_chatService == null)
        {
            StatusMessage = "未找到聊天服务实例";
            return;
        }

        try
        {
            IsBusy = true;
            var detail = await _chatService.GetGroupDetailsAsync(Group.Id, _currentUserId);
            if (detail == null)
            {
                StatusMessage = "无法加载群信息";
                return;
            }

            GroupName = detail.Name;
            Group.Description = detail.Description ?? Group.Description;
            GroupDescription = string.IsNullOrWhiteSpace(detail.Description) ? "暂无描述" : detail.Description;
            GroupCode = detail.GroupCode;
            CreatorName = detail.CreatorName;
            MemberCount = detail.MemberCount;
            Group.MemberCount = detail.MemberCount;
            Group.MemberRole = detail.MemberRole;
            CurrentUserRole = detail.CurrentUserRole ?? detail.MemberRole;
            CanManageMembers = detail.CanManageMembers;
            StatusMessage = detail.HasPendingRequest ? "已提交加群请求，等待审批" : string.Empty;

            Members.Clear();
            var members = detail.Members ?? new System.Collections.Generic.List<GroupMemberDto>();
            foreach (var member in members)
            {
                Members.Add(member);
            }

            PendingRequests.Clear();
            if (detail.CanManageMembers)
            {
                var requests = detail.PendingRequests ?? new System.Collections.Generic.List<GroupJoinRequestDto>();
                foreach (var request in requests)
                {
                    PendingRequests.Add(request);
                }
            }
            else
            {
                StatusMessage = string.IsNullOrEmpty(StatusMessage) ? string.Empty : StatusMessage;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
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
            StatusMessage = "移除成员失败";
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
            StatusMessage = accept ? "通过请求失败" : "拒绝请求失败";
        }
    }

    private async Task LeaveGroupAsync()
    {
        var success = await _chatService.RemoveGroupMemberAsync(Group.Id, _currentUserId, _currentUserId);
        if (success)
        {
            await LoadDetailsAsync();
            StatusMessage = "已退出群组";
        }
        else
        {
            StatusMessage = "退出群组失败";
        }
    }
}
