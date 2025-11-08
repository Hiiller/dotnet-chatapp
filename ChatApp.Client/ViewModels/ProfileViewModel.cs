
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using ChatApp.Client.Services;
using ChatApp.Client.Views;
using ChatApp.Client.Helpers;
using Avalonia.Media.Imaging;

namespace ChatApp.Client.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    private readonly Action _onPrimaryAction;
    private readonly Action? _onSecondaryAction;
    private readonly IChatService? _chatService;
    public IChatService? ChatService => _chatService;

    public Guid UserId { get; }
    private string _displayName;
    public string DisplayName
    {
        get => _displayName;
        set => this.RaiseAndSetIfChanged(ref _displayName, value);
    }
    public string AvatarInitials => BuildInitials(DisplayName);
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

    private string _identifierLabel = string.Empty;
    public string IdentifierLabel
    {
        get => _identifierLabel;
        set => this.RaiseAndSetIfChanged(ref _identifierLabel, value);
    }
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }
    private string _personalCode = string.Empty;
    public string PersonalCode
    {
        get => _personalCode;
        set => this.RaiseAndSetIfChanged(ref _personalCode, value);
    }

    public string PrimaryActionLabel { get; }
    public string? SecondaryActionLabel { get; }
    public bool HasSecondaryAction => !string.IsNullOrWhiteSpace(SecondaryActionLabel);

    public ObservableCollection<string> Highlights { get; }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> PrimaryActionCommand { get; }
    public ReactiveCommand<Unit, Unit>? SecondaryActionCommand { get; }
    public ReactiveCommand<Unit, Unit>? ShareCardCommand { get; }
    public Interaction<Unit, bool> EditProfileInteraction { get; } = new();
    public Interaction<Unit, bool> SecurityInteraction { get; } = new();
    public Interaction<(Guid userId, string displayName, string personalCode), Unit> ShareCardInteraction { get; } = new();
    private Bitmap? _avatarPreview;
    public Bitmap? AvatarPreview
    {
        get => _avatarPreview;
        set => this.RaiseAndSetIfChanged(ref _avatarPreview, value);
    }

    public ProfileViewModel(Guid userId, string displayName, bool isCurrentUser, Action? onPrimaryAction, RoutingState router)
        : this(userId, displayName, isCurrentUser, onPrimaryAction, null, router, null)
    {
    }

    public ProfileViewModel(Guid userId, string displayName, bool isCurrentUser, Action? onPrimaryAction, Action? onSecondaryAction, RoutingState router)
        : this(userId, displayName, isCurrentUser, onPrimaryAction, onSecondaryAction, router, null)
    {
    }

    public ProfileViewModel(Guid userId, string displayName, bool isCurrentUser, Action? onPrimaryAction, Action? onSecondaryAction, RoutingState router, IChatService? chatService)
        : base(router)
    {
        UserId = userId;
        _displayName = displayName;
        IsCurrentUser = isCurrentUser;
        IdentifierLabel = userId.ToString();
        _chatService = chatService;

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
        if (IsCurrentUser && _chatService != null)
        {
            PrimaryActionCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await EditProfileInteraction.Handle(Unit.Default).ToTask();
                await LoadProfileAsync();
            });
            SecondaryActionCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SecurityInteraction.Handle(Unit.Default).ToTask();
            });
            ShareCardCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var profile = await _chatService.GetProfile(UserId);
                var code = profile?.PersonalCode ?? IdentifierLabel;
                await ShareCardInteraction.Handle((UserId, DisplayName, code)).ToTask();
            });
        }
        else
        {
            PrimaryActionCommand = ReactiveCommand.Create(() => _onPrimaryAction());
            SecondaryActionCommand = string.IsNullOrWhiteSpace(SecondaryActionLabel)
                ? null
                : ReactiveCommand.Create(() => _onSecondaryAction?.Invoke());
        }
        _ = LoadProfileAsync();
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

    private async Task LoadProfileAsync()
    {
        try
        {
            if (_chatService == null) { await LoadAvatarAsync(); return; }
            var profile = await _chatService.GetProfile(UserId);
            if (profile != null)
            {
                Username = profile.Username;
                PersonalCode = profile.PersonalCode ?? string.Empty;
                var name = string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.Username : profile.DisplayName;
                DisplayName = name;
                IdentifierLabel = string.IsNullOrWhiteSpace(Username) ? UserId.ToString() : Username;
                await LoadAvatarAsync();
                ProfileEvents.RaiseProfileUpdated(UserId, name);
            }
        }
        catch { }
    }

    private async Task LoadAvatarAsync()
    {
        try
        {
            if (_chatService == null) { AvatarPreview = null; return; }
            var bytes = await AvatarCache.TryLoadAsync(UserId) ?? await _chatService.GetAvatar(UserId);
            if (bytes != null && bytes.Length > 0)
            {
                AvatarPreview = new Bitmap(new System.IO.MemoryStream(bytes));
                try { await AvatarCache.SaveAsync(UserId, bytes); } catch { }
            }
            else
            {
                AvatarPreview = null;
            }
        }
        catch { AvatarPreview = null; }
    }
}
