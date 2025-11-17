
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
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
        set
        {
            if (_displayName != value)
            {
                this.RaiseAndSetIfChanged(ref _displayName, value);
                this.RaisePropertyChanged(nameof(AvatarInitials));
            }
            else
            {
                // Even if value is the same, force property change notification to ensure UI updates
                // This is important when data is reloaded from database after an update
                this.RaisePropertyChanged(nameof(DisplayName));
                this.RaisePropertyChanged(nameof(AvatarInitials));
            }
        }
    }
    private string _userInitials = string.Empty;
    public string UserInitials
    {
        get => _userInitials;
        set => this.RaiseAndSetIfChanged(ref _userInitials, value);
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
    public ReactiveCommand<Unit, Unit>? LoadProfileCommand { get; }
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
        IsCurrentUser = isCurrentUser;
        _chatService = chatService;

        // Don't use the passed displayName - it might be stale. Load from database instead.
        // Use it only as a fallback placeholder until LoadProfileAsync completes
        _displayName = displayName ?? userId.ToString();
        
        // Initialize with empty/placeholder values - will be loaded from database
        _username = string.Empty;
        _personalCode = string.Empty;
        IdentifierLabel = userId.ToString(); // Will be updated after loading from DB
        _statusMessage = string.Empty; // Will be loaded from database (Bio field)
        _about = string.Empty; // Will be loaded from database, don't set default here

        PrimaryActionLabel = isCurrentUser ? "编辑资料" : "开始聊天";
        SecondaryActionLabel = isCurrentUser ? "账号与安全" : "发起语音通话";

        Highlights = new ObservableCollection<string>
        {
            "最近上线：刚刚",
            "群聊参与：加载中...",
            "好友数量：加载中..."
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
            
            // Subscribe to profile update events - use lambda like ChatListModel does
            ProfileEvents.ProfileUpdated += (id, name) =>
            {
                if (id == UserId)
                {
                    // Directly update DisplayName like ChatListModel does for immediate UI update
                    DisplayName = name;
                    // Reload full profile from database to get Bio and other updated fields
                    UserInitials = BuildInitials(name);
                    _ = LoadProfileAsync(); // Reload full profile to get Bio and other fields
                }
            };
            
            // Create command to reload profile from database
            LoadProfileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await LoadProfileAsync();
            });
        }
        else
        {
            PrimaryActionCommand = ReactiveCommand.Create(() => _onPrimaryAction());
            SecondaryActionCommand = string.IsNullOrWhiteSpace(SecondaryActionLabel)
                ? null
                : ReactiveCommand.Create(() => _onSecondaryAction?.Invoke());
            
            // Also create LoadProfileCommand for non-current users
            if (_chatService != null)
            {
                LoadProfileCommand = ReactiveCommand.CreateFromTask(async () =>
                {
                    await LoadProfileAsync();
                });
            }
        }
        
        // Initial load in constructor - View's WhenActivated will also trigger reload
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
            if (_chatService == null) 
            { 
                await LoadAvatarAsync(); 
                return; 
            }
            
            // Load profile from database - same way EditProfileViewModel does
            var profile = await _chatService.GetProfile(UserId);
            
            if (profile == null)
            {
                return;
            }
            
            // Directly use values from database, just like EditProfileViewModel does
            Username = profile.Username;
            PersonalCode = profile.PersonalCode ?? string.Empty;
            
            // DisplayName: use DisplayName if available, otherwise fallback to Username
            var newDisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) 
                ? profile.Username 
                : profile.DisplayName;
            
            DisplayName = newDisplayName;
            
            IdentifierLabel = string.IsNullOrWhiteSpace(Username) 
                ? UserId.ToString() 
                : Username;
            
            // Bio/个性签名: directly use Bio from database
            // StatusMessage shows Bio (under avatar), About also shows Bio (in the text box)
            if (!string.IsNullOrWhiteSpace(profile.Bio))
            {
                StatusMessage = profile.Bio;
                About = profile.Bio;
            }
            else
            {
                // Default messages when Bio is empty
                StatusMessage = IsCurrentUser ? "打造属于你的个性签名" : "向 Ta 打个招呼吧";
                About = IsCurrentUser
                    ? "完善个人资料，让好友更好地了解你。"
                    : "还没有更多资料，发送第一条消息开始建立联系。";
            }
            
            await LoadAvatarAsync();
            await LoadHighlightsAsync();
        }
        catch
        { 
            // Silently handle errors
        }
    }

    private async Task LoadHighlightsAsync()
    {
        try
        {
            if (_chatService == null) return;
            
            // Load friends count from database
            var friends = await _chatService.GetFriend(UserId);
            var friendsCount = friends?.Count ?? 0;
            
            // Get groups the user has participated in (sent messages to)
            // This uses a new API endpoint that queries the database directly
            var userGroups = await _chatService.GetGroupsByUser(UserId);
            var groupsCount = userGroups?.Count ?? 0;
            
            // Update highlights
            if (Highlights.Count >= 3)
            {
                Highlights[1] = $"群聊参与：{groupsCount} 个";
                Highlights[2] = $"好友数量：{friendsCount} 位";
            }
        }
        catch
        {
            // If loading fails, set to 0
            if (Highlights.Count >= 3)
            {
                Highlights[1] = "群聊参与：0 个";
                Highlights[2] = "好友数量：0 位";
            }
        }
    }

    private async Task LoadAvatarAsync()
    {
        try
        {
            if (_chatService == null) { AvatarPreview = null; return; }
            var bytes = await AvatarCache.TryLoadAsync(UserId) ?? await _chatService.GetAvatar(UserId);
            if (bytes != null && bytes.Length > 0)
            {
                try
                {
                    var bmp = new Bitmap(new System.IO.MemoryStream(bytes));
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => AvatarPreview = bmp);
                }
                catch
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => AvatarPreview = null);
                }
                try { await AvatarCache.SaveAsync(UserId, bytes); } catch { }
            }
            else
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => AvatarPreview = null);
            }
        }
        catch { Avalonia.Threading.Dispatcher.UIThread.Post(() => AvatarPreview = null); }
    }
}
