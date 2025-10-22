
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace ChatApp.Client.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    private readonly Action _onPrimaryAction;
    private readonly Action? _onSecondaryAction;

    public Guid UserId { get; }
    public string DisplayName { get; }
    public string AvatarInitials { get; }
    public bool IsCurrentUser { get; }

    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    private string _about;
    public string About
    {
        get => _about;
        set => this.RaiseAndSetIfChanged(ref _about, value);
    }

    public string IdentifierLabel { get; }

    public string PrimaryActionLabel { get; }
    public string? SecondaryActionLabel { get; }
    public bool HasSecondaryAction => !string.IsNullOrWhiteSpace(SecondaryActionLabel);

    public ObservableCollection<string> Highlights { get; }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> PrimaryActionCommand { get; }
    public ReactiveCommand<Unit, Unit>? SecondaryActionCommand { get; }

    public ProfileViewModel(Guid userId, string displayName, bool isCurrentUser, Action? onPrimaryAction, RoutingState router)
        : this(userId, displayName, isCurrentUser, onPrimaryAction, null, router)
    {
    }

    public ProfileViewModel(Guid userId, string displayName, bool isCurrentUser, Action? onPrimaryAction, Action? onSecondaryAction, RoutingState router)
        : base(router)
    {
        UserId = userId;
        DisplayName = displayName;
        IsCurrentUser = isCurrentUser;
        AvatarInitials = BuildInitials(displayName);
        IdentifierLabel = userId.ToString();

        _statusMessage = isCurrentUser ? "打造属于你的个性签名" : "向 Ta 打个招呼吧";
        _about = isCurrentUser
            ? "完善个人资料，让好友更好地了解你。"
            : "还没有更多资料，发送第一条消息开始建立联系。";

        PrimaryActionLabel = isCurrentUser ? "编辑资料" : "开始聊天";
        SecondaryActionLabel = isCurrentUser ? "账号与安全" : "发起语音通话";

        Highlights = new ObservableCollection<string>
        {
            "最近上线：刚刚",
            "群聊参与：3 个",
            "置顶好友：2 位"
        };

        _onPrimaryAction = onPrimaryAction ?? (() => { });
        _onSecondaryAction = onSecondaryAction;

        BackCommand = ReactiveCommand.CreateFromObservable(
            () => Router.NavigateBack.Execute().Select(_ => Unit.Default)
        );
        PrimaryActionCommand = ReactiveCommand.Create(() => _onPrimaryAction());
        SecondaryActionCommand = string.IsNullOrWhiteSpace(SecondaryActionLabel)
            ? null
            : ReactiveCommand.Create(() => _onSecondaryAction?.Invoke());
    }

    private static string BuildInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
        }

        return string.Concat(parts[0].AsSpan(0, 1), parts[^1].AsSpan(0, 1)).ToUpperInvariant();
    }
}
