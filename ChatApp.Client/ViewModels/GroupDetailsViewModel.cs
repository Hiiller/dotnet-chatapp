using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using ChatApp.Client.Services;
using ChatApp.Client.Views;
using ChatApp.Client.Models;

namespace ChatApp.Client.ViewModels;

public class GroupDetailsViewModel : ViewModelBase
{
    private readonly IChatService? _chatService;
    private GroupModel _group;
    private string _groupName;
    private string _groupDescription;
    private ObservableCollection<UserModel> _members;

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

    public ObservableCollection<UserModel> Members
    {
        get => _members;
        set => this.RaiseAndSetIfChanged(ref _members, value);
    }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> EditGroupCommand { get; }
    public ReactiveCommand<Unit, Unit> AddMemberCommand { get; }
    public ReactiveCommand<Unit, Unit> LeaveGroupCommand { get; }

    public GroupDetailsViewModel(GroupModel group, RoutingState router, IChatService? chatService = null)
        : base(router)
    {
        _chatService = chatService;
        _group = group;
        _groupName = group.Name ?? "未命名群组";
        _groupDescription = group.Description ?? "暂无描述";
        _members = new ObservableCollection<UserModel>();

        BackCommand = ReactiveCommand.CreateFromObservable(
            () => Router.NavigateBack.Execute().Select(_ => Unit.Default)
        );

        EditGroupCommand = ReactiveCommand.Create(() => { /* TODO: 实现编辑群组功能 */ });
        AddMemberCommand = ReactiveCommand.Create(() => { /* TODO: 实现添加成员功能 */ });
        LeaveGroupCommand = ReactiveCommand.Create(() => { /* TODO: 实现退出群组功能 */ });

        // 加载成员列表
        _ = LoadMembersAsync();
    }

    private async Task LoadMembersAsync()
    {
        try
        {
            if (_chatService == null) return;

            // TODO: 实现获取群组成员的API
            // var members = await _chatService.GetGroupMembers(Group.Id);
            // Members = new ObservableCollection<UserModel>(members);
        }
        catch (Exception ex)
        {
            // 处理错误
        }
    }
}
