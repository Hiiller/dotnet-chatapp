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
using static ChatApp.Client.Helpers.DebugLogger;

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
    public ReactiveCommand<Unit, Unit> RefreshRightPanelCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowSearchFriendsCommand { get; }
    public ReactiveCommand<Unit, Unit> PopoutChatCommand { get; }
    public ReactiveCommand<UserModel, Unit> AcceptFriendRequestCommand { get; }
    public ReactiveCommand<UserModel, Unit> RejectFriendRequestCommand { get; }
    
    // Embedded mode support
    private object? _rightPanelContent;
    public object? RightPanelContent
    {
        get => _rightPanelContent;
        set
        {
            Log("ChatListModel", $"RightPanelContent setter: old={_rightPanelContent?.GetType().Name ?? "null"}, new={value?.GetType().Name ?? "null"}");
            this.RaiseAndSetIfChanged(ref _rightPanelContent, value);
            // Force property change notification to ensure UI updates immediately
            this.RaisePropertyChanged(nameof(RightPanelContent));
            Log("ChatListModel", $"RightPanelContent property changed notification raised, current value: {_rightPanelContent?.GetType().Name ?? "null"}");
        }
    }
    
    private bool _isEmbeddedChatMode;
    public bool IsEmbeddedChatMode
    {
        get => _isEmbeddedChatMode;
        set
        {
            Log("ChatListModel", $"IsEmbeddedChatMode setter: old={_isEmbeddedChatMode}, new={value}");
            this.RaiseAndSetIfChanged(ref _isEmbeddedChatMode, value);
            // Force property change notification to ensure UI updates immediately
            this.RaisePropertyChanged(nameof(IsEmbeddedChatMode));
            Log("ChatListModel", $"IsEmbeddedChatMode property changed notification raised, current value: {_isEmbeddedChatMode}");
        }
    }
    
    private ChatViewModel? _embeddedChatViewModel;
    public ChatViewModel? EmbeddedChatViewModel
    {
        get => _embeddedChatViewModel;
        set => this.RaiseAndSetIfChanged(ref _embeddedChatViewModel, value);
    }

    private AddRequestDto AddRequestDto => new()
    {
        userId = _loginResponse.currentUserId,
        friendName = NewContactName
    };

    public ChatListModel(LoginResponse loginResponse, RoutingState router) : base(router)
    {
        try
        {
            Log("ChatListModel", "Constructor: Starting initialization");
            _loginResponse = loginResponse;

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5005")
            };
            _chatService = new ChatService(httpClient);
            Log("ChatListModel", "Constructor: ChatService created");

            _hubService = Locator.Current.GetService<IHubService>();
            if (_hubService != null)
            {
                _hubService.ConnectAsync(_loginResponse.currentUserId).ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        _hubService.MessageReceived += OnMessageReceived;
                        Log("ChatListModel", "Constructor: HubService connected");
                    }
                    else
                    {
                        Log("ChatListModel", $"Constructor: Error connecting to HubService: {task.Exception?.Message}");
                    }
                });
            }
            else
            {
                Log("ChatListModel", "Constructor: Warning - IHubService not found in Locator");
            }

            UserDisplayName = _loginResponse.currentUsername;
            UserInitials = BuildInitials(UserDisplayName);
            _ = LoadAvatarAsync();
            Log("ChatListModel", "Constructor: User profile initialized");
            
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
            Log("ChatListModel", "Constructor: Collections created");

            RecentContacts.CollectionChanged += RecentContactsOnCollectionChanged;

            Log("ChatListModel", "Constructor: Creating commands...");
            try
            {
                RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
                Log("ChatListModel", "Constructor: RefreshCommand created");
                
                RefreshRightPanelCommand = ReactiveCommand.CreateFromTask(RefreshRightPanelAsync);
                Log("ChatListModel", "Constructor: RefreshRightPanelCommand created");
                
                AddCommand = ReactiveCommand.Create(ShowSearchFriends);
                Log("ChatListModel", "Constructor: AddCommand created");
                
                OpenProfileCommand = ReactiveCommand.Create(OpenProfile);
                Log("ChatListModel", "Constructor: OpenProfileCommand created");
                
                CreateGroupCommand = ReactiveCommand.CreateFromTask(CreateGroupAsync);
                Log("ChatListModel", "Constructor: CreateGroupCommand created");
                
                ShowSearchFriendsCommand = ReactiveCommand.Create(ShowSearchFriends);
                Log("ChatListModel", "Constructor: ShowSearchFriendsCommand created");
                
                PopoutChatCommand = ReactiveCommand.Create(PopoutChat);
                Log("ChatListModel", "Constructor: PopoutChatCommand created");
                
                AcceptFriendRequestCommand = ReactiveCommand.CreateFromTask<UserModel>(AcceptFriendRequestAsync);
                Log("ChatListModel", "Constructor: AcceptFriendRequestCommand created");
                
                RejectFriendRequestCommand = ReactiveCommand.CreateFromTask<UserModel>(RejectFriendRequestAsync);
                Log("ChatListModel", "Constructor: RejectFriendRequestCommand created");
                
                Log("ChatListModel", "Constructor: All commands created successfully");
            }
            catch (Exception cmdEx)
            {
                Log("ChatListModel", $"Constructor: Error creating commands: {cmdEx.Message}");
                Log("ChatListModel", $"Constructor: Command creation StackTrace: {cmdEx.StackTrace}");
                throw;
            }

            Log("ChatListModel", "Constructor: Initializing settings and groups...");
            InitializeSettingsOptions();
            InitializeGroups();
            Log("ChatListModel", "Constructor: Settings and Groups initialized");

            Log("ChatListModel", "Constructor: Executing RefreshCommand...");
            RefreshCommand.Execute().Subscribe();
            Log("ChatListModel", "Constructor: RefreshCommand executed");
            
            // Load pending friend requests
            Log("ChatListModel", "Constructor: Loading pending friend requests...");
            _ = LoadPendingFriendRequestsAsync();
            Log("ChatListModel", "Constructor: Completed successfully");
        }
        catch (Exception ex)
        {
            Log("ChatListModel", $"Constructor CRITICAL ERROR: {ex.Message}");
            Log("ChatListModel", $"Constructor StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Log("ChatListModel", $"Constructor InnerException: {ex.InnerException.Message}");
            }
            throw; // Re-throw to see the error
        }
    }
    
    private async Task LoadPendingFriendRequestsAsync()
    {
        try
        {
            if (_chatService == null) return;
            
            var requests = await _chatService.GetPendingFriendRequestsAsync(_loginResponse.currentUserId);
            if (requests == null) return;
            
            foreach (var request in requests)
            {
                // Check if requester is already in RecentContacts
                var contact = RecentContacts.FirstOrDefault(c => c.Id == request.RequesterId);
                if (contact == null)
                {
                    // Add as a new contact with pending request
                    contact = new UserModel
                    {
                        Id = request.RequesterId,
                        Username = request.RequesterUsername,
                        AvatarInitials = BuildInitials(request.RequesterDisplayName),
                        StatusMessage = "待处理的好友请求",
                        HasPendingRequest = true,
                        PendingRequestId = request.Id,
                        ButtonCommand = new RelayCommand(OnFriendSelected),
                    };
                    contact.ProfileCommand = new RelayCommand(_ => OpenFriendProfile(contact));
                    contact.AcceptFriendRequestCommand = new RelayCommand(_ => AcceptFriendRequestAsync(contact));
                    contact.RejectFriendRequestCommand = new RelayCommand(_ => RejectFriendRequestAsync(contact));
                    RecentContacts.Add(contact);
                }
                else
                {
                    contact.HasPendingRequest = true;
                    contact.PendingRequestId = request.Id;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoadPendingFriendRequestsAsync failed: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
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

            if (friendList == null)
            {
                Console.WriteLine("friendList is null (server may be offline)");
            }
            else
            {
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
                    userModel.AcceptFriendRequestCommand = new RelayCommand(_ => AcceptFriendRequestAsync(userModel));
                    userModel.RejectFriendRequestCommand = new RelayCommand(_ => RejectFriendRequestAsync(userModel));
                    RecentContacts.Add(userModel);
                }
            }

            var recentMessages = await _chatService.GetRecentMessages(_loginResponse.currentUserId);
            if (recentMessages == null)
            {
                Console.WriteLine("recentMessages is null (server may be offline)");
            }
            else
            {
                foreach (var message in recentMessages)
                {
                    var sender = RecentContacts.FirstOrDefault(u =>
                        u.Id == message.senderId ||
                        (message.receiverId.HasValue && u.Id == message.receiverId.Value));
                    if (sender is null)
                    {
                        continue;
                    }

                    sender.LastMessagePreview = string.IsNullOrWhiteSpace(message.content)
                        ? "图片或附件"
                        : message.content;

                    if (message.receiverId.HasValue && message.receiverId.Value == _loginResponse.currentUserId)
                    {
                        sender.BackgroundColor = "#FF3B2F";
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"RefreshAsync failed: {e.Message}");
            StatusMessage = "离线 | 无法连接服务器";
        }
    }

    private void OnMessageReceived(MessageDto message)
    {
        if (!message.receiverId.HasValue || message.receiverId.Value != _loginResponse.currentUserId)
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
        try
        {
            Log("ChatListModel", $"OnFriendSelected called with parameter: {parameter?.GetType().Name}");
            if (parameter is not UserModel user)
            {
                Log("ChatListModel", "OnFriendSelected: parameter is not UserModel");
                return;
            }

            Log("ChatListModel", $"OnFriendSelected: Starting chat with user {user.Username} (Id: {user.Id})");
            // Use embedded mode instead of navigation
            StartEmbeddedChat(user);
            user.BackgroundColor = "#0078D7";
            user.LastMessagePreview = string.Empty;
            Cleanup();
            Log("ChatListModel", "OnFriendSelected: Completed successfully");
        }
        catch (Exception ex)
        {
            Log("ChatListModel", $"OnFriendSelected ERROR: {ex.Message}");
            Log("ChatListModel", $"StackTrace: {ex.StackTrace}");
        }
    }
    
    // Store current chat info for popout functionality
    private UserModel? _currentEmbeddedChatUser;
    private GroupModel? _currentEmbeddedChatGroup;
    
    private void StartEmbeddedChat(UserModel user)
    {
        try
        {
            Log("ChatListModel", $"StartEmbeddedChat: Creating chat with {user.Username}");
            var contactor = new InContact
            {
                user_id = _loginResponse.currentUserId,
                _oppo_id = user.Id,
                _oppo_name = user.Username
            };

            Log("ChatListModel", "StartEmbeddedChat: Creating ChatViewModel...");
            EmbeddedChatViewModel = new ChatViewModel(_loginResponse, contactor, Router);
            Log("ChatListModel", "StartEmbeddedChat: ChatViewModel created");
            
            // Store user info for popout functionality
            _currentEmbeddedChatUser = user;
            _currentEmbeddedChatGroup = null;
            
            // Set RightPanelContent directly - don't clear first, as that might prevent DataTemplate lookup
            Log("ChatListModel", "StartEmbeddedChat: Setting RightPanelContent to new ChatViewModel...");
            RightPanelContent = EmbeddedChatViewModel;
            Log("ChatListModel", $"StartEmbeddedChat: RightPanelContent set, type: {RightPanelContent?.GetType().Name}, value is null: {RightPanelContent == null}");
            
            // Force immediate UI update on UI thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                this.RaisePropertyChanged(nameof(RightPanelContent));
            }, Avalonia.Threading.DispatcherPriority.Send);
            
            IsEmbeddedChatMode = true;
            Log("ChatListModel", $"StartEmbeddedChat: IsEmbeddedChatMode set to true, current value: {IsEmbeddedChatMode}");
            
            Log("ChatListModel", "StartEmbeddedChat: Completed successfully");
        }
        catch (Exception ex)
        {
            Log("ChatListModel", $"StartEmbeddedChat ERROR: {ex.Message}");
            Log("ChatListModel", $"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Log("ChatListModel", $"InnerException: {ex.InnerException.Message}");
            }
            throw; // Re-throw to see the error
        }
    }
    
    private void ShowSearchFriends()
    {
        Log("ChatListModel", "ShowSearchFriends: Method entry point reached");
        try
        {
            Log("ChatListModel", "ShowSearchFriends: Inside try block");
            if (_chatService == null)
            {
                Log("ChatListModel", "ShowSearchFriends ERROR: _chatService is null");
                return;
            }
            
            Log("ChatListModel", $"ShowSearchFriends: _chatService is not null, currentUserId={_loginResponse.currentUserId}");
            
            Log("ChatListModel", $"ShowSearchFriends: About to create SearchFriendsViewModel...");
            var searchViewModel = new SearchFriendsViewModel(_loginResponse.currentUserId, _chatService);
            Log("ChatListModel", "ShowSearchFriends: SearchFriendsViewModel created successfully");
            
            // Set RightPanelContent directly
            Log("ChatListModel", "ShowSearchFriends: Setting RightPanelContent to SearchFriendsViewModel...");
            RightPanelContent = searchViewModel;
            Log("ChatListModel", $"ShowSearchFriends: RightPanelContent set, type: {RightPanelContent?.GetType().Name}, value is null: {RightPanelContent == null}");
            
            // Force immediate UI update on UI thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                this.RaisePropertyChanged(nameof(RightPanelContent));
                Log("ChatListModel", "ShowSearchFriends: Property change notification sent");
            }, Avalonia.Threading.DispatcherPriority.Send);
            
            IsEmbeddedChatMode = false;
            Log("ChatListModel", $"ShowSearchFriends: IsEmbeddedChatMode set to false, current value: {IsEmbeddedChatMode}");
            
            Log("ChatListModel", "ShowSearchFriends: Completed successfully");
        }
        catch (Exception ex)
        {
            Log("ChatListModel", $"ShowSearchFriends CRITICAL ERROR: {ex.Message}");
            Log("ChatListModel", $"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Log("ChatListModel", $"InnerException: {ex.InnerException.Message}");
                Log("ChatListModel", $"InnerException StackTrace: {ex.InnerException.StackTrace}");
            }
            // Don't re-throw to prevent app crash, but log the error
        }
    }
    
    private void PopoutChat()
    {
        try
        {
            Log("ChatListModel", "PopoutChat: Starting popout");
            
            // Create a new ChatViewModel instance for the popout window
            // Don't reuse EmbeddedChatViewModel to avoid conflicts
            if (_currentEmbeddedChatUser != null)
            {
                Log("ChatListModel", $"PopoutChat: Creating new ChatViewModel for user {_currentEmbeddedChatUser.Username}");
                var contactor = new InContact
                {
                    user_id = _loginResponse.currentUserId,
                    _oppo_id = _currentEmbeddedChatUser.Id,
                    _oppo_name = _currentEmbeddedChatUser.Username
                };
                
                var popoutViewModel = new ChatViewModel(_loginResponse, contactor, Router);
                Router.Navigate.Execute(popoutViewModel);
                Log("ChatListModel", "PopoutChat: Navigated to new ChatViewModel window");
            }
            else if (_currentEmbeddedChatGroup != null)
            {
                Log("ChatListModel", $"PopoutChat: Creating new ChatViewModel for group {_currentEmbeddedChatGroup.Name}");
                var popoutViewModel = new ChatViewModel(_loginResponse, _currentEmbeddedChatGroup.Id, _currentEmbeddedChatGroup.Name, Router);
                Router.Navigate.Execute(popoutViewModel);
                Log("ChatListModel", "PopoutChat: Navigated to new ChatViewModel window");
            }
            else
            {
                Log("ChatListModel", "PopoutChat: No current chat user or group to popout");
                return;
            }
            
            // Keep embedded mode - don't clear it, so user can continue chatting in embedded window
            // The popout window is a separate instance
            Log("ChatListModel", "PopoutChat: Completed successfully (embedded window remains)");
        }
        catch (Exception ex)
        {
            Log("ChatListModel", $"PopoutChat ERROR: {ex.Message}");
            Log("ChatListModel", $"StackTrace: {ex.StackTrace}");
        }
    }
    
    private async Task RefreshRightPanelAsync()
    {
        try
        {
            Log("ChatListModel", "RefreshRightPanel: Starting refresh");
            
            // Save current content
            var currentContent = RightPanelContent;
            var currentEmbeddedChat = EmbeddedChatViewModel;
            var wasEmbeddedMode = IsEmbeddedChatMode;
            
            if (currentContent == null)
            {
                Log("ChatListModel", "RefreshRightPanel: No content to refresh");
                return;
            }
            
            Log("ChatListModel", $"RefreshRightPanel: Current content type: {currentContent.GetType().Name}");
            
            // Clear content to force UI update
            RightPanelContent = null;
            IsEmbeddedChatMode = false;
            
            // Force property change notifications for clearing
            this.RaisePropertyChanged(nameof(RightPanelContent));
            this.RaisePropertyChanged(nameof(IsEmbeddedChatMode));
            
            // Wait a bit to let UI process the clear
            await Task.Delay(50);
            
            // Use Dispatcher to ensure UI thread updates
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Restore content to force refresh
                RightPanelContent = currentContent;
                IsEmbeddedChatMode = wasEmbeddedMode;
                
                // Force property change notifications
                this.RaisePropertyChanged(nameof(RightPanelContent));
                this.RaisePropertyChanged(nameof(IsEmbeddedChatMode));
                
                Log("ChatListModel", "RefreshRightPanel: Content restored and properties notified");
            }, Avalonia.Threading.DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            Log("ChatListModel", $"RefreshRightPanel ERROR: {ex.Message}");
            Log("ChatListModel", $"StackTrace: {ex.StackTrace}");
        }
    }
    
    private async Task AcceptFriendRequestAsync(UserModel user)
    {
        if (user.PendingRequestId == null) return;
        
        try
        {
            var dto = new RespondToFriendRequestDto
            {
                RequestId = user.PendingRequestId.Value,
                UserId = _loginResponse.currentUserId,
                Accept = true
            };
            
            var success = await _chatService.RespondToFriendRequestAsync(dto);
            if (success)
            {
                user.HasPendingRequest = false;
                user.PendingRequestId = null;
                // Refresh friend list
                await RefreshAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AcceptFriendRequestAsync failed: {ex.Message}");
        }
    }
    
    private async Task RejectFriendRequestAsync(UserModel user)
    {
        if (user.PendingRequestId == null) return;
        
        try
        {
            var dto = new RespondToFriendRequestDto
            {
                RequestId = user.PendingRequestId.Value,
                UserId = _loginResponse.currentUserId,
                Accept = false
            };
            
            var success = await _chatService.RespondToFriendRequestAsync(dto);
            if (success)
            {
                user.HasPendingRequest = false;
                user.PendingRequestId = null;
                // Remove from list if not a friend
                if (!RecentContacts.Any(c => c.Id == user.Id && !c.HasPendingRequest))
                {
                    RecentContacts.Remove(user);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RejectFriendRequestAsync failed: {ex.Message}");
        }
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
        userModel.AcceptFriendRequestCommand = new RelayCommand(_ => AcceptFriendRequestAsync(userModel));
        userModel.RejectFriendRequestCommand = new RelayCommand(_ => RejectFriendRequestAsync(userModel));

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
        _ = LoadGroupsAsync();
    }

    private async Task LoadGroupsAsync()
    {
        try
        {
            Groups.Clear();
            var groups = await _chatService.GetGroups();
            foreach (var g in groups)
            {
                var gm = new GroupModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = "群聊",
                    MemberCount = 0
                };
                // 先完成对象初始化，再绑定命令以避免“在声明之前使用变量”错误
                gm.OpenCommand = new RelayCommand(_ => NavigateToGroupChat(gm));
                Groups.Add(gm);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoadGroups failed: {ex.Message}");
        }
    }

    private void NavigateToGroupChat(GroupModel group)
    {
        try
        {
            Log("ChatListModel", $"NavigateToGroupChat: Creating embedded chat for group {group.Name}");
            // Use embedded mode for group chat too
            EmbeddedChatViewModel = new ChatViewModel(_loginResponse, group.Id, group.Name, Router);
            
            // Store group info for popout functionality
            _currentEmbeddedChatGroup = group;
            _currentEmbeddedChatUser = null;
            
            RightPanelContent = EmbeddedChatViewModel;
            IsEmbeddedChatMode = true;
            Log("ChatListModel", "NavigateToGroupChat: Completed successfully");
        }
        catch (Exception ex)
        {
            Log("ChatListModel", $"NavigateToGroupChat ERROR: {ex.Message}");
            Log("ChatListModel", $"StackTrace: {ex.StackTrace}");
        }
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

    private async Task CreateGroupAsync()
    {
        try
        {
            Log("ChatListModel", "CreateGroup: Starting group creation");
            
            // TODO: Show dialog to input group name
            // For now, create a group with timestamp
            var groupName = $"新群聊 {DateTime.Now:MM-dd HH:mm}";
            Log("ChatListModel", $"CreateGroup: Creating group with name: {groupName}");
            
            var group = await _chatService.CreateGroupAsync(groupName, _loginResponse.currentUserId);
            if (group != null)
            {
                Log("ChatListModel", $"CreateGroup: Group created successfully: {group.Name} (Id: {group.Id})");
                
                // Refresh groups list
                await LoadGroupsAsync();
                Log("ChatListModel", "CreateGroup: Groups list refreshed");
                
                // Navigate to the new group chat in embedded mode
                var groupModel = Groups.FirstOrDefault(g => g.Id == group.Id);
                if (groupModel != null)
                {
                    Log("ChatListModel", "CreateGroup: Navigating to new group chat");
                    NavigateToGroupChat(groupModel);
                }
                else
                {
                    Log("ChatListModel", "CreateGroup: Warning - group not found in Groups list after refresh");
                }
            }
            else
            {
                Log("ChatListModel", "CreateGroup: Failed to create group (API returned null)");
            }
        }
        catch (Exception ex)
        {
            Log("ChatListModel", $"CreateGroup ERROR: {ex.Message}");
            Log("ChatListModel", $"StackTrace: {ex.StackTrace}");
        }
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
                // 在 UI 线程上更新位图，避免首屏渲染异常
                try
                {
                    var bmp = new Bitmap(new System.IO.MemoryStream(bytes));
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => UserAvatar = bmp);
                }
                catch
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => UserAvatar = null);
                }
                try { await AvatarCache.SaveAsync(_loginResponse.currentUserId, bytes); } catch { }
            }
            else
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => UserAvatar = null);
            }
        }
        catch { Avalonia.Threading.Dispatcher.UIThread.Post(() => UserAvatar = null); }
    }
}
