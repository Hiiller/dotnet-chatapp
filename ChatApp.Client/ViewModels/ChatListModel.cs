using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.Client.DTOs;
using ChatApp.Client.Helpers;
using ChatApp.Client.Models;
using ChatApp.Client.Services;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using Shared.Models;
using Splat;
using System.Reactive;
using Avalonia.Media.Imaging;

namespace ChatApp.Client.ViewModels;

public class ChatListModel : ViewModelBase
{
    private readonly LoginResponse _loginResponse;
    private readonly IHubService _hubService;
    private readonly ChatService _chatService;

    public ObservableCollection<UserModel> RecentContacts { get; }
    public ObservableCollection<UserModel> FilteredContacts { get; }
    public ObservableCollection<GroupModel> Groups { get; }
    public ObservableCollection<SettingOptionModel> SettingsOptions { get; }

    private ObservableCollection<MessageDto> _readMessages = new();
    public ObservableCollection<MessageDto> ReadMessages
    {
        get => _readMessages;
        set => this.SetProperty(ref _readMessages, value);
    }

    private string _newContactName = string.Empty;
    public string NewContactName
    {
        get => _newContactName;
        set => this.RaiseAndSetIfChanged(ref _newContactName, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            UpdateFilteredContacts();
        }
    }

    private string _userDisplayName = string.Empty;
    public string UserDisplayName
    {
        get => _userDisplayName;
        set => this.RaiseAndSetIfChanged(ref _userDisplayName, value);
    }
    private string _userInitials = string.Empty;
    public string UserInitials
    {
        get => _userInitials;
        set => this.RaiseAndSetIfChanged(ref _userInitials, value);
    }

    private Bitmap? _userAvatar;
    public Bitmap? UserAvatar
    {
        get => _userAvatar;
        set => this.RaiseAndSetIfChanged(ref _userAvatar, value);
    }

    private string _statusMessage = "在线 | Ready to chat";
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; }

    private AddRequestDto AddRequestDto => new()
    {
        userId = _loginResponse.currentUserId,
        friendName = NewContactName
    };

    public ChatListModel(LoginResponse loginResponse, RoutingState router) : base(router)
    {
        _loginResponse = loginResponse;

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5005")
        };
        _chatService = new ChatService(httpClient);

        _hubService = Locator.Current.GetService<IHubService>();
        _hubService.ConnectAsync(_loginResponse.currentUserId).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                _hubService.MessageReceived += OnMessageReceived;
            }
            else
            {
                Console.WriteLine("Error connecting to HubService in ChatListModel.");
            }
        });

        UserDisplayName = _loginResponse.currentUsername;
        UserInitials = BuildInitials(UserDisplayName);
        _ = LoadAvatarAsync();
        // Listen for profile updates to refresh UI immediately
        ProfileEvents.ProfileUpdated += (id, name) =>
        {
            if (id == _loginResponse.currentUserId)
            {
                UserDisplayName = name;
                UserInitials = BuildInitials(name);
                _ = LoadAvatarAsync();
            }
        };

        RecentContacts = new ObservableCollection<UserModel>();
        FilteredContacts = new ObservableCollection<UserModel>();
        Groups = new ObservableCollection<GroupModel>();
        SettingsOptions = new ObservableCollection<SettingOptionModel>();

        RecentContacts.CollectionChanged += RecentContactsOnCollectionChanged;

        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
        AddCommand = ReactiveCommand.Create(AddContact);
        OpenProfileCommand = ReactiveCommand.Create(OpenProfile);
        CreateGroupCommand = ReactiveCommand.Create(CreateGroup);

        InitializeSettingsOptions();
        InitializeGroups();

        RefreshCommand.Execute().Subscribe();
    }

    private void RecentContactsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateFilteredContacts();
    }

    private async Task RefreshAsync()
    {
        try
        {
            RecentContacts.Clear();
            var friendList = await _chatService.GetFriend(_loginResponse.currentUserId);
            Console.WriteLine("try getting friends....");

            foreach (var friend in friendList)
            {
                Console.WriteLine($"get friend:{friend.friendName},{friend.friendId}");
                var userModel = new UserModel
                {
                    Id = friend.friendId,
                    Username = friend.friendName,
                    AvatarInitials = BuildInitials(friend.friendName),
                    StatusMessage = "Available",
                    ButtonCommand = new RelayCommand(OnFriendSelected),
                };
                userModel.ProfileCommand = new RelayCommand(_ => OpenFriendProfile(userModel));
                RecentContacts.Add(userModel);
            }

            var recentMessages = await _chatService.GetRecentMessages(_loginResponse.currentUserId);
            foreach (var message in recentMessages)
            {
                var sender = RecentContacts.FirstOrDefault(u => u.Id == message.senderId || u.Id == message.receiverId);
                if (sender is null)
                {
                    continue;
                }

                sender.LastMessagePreview = string.IsNullOrWhiteSpace(message.content)
                    ? "图片或附件"
                    : message.content;

                if (message.receiverId == _loginResponse.currentUserId)
                {
                    sender.BackgroundColor = "#FF3B2F";
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void OnMessageReceived(MessageDto message)
    {
        if (message.receiverId != _loginResponse.currentUserId)
        {
            return;
        }

        var user = RecentContacts.FirstOrDefault(u => u.Id == message.senderId);
        if (user != null)
        {
            user.BackgroundColor = "#FF3B2F";
            user.LastMessagePreview = message.content;
        }

        Console.WriteLine($"List Received message: {message.content},id:{message.id}");
        _hubService.SetMessageToUnread(message);
    }

    private void OnFriendSelected(object? parameter)
    {
        if (parameter is not UserModel user)
        {
            return;
        }

        NavigateToChat(user);
        user.BackgroundColor = "#0078D7";
        user.LastMessagePreview = string.Empty;
        Cleanup();
    }

    private void NavigateToChat(UserModel user)
    {
        var contactor = new InContact
        {
            user_id = _loginResponse.currentUserId,
            _oppo_id = user.Id,
            _oppo_name = user.Username
        };

        Router.Navigate.Execute(new ChatViewModel(_loginResponse, contactor, Router));
    }

    private void OpenFriendProfile(UserModel user)
    {
        Router.Navigate.Execute(new ProfileViewModel(
            user.Id,
            user.Username,
            false,
            () => StartChatFromProfile(user),
            null,
            Router,
            _chatService));
    }

    private void StartChatFromProfile(UserModel user)
    {
        NavigateToChat(user);
    }

    private void AddContact()
    {
        if (string.IsNullOrWhiteSpace(NewContactName))
        {
            return;
        }
        // 不允许添加自己为好友（本地拦截）
        if (string.Equals(NewContactName, _loginResponse.currentUsername, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("不可添加自己为好友");
            return;
        }

        Console.WriteLine("try adding friend...." + AddRequestDto.friendName);
        _ = AddContactInternalAsync();
    }

    private async Task AddContactInternalAsync()
    {
        var friend = await _chatService.AddFriend(AddRequestDto);

        if (string.IsNullOrWhiteSpace(friend.friendName))
        {
            Console.WriteLine("friend object is null");
            return;
        }

        Console.WriteLine("add a friend: " + friend.friendName);
        NewContactName = string.Empty;

        var userModel = new UserModel
        {
            Id = friend.friendId,
            Username = friend.friendName,
            AvatarInitials = BuildInitials(friend.friendName),
            StatusMessage = "Just added",
            ButtonCommand = new RelayCommand(OnFriendSelected)
        };
        userModel.ProfileCommand = new RelayCommand(_ => OpenFriendProfile(userModel));

        RecentContacts.Add(userModel);
    }

    private void UpdateFilteredContacts()
    {
        var filter = SearchText?.Trim() ?? string.Empty;
        var query = string.IsNullOrWhiteSpace(filter)
            ? RecentContacts
            : RecentContacts.Where(user => user.Username.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                           user.StatusMessage.Contains(filter, StringComparison.OrdinalIgnoreCase));

        FilteredContacts.Clear();
        foreach (var contact in query)
        {
            FilteredContacts.Add(contact);
        }
    }

    private void InitializeSettingsOptions()
    {
        SettingsOptions.Clear();
        SettingsOptions.Add(new SettingOptionModel
        {
            Title = "通知设置",
            Description = "自定义消息提醒、静音和声音。",
            Command = new RelayCommand(_ => Console.WriteLine("Open notification settings"))
        });
        SettingsOptions.Add(new SettingOptionModel
        {
            Title = "隐私与安全",
            Description = "管理拉黑名单、最后上线时间和数据备份。",
            Command = new RelayCommand(_ => Console.WriteLine("Open privacy settings"))
        });
        SettingsOptions.Add(new SettingOptionModel
        {
            Title = "主题与外观",
            Description = "切换浅色/深色主题，调整聊天字体大小。",
            Command = new RelayCommand(_ => Console.WriteLine("Open appearance settings"))
        });
    }

    private void InitializeGroups()
    {
        Groups.Clear();
        Groups.Add(new GroupModel
        {
            Name = "产品讨论组",
            Description = "规划版本路线，分享最新迭代。",
            MemberCount = 12,
            IsPinned = true,
            OpenCommand = new RelayCommand(_ => Console.WriteLine("Navigate to 产品讨论组"))
        });
        Groups.Add(new GroupModel
        {
            Name = "设计灵感库",
            Description = "灵感、素材与设计评审集中地。",
            MemberCount = 8,
            OpenCommand = new RelayCommand(_ => Console.WriteLine("Navigate to 设计灵感库"))
        });
        Groups.Add(new GroupModel
        {
            Name = "周末出游群",
            Description = "一起规划下一次线下团建。",
            MemberCount = 5,
            OpenCommand = new RelayCommand(_ => Console.WriteLine("Navigate to 周末出游群"))
        });
    }

    private void OpenProfile()
    {
        Router.Navigate.Execute(new ProfileViewModel(
            _loginResponse.currentUserId,
            _loginResponse.currentUsername,
            true,
            () => Console.WriteLine("Edit profile"),
            null,
            Router,
            _chatService));
    }

    private void CreateGroup()
    {
        Console.WriteLine("Launch group creation flow");
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

        return string.Concat(parts[0].First(), parts[^1].First()).ToUpperInvariant();
    }

    public void Cleanup()
    {
        _hubService.MessageReceived -= OnMessageReceived;
    }

    private async Task LoadAvatarAsync()
    {
        try
        {
            var bytes = await AvatarCache.TryLoadAsync(_loginResponse.currentUserId) ?? await _chatService.GetAvatar(_loginResponse.currentUserId);
            if (bytes != null && bytes.Length > 0)
            {
                UserAvatar = new Bitmap(new System.IO.MemoryStream(bytes));
                try { await AvatarCache.SaveAsync(_loginResponse.currentUserId, bytes); } catch { }
            }
            else
            {
                UserAvatar = null;
            }
        }
        catch { UserAvatar = null; }
    }
}
