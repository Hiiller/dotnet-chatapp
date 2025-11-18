using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    public sealed class EmojiOption
    {
        public EmojiOption(string assetName, Bitmap preview)
        {
            AssetName = assetName;
            Preview = preview;
        }

        public string AssetName { get; }
        public Bitmap Preview { get; }
    }

    //閼卞﹤銇夌憴鍡楁禈閻ㄥ嫯顫嬮崶鐐侀崹瀣剁礉鐠愮喕鐭楃粻锛勬倞 UI 閸滃奔绗熼崝锟犫偓鏄忕帆閻ㄥ嫪姘︽禍?
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
        private bool _isGroupChat;
        private string _currentGroupCode = string.Empty;
        private string? _currentGroupRole;
        // 缂傛挸鐡ㄩ悽銊﹀煕閺勫墽銇氶崥宥忕礉閸戝繐鐨柌宥咁槻鐠囬攱鐪?
        private readonly Dictionary<Guid, string> _displayNameCache = new();
        // 缂傛挸鐡ㄩ悽銊﹀煕婢舵潙鍎氭担宥呮禈閿涘矂浼╅崗宥夊櫢婢跺秷袙閻?鐠囬攱鐪?
        private readonly Dictionary<Guid, Bitmap?> _avatarBitmapCache = new();
        private readonly List<EmojiOption> _emojiPalette = new();
        
       
        
        // ObservableCollection閻劍娼电紒鎴濈暰濞戝牊浼呴崚妤勩€?Messages鐞涖劎銇氳ぐ鎾冲閼卞﹤銇夐惃鍕閺堝绉烽幁?
        public ObservableCollection<MessageDto> Messages
        {
            //get => _messages;
            get => _messages ?? (_messages = new ObservableCollection<MessageDto>());
            set => SetProperty<ObservableCollection<MessageDto>>(ref _messages, value);
        }

        public IReadOnlyList<EmojiOption> EmojiOptions { get; private set; } = Array.Empty<EmojiOption>();
        
        // 缂佹垵鐣鹃崚鐧焑xtBox閻ㄥ嚜essageContent
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

        public bool IsGroupChat
        {
            get => _isGroupChat;
            private set => SetProperty(ref _isGroupChat, value);
        }
        
        public ReactiveCommand<Unit, Unit>? OpenGroupDetailsCommand { get; private set; }

        public string OppositeUserName
        {
            get => _oppositeUserName;
            set => this.RaiseAndSetIfChanged(ref _oppositeUserName, value);
        }
        
        // 閸涙垝鎶?
        public ICommand DictateMessageCommand { get; private set; }

        public ICommand AttachImageCommand { get; private set; }
        public ICommand SendEmojiCommand { get; private set; }
        public ICommand UploadEmojiCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        
        public ICommand ReturnToChatListCommand { get; private set; }

        public  ChatViewModel(LoginResponse loginResponse, InContact contactor, RoutingState router, IChatService? chatService = null) : base(router)
        {
            _loginResponse = loginResponse;
            _hubService = Locator.Current.GetService<IHubService>();
            _hubService.MessageReceived += OnMessageReceived;
            // 閸樼喐娼甸敍姝?= _hubService.ConnectAsync(loginResponse.currentUserId);
    
            _chatService = chatService ?? new ChatService(new HttpClient { BaseAddress = new Uri("http://localhost:5005") });
    
            _currentChatId = contactor._oppo_id;
            _currentUserId = contactor.user_id;
            _oppositeUserName = contactor._oppo_name;   
    
            // 閹峰褰囬崢鍡楀蕉濞戝牊浼呮稊瀣閿涘苯鍘涙い鍝勭碍鐎瑰本鍨氭潻鐐村复
            _ = InitPrivateAsync();
    
            // 閸掋倖鏌囬弰顖氭儊閼宠棄褰傞柅浣圭Х閹?
            canSendMessage = this.WhenAnyValue(x => x.MessageContent).Select(x => !string.IsNullOrEmpty(x));
    
            // 閸掓稑缂撻崨鎴掓姢
            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessageAsync);
            AttachImageCommand = ReactiveCommand.CreateFromTask(AttachImage);
            ReturnToChatListCommand = ReactiveCommand.CreateFromTask(ReturnToChatList);

            // 閹规洝骞忛崨鎴掓姢瀵倸鐖堕敍宀勪缉閸?ReactiveUI 缁狅繝浜鹃張顏勵槱閻炲棝鏁婄拠顖氼嚤閼锋潙绌垮┃?
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

            InitializeEmojiSupport();
            IsGroupChat = false;
        }

        // 閸旂姾娴囬獮鍓佺处鐎涙鏁ら幋宄般仈閸嶅繋缍呴崶鎾呯窗娴兼ê鍘涢張顒€婀寸紓鎾崇摠閿涘苯鍙惧▎鈩冩箛閸旓紕顏敍娑樸亼鐠愩儱鍨潻鏂挎礀 null
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

        // 缂囥倛浜伴弸鍕偓鐘插毐閺?
        public ChatViewModel(LoginResponse loginResponse, Guid groupId, string groupName, RoutingState router, IChatService? chatService = null) : base(router)
        {
            _loginResponse = loginResponse;
            _hubService = Locator.Current.GetService<IHubService>();
            _hubService.GroupMessageReceived += OnGroupMessageReceived;
            // 閸樼喐娼甸敍姝?= _hubService.ConnectAsync(loginResponse.currentUserId);
    
            _chatService = chatService ?? new ChatService(new HttpClient { BaseAddress = new Uri("http://localhost:5005") });
    
            _currentUserId = loginResponse.currentUserId;
            _currentGroupId = groupId;
            _oppositeUserName = groupName;
            IsGroupChat = true;
            OpenGroupDetailsCommand = ReactiveCommand.Create(OpenGroupDetails);
    
            // 閸旂姴鍙嗙紘銈囩矋楠炶埖濯洪崣鏍у坊閸欏弶绉烽幁顖ょ礄妞ゅ搫绨崚婵嗩潗閸栨牭绱?
            _ = InitGroupAsync(groupId);
    
            canSendMessage = this.WhenAnyValue(x => x.MessageContent).Select(x => !string.IsNullOrEmpty(x));
            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessageAsync);
            AttachImageCommand = ReactiveCommand.CreateFromTask(AttachImage);
            ReturnToChatListCommand = ReactiveCommand.CreateFromTask(ReturnToChatList);

            // 閹规洝骞忛崨鎴掓姢瀵倸鐖堕敍宀勪缉閸?ReactiveUI 缁狅繝浜鹃張顏勵槱閻炲棝鏁婄拠顖氼嚤閼锋潙绌垮┃?
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

            InitializeEmojiSupport();
        }
        
        public ChatViewModel(LoginResponse loginResponse, GroupModel group, RoutingState router, IChatService? chatService = null)
            : this(loginResponse, group.Id, group.Name, router, chatService)
        {
            _currentGroupCode = group.GroupCode;
            _currentGroupRole = group.MemberRole;
        }

        private void InitializeEmojiSupport()
        {
            DisposeEmojiPalette();
            foreach (var asset in SystemImageProvider.Emojis)
            {
                var bitmap = SystemImageProvider.LoadAssetBitmap(asset);
                if (bitmap != null)
                {
                    _emojiPalette.Add(new EmojiOption(asset, bitmap));
                }
            }

            EmojiOptions = _emojiPalette.AsReadOnly();
            SendEmojiCommand = ReactiveCommand.CreateFromTask<string>(SendEmojiAsync);
            UploadEmojiCommand = ReactiveCommand.CreateFromTask(UploadCustomEmojiAsync);
        }
        
        private void OpenGroupDetails()
        {
            if (!IsGroupChat)
            {
                return;
            }
            
            try
            {
                var model = new GroupModel
                {
                    Id = _currentGroupId,
                    Name = _oppositeUserName,
                    GroupCode = _currentGroupCode,
                    MemberRole = _currentGroupRole
                };
                Router.Navigate.Execute(new GroupDetailsViewModel(model, _loginResponse.currentUserId, Router, _chatService));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenGroupDetails error: {ex.Message}");
            }
        }
        
        ~ChatViewModel()
        {
            Dispose(false);
        }

        
        // 閸旂姾娴囬張鈧潻鎴犳畱閼卞﹤銇夌拋鏉跨秿
        private async void LoadMessages()
        {
            try
            {
                List<MessageDto> messages;
                if (IsGroupChat)
                {
                    messages = await _chatService.GetGroupMessages(_currentGroupId);
                }
                else
                {
                    // 娴ｈ法鏁?GetPrivateMessages 娴犲骸鎮楃粩顖濆箯閸欐牕宸婚崣鍙夌Х閹?
                    messages = await _chatService.GetPrivateMessages(_currentChatId, _currentUserId);
                }

                // 鐏忓棗宸婚崣鍙夌Х閹垰鎮庨獮璺哄煂 MessageHistory 娑?
                foreach (var message in messages)
                {
                    // 鐠佸墽鐤嗗В蹇旀蒋濞戝牊浼呴惃鍕潡閼?
                    SetMessageRole(message);

                    // 婵″倹鐏夐弰顖氭禈閻楀洦绉烽幁顖ょ礉鐏忔繆鐦稉瀣祰娴ｅ秴娴橀悽銊ょ艾閺勫墽銇?
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

                    // 婵夘偄鍘栭崣鎴︹偓浣解偓鍛█缁夊府绱欑紘銈堜喊娑撳海顫嗛懕濠囧厴缂佺喍绔寸拋鍓х枂閿涘瞼鈥樻穱?UI 婵绮撻弰鍓с仛閺勭數袨閼板奔绗夐弰鐤楧閿?
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
        
        //閺嶈宓乮d鐠佸墽鐤咰hatRoleType
        //message鐎圭偘绶ラ崠鏈歟ssageDto
        private void SetMessageRole(MessageDto message)
        {
            message.ChatRoleType = message.senderId == _currentUserId ? ChatRoleType.Sender : ChatRoleType.Receiver;
        }
        
        // 閸欐垿鈧焦绉烽幁?
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(MessageContent)) return;
            
            // Create the message and add it immediately to the collection
            var message = new MessageDto
            {
                senderId = _currentUserId,
                receiverId = IsGroupChat ? (Guid?)null : _currentChatId,
                groupId = IsGroupChat ? _currentGroupId : null,
                content = MessageContent,
                timestamp = DateTime.UtcNow,
                ChatRoleType = ChatRoleType.Receiver, // 姒涙顓荤拋鍓х枂娑撶療eceiver
                IsRead = _isRead //姒涙顓婚張顏囶嚢
            };
    
            // 鐠嬪啰鏁ら弬瑙勭《鐠佸墽鐤嗗☉鍫熶紖閻ㄥ嫯顫楅懝?
            SetMessageRole(message);
            
            // 缁変浇浜伴敍姘辩彌閸掔粯婀伴崷鐗堝潑閸旂媴绱辩紘銈堜喊閿涙氨鐡戝鍛箛閸斺€虫珤楠炴寧鎸遍柆鍨帳闁插秴顦?
            if (!IsGroupChat)
            {
                // 缁変浇浜伴敍姘瘍娑撶儤婀伴崷鐗堢Х閹垵顔曠純顔芥█缁夊府绱濋柆鍨帳閺勫墽銇氭稉绡扗
                try
                {
                    message.senderName = await ResolveDisplayName(_currentUserId);
                    message.senderAvatar = await LoadAvatarBitmapAsync(_currentUserId);
                }
                catch { /* 韫囩晫鏆愰弰鐢敌炵憴锝嗙€芥径杈Е */ }
                // 閸︹晳I缁捐法鈻煎ǎ璇插閿涘奔绻氶幐浣风瑢閼奉亜濮╁姘З娑撯偓閼?
                Dispatcher.UIThread.Post(() => Messages.Add(message));
            }
            //await _chatService.PostreadMessageToDb(message);
            
            // Send the message via SignalR
            try
            {
                if (IsGroupChat)
                {
                    if (_currentGroupId == Guid.Empty)
                    {
                        Console.WriteLine("Current group id is invalid, unable to send group messages.");
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

        // 闁瀚ㄩ崶鍓у楠炴湹绗傛导鐙呯礉闂呭繐鎮楅崣鎴︹偓浣哥敨闂勫嫪娆RL閻ㄥ嫭绉烽幁?
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

                await SendImageMessageFromPathAsync(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AttachImage error: {ex.Message}");
            }
        }

        
        private async Task SendEmojiAsync(string? assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
            {
                return;
            }

            var tempFile = SystemImageProvider.SaveAssetToTemp(assetName);
            if (string.IsNullOrEmpty(tempFile))
            {
                return;
            }

            try
            {
                await SendImageMessageFromPathAsync(tempFile);
            }
            finally
            {
                TryDelete(tempFile);
            }
        }

        private async Task UploadCustomEmojiAsync()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    AllowMultiple = false,
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "Emojis", Extensions = new List<string>{ "png" } }
                    }
                };

                var window = (App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                if (window == null) return;
                var files = await dialog.ShowAsync(window);
                var path = files?.FirstOrDefault();
                if (string.IsNullOrEmpty(path)) return;

                await SendImageMessageFromPathAsync(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UploadCustomEmojiAsync error: {ex.Message}");
            }
        }

        private async Task SendImageMessageFromPathAsync(string path)
        {
            var url = await _chatService.UploadImageAsync(path);
            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("涓婁紶鍥剧墖澶辫触");
                return;
            }

            await SendAttachmentMessageAsync(url);
        }

        private async Task SendAttachmentMessageAsync(string url)
        {
            var message = new MessageDto
            {
                senderId = _currentUserId,
                receiverId = IsGroupChat ? (Guid?)null : _currentChatId,
                groupId = IsGroupChat ? _currentGroupId : null,
                content = string.Empty,
                attachmentUrl = url,
                timestamp = DateTime.UtcNow,
                ChatRoleType = ChatRoleType.Receiver,
                IsRead = _isRead
            };

            var bmp = await _chatService.GetImageBitmapAsync(url);
            message.attachmentImage = bmp;
            try
            {
                message.senderName = await ResolveDisplayName(_currentUserId);
                message.senderAvatar = await LoadAvatarBitmapAsync(_currentUserId);
            }
            catch { }

            SetMessageRole(message);
            if (!IsGroupChat)
            {
                Dispatcher.UIThread.Post(() => Messages.Add(message));
            }

            if (IsGroupChat)
            {
                await _hubService.SendGroupAttachmentMessageAsync(_currentUserId, _currentGroupId, url, null);
            }
            else
            {
                await _hubService.SendPrivateAttachmentMessageAsync(_currentUserId, _currentChatId, url, null);
            }
        }

        private void DisposeEmojiPalette()
        {
            foreach (var option in _emojiPalette)
            {
                option.Preview.Dispose();
            }
            _emojiPalette.Clear();
        }

        private static void TryDelete(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { File.Delete(path); } catch { }
        }

        // 婢跺嫮鎮婇幒銉︽暪閸掓壆娈戝☉鍫熶紖
        private async void OnMessageReceived(MessageDto message)
        {
            // 婵″倹鐏夐幒銉︽暪閸掓壆娈戝☉鍫熶紖閺勵垰缍嬮崜宥堜喊婢垛晝鏁ら幋椋庢畱濞戝牊浼呴敍灞惧潑閸旂姴鍩屽☉鍫熶紖閸掓銆?
            
            if (message.senderId == _currentChatId && message.receiverId.HasValue && message.receiverId.Value == _currentUserId)
            {
                // 閺嶈宓?senderId 鐠佸墽鐤?ChatRoleType
                //Console.WriteLine($"received message: {message.content},senderId: {message.senderId},receiverId: {message.receiverId}");
                Console.WriteLine($"Correct View Received messsage: {message.content},id:{message.id}");
                SetMessageRole(message);

                // 婵″倹鐏夐弰顖氭禈閻楀洦绉烽幁顖欑瑬鐏忔碍婀張澶夌秴閸ユ拝绱濇稉瀣祰娴ｅ秴娴樻禒銉ょ返 UI 閺勫墽銇?
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
                // 缁変浇浜版稊鐔凤綖閸忓懏妯€缁夊府绱濇穱婵囧瘮閺勫墽銇氭稉鈧懛瀛樷偓?
                message.senderName = await ResolveDisplayName(message.senderId);
                message.senderAvatar = await LoadAvatarBitmapAsync(message.senderId);
                Messages.Add(message);

                if (message.senderId != _currentUserId)
                {
                    NotificationSettingsService.HandleIncomingMessage();
                }
            }
            else
            {
                Console.WriteLine($"Not this View but Received messsage: {message.content},id:{message.id}");
                _hubService.SetMessageToUnread(message);
                //todo : set message unread
            }
        }

        // 婢跺嫮鎮婇幒銉︽暪閸掓壆娈戠紘銈堜喊濞戝牊浼?
        private async void OnGroupMessageReceived(MessageDto message)
        {
            if (!IsGroupChat) return;
            if (message.groupId != _currentGroupId) return;

            Console.WriteLine($"Group message received: {message.content}, id:{message.id}");
            SetMessageRole(message);

            // 缂囥倛浜伴敍姘綖閸忓懎褰傞柅浣解偓鍛█缁夌増鏌熸笟鍨潔缁€?
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
            // 绾喕绻氶崷鈺慖缁捐法鈻煎ǎ璇插娴犮儴袝閸欐垶绮撮崝?
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
                //鏉╂瑩鍣烽崣顖欎簰閸旂姳绗傛禒璁崇秿闁偓閸戝搫缍嬮崜宥堜喊婢垛晝娈戦幙宥勭稊閿涘本鐦俊鍌涙焽瀵偓鏉╃偞甯寸粵澶堚偓?
                
                // foreach (var kvp in _chatmessages)
                // {
                //     if (kvp.Key != _currentChatId)
                //     {
                //         PostunreadMessages(kvp.Value.ToList());
                //     }
                // }
                
                
                await _hubService.DisconnectAsync();
            
                // 娴ｈ法鏁outer鐎佃壈鍩呴崚?ChatListModel 妞ょ敻娼?
                Router.Navigate.Execute(new ChatListModel(_loginResponse, Router));
                Dispose();
            }
            catch (Exception e)
            {
                // 婵″倹鐏夐崙铏瑰箛瀵倸鐖堕敍宀冪翻閸戞椽鏁婄拠顖欎繆閹?
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
                // 鐟欙綁娅庢禍瀣╂缂佹垵鐣?
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
        // 閺傛澘顤冮敍姘鳖潌閼卞﹤鍨垫慨瀣閿涘牏鈥樻穱婵嗗帥鏉╃偞甯撮崘宥呭鏉炴枻绱?
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

        // 閺傛澘顤冮敍姘卞參閼卞﹤鍨垫慨瀣閿涘牏鈥樻穱婵嗗帥鏉╃偞甯撮敍灞藉晙閸旂姴鍙嗙紘銈忕礉閸愬秴濮炴潪鏂ょ礆
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

        // 鐟欙絾鐎介獮鍓佺处鐎涙鏁ら幋閿嬫▔缁€鍝勬倳閿涙矮绱崗鍫滃▏閻劑娼粚铏规畱 DisplayName閿涘苯鍙惧▎?Username閿涙稑娲栭柅鈧稉绡扗閺冩湹绗夌紓鎾崇摠閿涘矂浼╅崗宥囩处鐎涙ɑ钖勯弻?
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
                    // 閸ョ偤鈧偓娑撶瘨D娴ｅ棔绗夌紓鎾崇摠閿涘矂浼╅崗宥勪簰閸氬孩妫ゅ▔鏇炲煕閺傜増妯€缁?
                    var fallbackId = userId.ToString();
                    return fallbackId;
                }

                _displayNameCache[userId] = name;
                return name;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResolveDisplayName failed: {ex.Message}");
                // 瀵倸鐖堕弮鎯扮箲閸ユ勘D娴ｅ棔绗夌紓鎾崇摠閿涘矁顔€閸氬海鐢婚柌宥堢槸閺堝婧€娴兼碍瀣侀崚鐗堟█缁?
                var fallback = userId.ToString();
                return fallback;
            }
        }
    }
}







