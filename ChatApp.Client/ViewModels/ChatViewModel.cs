using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Shared.MessageTypes;
using ChatApp.Client.Models;
using Avalonia.Controls.Notifications;
using System.Reactive.Linq;
using System.Reactive;
using ChatApp.Client.DTOs;
using System.Net.Http;
using Shared.Models;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ChatApp.Client.Helpers;
using Splat;
using Avalonia.Threading;

namespace ChatApp.Client.ViewModels
{
    //聊天视图的视图模型，负责管理 UI 和业务逻辑的交互
    public class ChatViewModel : ViewModelBase
    {
        private readonly IChatService _chatService;
        private readonly IHubService _hubService;
        private ObservableCollection<MessageDto> _messages;
        private ObservableCollection<MessageDto> _newMessages;
        private string _messageContent;
        private Guid _currentUserId;
        private Guid _currentChatId;
        private Guid _currentGroupId;
        private string _oppositeUserName;
        private LoginResponse _loginResponse;
        private bool _isRead;
        private bool _disposed = false;
        private bool _isGroupChat = false;
        // 缓存用户显示名，减少重复请求
        private readonly Dictionary<Guid, string> _displayNameCache = new();
        // 缓存用户头像位图，避免重复解码/请求
        private readonly Dictionary<Guid, Bitmap?> _avatarBitmapCache = new();
        
       
        
        // ObservableCollection用来绑定消息列表,Messages表示当前聊天的所有消息
        public ObservableCollection<MessageDto> Messages
        {
            //get => _messages;
            get => _messages ?? (_messages = new ObservableCollection<MessageDto>());
            set => SetProperty<ObservableCollection<MessageDto>>(ref _messages, value);
        }
        
        // 绑定到TextBox的MessageContent
        public string MessageContent
        {
            get => _messageContent;
            set => SetProperty<string>(ref _messageContent, value);
        }
        
        public bool isRead
        {
            get => _isRead;
            set => SetProperty<bool>(ref _isRead, value);
        }

        public string OppositeUserName
        {
            get => _oppositeUserName;
            set => this.RaiseAndSetIfChanged(ref _oppositeUserName, value);
        }
        
        // 命令
        public ICommand DictateMessageCommand { get; private set; }

        public ICommand AttachImageCommand { get; private set; }

        public ICommand SendMessageCommand { get; private set; }
        
        public ICommand ReturnToChatListCommand { get; private set; }

        public  ChatViewModel(LoginResponse loginResponse, InContact contactor, RoutingState router) : base(router)
        {
            _loginResponse = loginResponse;
            _hubService = Locator.Current.GetService<IHubService>();
            _hubService.MessageReceived += OnMessageReceived;
            // 原来：_ = _hubService.ConnectAsync(loginResponse.currentUserId);
    
            _chatService = new ChatService(new HttpClient { BaseAddress = new Uri("http://localhost:5005") });
    
            _currentChatId = contactor._oppo_id;
            _currentUserId = contactor.user_id;
            _oppositeUserName = contactor._oppo_name;   
    
            // 拉取历史消息之前，先顺序完成连接
            _ = InitPrivateAsync();
    
            // 判断是否能发送消息
            canSendMessage = this.WhenAnyValue(x => x.MessageContent).Select(x => !string.IsNullOrEmpty(x));
    
            // 创建命令
            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessageAsync);
            AttachImageCommand = ReactiveCommand.CreateFromTask(AttachImage);
            ReturnToChatListCommand = ReactiveCommand.CreateFromTask(ReturnToChatList);

            // 捕获命令异常，避免 ReactiveUI 管道未处理错误导致崩溃
            if (SendMessageCommand is ReactiveCommand<Unit, Unit> sendCmd)
            {
                sendCmd.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine($"SendMessageCommand error: {ex.Message}");
                });
            }
            if (AttachImageCommand is ReactiveCommand<Unit, Unit> attachCmd)
            {
                attachCmd.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine($"AttachImageCommand error: {ex.Message}");
                });
            }
            if (ReturnToChatListCommand is ReactiveCommand<Unit, Unit> returnCmd)
            {
                returnCmd.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine($"ReturnToChatListCommand error: {ex.Message}");
                });
            }
        }

        // 加载并缓存用户头像位图：优先本地缓存，其次服务端；失败则返回 null
        private async Task<Bitmap?> LoadAvatarBitmapAsync(Guid userId)
        {
            if (_avatarBitmapCache.TryGetValue(userId, out var cached))
            {
                return cached;
            }

            try
            {
                var bytes = await AvatarCache.TryLoadAsync(userId) ?? await _chatService.GetAvatar(userId);
                if (bytes != null)
                {
                    var bmp = new Bitmap(new System.IO.MemoryStream(bytes));
                    _avatarBitmapCache[userId] = bmp;
                    try { await AvatarCache.SaveAsync(userId, bytes); } catch { }
                    return bmp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadAvatarBitmapAsync failed: {ex.Message}");
            }

            _avatarBitmapCache[userId] = null;
            return null;
        }

        // 群聊构造函数
        public ChatViewModel(LoginResponse loginResponse, Guid groupId, string groupName, RoutingState router) : base(router)
        {
            _loginResponse = loginResponse;
            _hubService = Locator.Current.GetService<IHubService>();
            _hubService.GroupMessageReceived += OnGroupMessageReceived;
            // 原来：_ = _hubService.ConnectAsync(loginResponse.currentUserId);
    
            _chatService = new ChatService(new HttpClient { BaseAddress = new Uri("http://localhost:5005") });
    
            _currentUserId = loginResponse.currentUserId;
            _currentGroupId = groupId;
            _oppositeUserName = groupName;
            _isGroupChat = true;
    
            // 加入群组并拉取历史消息（顺序初始化）
            _ = InitGroupAsync(groupId);
    
            canSendMessage = this.WhenAnyValue(x => x.MessageContent).Select(x => !string.IsNullOrEmpty(x));
            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessageAsync);
            AttachImageCommand = ReactiveCommand.CreateFromTask(AttachImage);
            ReturnToChatListCommand = ReactiveCommand.CreateFromTask(ReturnToChatList);

            // 捕获命令异常，避免 ReactiveUI 管道未处理错误导致崩溃
            if (SendMessageCommand is ReactiveCommand<Unit, Unit> sendCmd)
            {
                sendCmd.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine($"SendMessageCommand error: {ex.Message}");
                });
            }
            if (AttachImageCommand is ReactiveCommand<Unit, Unit> attachCmd)
            {
                attachCmd.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine($"AttachImageCommand error: {ex.Message}");
                });
            }
            if (ReturnToChatListCommand is ReactiveCommand<Unit, Unit> returnCmd)
            {
                returnCmd.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine($"ReturnToChatListCommand error: {ex.Message}");
                });
            }
        }
        
        
        ~ChatViewModel()
        {
            Dispose(false);
        }

        
        // 加载最近的聊天记录
        private async void LoadMessages()
        {
            try
            {
                List<MessageDto> messages;
                if (_isGroupChat)
                {
                    messages = await _chatService.GetGroupMessages(_currentGroupId);
                }
                else
                {
                    // 使用 GetPrivateMessages 从后端获取历史消息
                    messages = await _chatService.GetPrivateMessages(_currentChatId, _currentUserId);
                }

                // 将历史消息合并到 MessageHistory 中
                foreach (var message in messages)
                {
                    // 设置每条消息的角色
                    SetMessageRole(message);

                    // 如果是图片消息，尝试下载位图用于显示
                    if (!string.IsNullOrWhiteSpace(message.attachmentUrl) && message.attachmentImage == null)
                    {
                        try
                        {
                            var bmp = await _chatService.GetImageBitmapAsync(message.attachmentUrl);
                            message.attachmentImage = bmp;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"LoadMessages image fetch failed: {ex.Message}");
                        }
                    }

                    // 填充发送者昵称（群聊与私聊都统一设置，确保 UI 始终显示昵称而不是ID）
                    message.senderName = await ResolveDisplayName(message.senderId);
                    message.senderAvatar = await LoadAvatarBitmapAsync(message.senderId);

                    Dispatcher.UIThread.Post(() => Messages.Add(message));
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading messages: " + e.Message);
            }
        }
        
        
        private async void PostunreadMessages(List<MessageDto> postmessage)
        {
            try
            {
                foreach (var message in postmessage)
                {
                    await _chatService.PostMessageToDb(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        //根据id设置ChatRoleType
        //message实例化MessageDto
        private void SetMessageRole(MessageDto message)
        {
            message.ChatRoleType = message.senderId == _currentUserId ? ChatRoleType.Sender : ChatRoleType.Receiver;
        }
        
        // 发送消息
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(MessageContent)) return;
            
            // Create the message and add it immediately to the collection
            var message = new MessageDto
            {
                senderId = _currentUserId,
                receiverId = _isGroupChat ? (Guid?)null : _currentChatId,
                groupId = _isGroupChat ? _currentGroupId : null,
                content = MessageContent,
                timestamp = DateTime.UtcNow,
                ChatRoleType = ChatRoleType.Receiver, // 默认设置为Receiver
                IsRead = _isRead //默认未读
            };
    
            // 调用方法设置消息的角色
            SetMessageRole(message);
            
            // 私聊：立刻本地添加；群聊：等待服务器广播避免重复
            if (!_isGroupChat)
            {
                // 私聊：也为本地消息设置昵称，避免显示为ID
                try
                {
                    message.senderName = await ResolveDisplayName(_currentUserId);
                    message.senderAvatar = await LoadAvatarBitmapAsync(_currentUserId);
                }
                catch { /* 忽略昵称解析失败 */ }
                // 在UI线程添加，保持与自动滚动一致
                Dispatcher.UIThread.Post(() => Messages.Add(message));
            }
            //await _chatService.PostreadMessageToDb(message);
            
            // Send the message via SignalR
            try
            {
                if (_isGroupChat)
                {
                    if (_currentGroupId == Guid.Empty)
                    {
                        Console.WriteLine("当前群组ID无效，无法发送群聊消息");
                        return;
                    }
                    Console.WriteLine("Sending group: " + MessageContent + " to: " + _currentGroupId);
                    await _hubService.SendGroupMessageAsync(_currentUserId, _currentGroupId, MessageContent);
                }
                else
                {
                    Console.WriteLine("Sending: " + MessageContent + " to: " + _currentChatId);
                    await _hubService.SendPrivateMessageAsync(_currentUserId, _currentChatId, MessageContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessageAsync error: {ex.Message}");
            }

            // Clear the input field after sending
            MessageContent = string.Empty;
        }

        // 选择图片并上传，随后发送带附件URL的消息
        private async Task AttachImage()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    AllowMultiple = false,
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "Images", Extensions = new List<string>{ "png","jpg","jpeg","gif","webp" } }
                    }
                };

                var window = (App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                if (window == null) return;
                var files = await dialog.ShowAsync(window);
                var path = files?.FirstOrDefault();
                if (string.IsNullOrEmpty(path)) return;

                var url = await _chatService.UploadImageAsync(path);
                if (string.IsNullOrEmpty(url))
                {
                    Console.WriteLine("上传图片失败");
                    return;
                }

                var message = new MessageDto
                {
                    senderId = _currentUserId,
                    receiverId = _isGroupChat ? (Guid?)null : _currentChatId,
                    groupId = _isGroupChat ? _currentGroupId : null,
                    content = string.Empty,
                    attachmentUrl = url,
                    timestamp = DateTime.UtcNow,
                    ChatRoleType = ChatRoleType.Receiver,
                    IsRead = _isRead
                };
                // 下载图片为Bitmap以确保UI正常显示
                var bmp = await _chatService.GetImageBitmapAsync(url);
                message.attachmentImage = bmp;
                // 为本地消息补充昵称与头像
                try
                {
                    message.senderName = await ResolveDisplayName(_currentUserId);
                    message.senderAvatar = await LoadAvatarBitmapAsync(_currentUserId);
                }
                catch { }
                SetMessageRole(message);
                if (!_isGroupChat)
                {
                    Dispatcher.UIThread.Post(() => Messages.Add(message));
                }
                if (_isGroupChat)
                {
                    await _hubService.SendGroupAttachmentMessageAsync(_currentUserId, _currentGroupId, url, null);
                }
                else
                {
                    await _hubService.SendPrivateAttachmentMessageAsync(_currentUserId, _currentChatId, url, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AttachImage error: {ex.Message}");
            }
        }
        
        // 处理接收到的消息
        private async void OnMessageReceived(MessageDto message)
        {
            // 如果接收到的消息是当前聊天用户的消息，添加到消息列表
            
            if (message.senderId == _currentChatId && message.receiverId.HasValue && message.receiverId.Value == _currentUserId)
            {
                // 根据 senderId 设置 ChatRoleType
                //Console.WriteLine($"received message: {message.content},senderId: {message.senderId},receiverId: {message.receiverId}");
                Console.WriteLine($"Correct View Received messsage: {message.content},id:{message.id}");
                SetMessageRole(message);

                // 如果是图片消息且尚未有位图，下载位图以供 UI 显示
                if (!string.IsNullOrWhiteSpace(message.attachmentUrl) && message.attachmentImage == null)
                {
                    try
                    {
                        var bmp = await _chatService.GetImageBitmapAsync(message.attachmentUrl);
                        message.attachmentImage = bmp;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"OnMessageReceived image fetch failed: {ex.Message}");
                    }
                }
                // 私聊也填充昵称，保持显示一致性
                message.senderName = await ResolveDisplayName(message.senderId);
                message.senderAvatar = await LoadAvatarBitmapAsync(message.senderId);
                Messages.Add(message);
            }
            else
            {
                Console.WriteLine($"Not this View but Received messsage: {message.content},id:{message.id}");
                _hubService.SetMessageToUnread(message);
                //todo : set message unread
            }
        }

        // 处理接收到的群聊消息
        private async void OnGroupMessageReceived(MessageDto message)
        {
            if (!_isGroupChat) return;
            if (message.groupId != _currentGroupId) return;

            Console.WriteLine($"Group message received: {message.content}, id:{message.id}");
            SetMessageRole(message);

            // 群聊：填充发送者昵称方便展示
            message.senderName = await ResolveDisplayName(message.senderId);
            message.senderAvatar = await LoadAvatarBitmapAsync(message.senderId);

            if (!string.IsNullOrWhiteSpace(message.attachmentUrl) && message.attachmentImage == null)
            {
                try
                {
                    var bmp = await _chatService.GetImageBitmapAsync(message.attachmentUrl);
                    message.attachmentImage = bmp;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnGroupMessageReceived image fetch failed: {ex.Message}");
                }
            }
            // 确保在UI线程添加以触发滚动
            Dispatcher.UIThread.Post(() => Messages.Add(message));
        }
        
       
        
        // private async Disconnect()
        // {
        //     await _hubService.DisconnectAsync();
        // }

        private async Task ReturnToChatList()
        {
            try
            {
                //这里可以加上任何退出当前聊天的操作，比如断开连接等。
                
                // foreach (var kvp in _chatmessages)
                // {
                //     if (kvp.Key != _currentChatId)
                //     {
                //         PostunreadMessages(kvp.Value.ToList());
                //     }
                // }
                
                
                await _hubService.DisconnectAsync();
            
                // 使用Router导航到 ChatListModel 页面
                Router.Navigate.Execute(new ChatListModel(_loginResponse, Router));
                Dispose();
            }
            catch (Exception e)
            {
                // 如果出现异常，输出错误信息
                Console.WriteLine(e);
                throw;
            }
        }

        private void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 解除事件绑定
                _hubService.MessageReceived -= OnMessageReceived;
                _hubService.GroupMessageReceived -= OnGroupMessageReceived;
            }

            _disposed = true;
        }
        
        
        //Fields
        private ChatService chatService;
        private string newMessageContent;
        private WindowNotificationManager windowNotificationManager;
        private IObservable<bool> canSendMessage;
        // 新增：私聊初始化（确保先连接再加载）
        private async Task InitPrivateAsync()
        {
            try
            {
                await _hubService.ConnectAsync(_currentUserId);
                LoadMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InitPrivateAsync failed: {ex.Message}");
            }
        }

        // 新增：群聊初始化（确保先连接，再加入群，再加载）
        private async Task InitGroupAsync(Guid groupId)
        {
            try
            {
                await _hubService.ConnectAsync(_currentUserId);
                await _hubService.JoinGroupAsync(groupId);
                LoadMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InitGroupAsync failed: {ex.Message}");
            }
        }

        // 解析并缓存用户显示名：优先使用非空的 DisplayName，其次 Username；回退为ID时不缓存，避免缓存污染
        private async Task<string?> ResolveDisplayName(Guid userId)
        {
            if (_displayNameCache.TryGetValue(userId, out var cached))
            {
                return cached;
            }
            try
            {
                var profile = await _chatService.GetProfile(userId);
                string? name = null;

                if (profile != null)
                {
                    var display = (profile.DisplayName ?? string.Empty).Trim();
                    var username = (profile.Username ?? string.Empty).Trim();
                    name = !string.IsNullOrWhiteSpace(display)
                        ? display
                        : (!string.IsNullOrWhiteSpace(username) ? username : null);
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    // 回退为ID但不缓存，避免以后无法刷新昵称
                    var fallbackId = userId.ToString();
                    return fallbackId;
                }

                _displayNameCache[userId] = name;
                return name;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResolveDisplayName failed: {ex.Message}");
                // 异常时返回ID但不缓存，让后续重试有机会拿到昵称
                var fallback = userId.ToString();
                return fallback;
            }
        }
    }
}
