using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApp.Client.Helpers;  // 引用 RelayCommand
using ChatApp.Client.Services;  // 引用 IHubService
using Avalonia.Threading;  // 引入 Avalonia 的 UI 线程处理
using ReactiveUI;
using System;
using System.Linq;
using System.Windows.Input;
using Shared.MessageTypes;
using ChatApp.Client.Models;
using Avalonia.Controls.Notifications;
using System.Reactive.Linq;
using ChatApp.Client.DTOs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Shared.Models;

namespace ChatApp.Client.ViewModels
{
    //聊天视图的视图模型，负责管理 UI 和业务逻辑的交互
    public class ChatViewModel : ViewModelBase
    {
        private readonly IChatService _chatService;
        private readonly IHubService _hubService;
        private ObservableCollection<PrivateChatDto> _recentChats;
        private ObservableCollection<MessageDto> _messages;
        private string _messageContent;
        private Guid _currentUserId;
        private Guid _currentChatId;
        
        public ObservableCollection<PrivateChatDto> RecentChats
        {
            get => _recentChats;
            set => this.SetProperty(ref _recentChats, value);
        }
        
        // ObservableCollection用来绑定消息列表
        public ObservableCollection<MessageDto> Messages
        {
            get => _messages;
            set => this.SetProperty(ref _messages, value);
        }
        
        // 绑定到TextBox的MessageContent
        public string MessageContent
        {
            get => MessageContent;
            set => this.RaiseAndSetIfChanged(ref newMessageContent, value);
        }

        // 命令
        public ICommand DictateMessageCommand { get; private set; }

        public ICommand AttachImageCommand { get; private set; }

        public ICommand SendMessageCommand { get; private set; }

        public ChatViewModel(InContact contactor, RoutingState router) : base(router)
        {
            _hubService = new HubService();
            _hubService.ConnectAsync(contactor._oppo_id);

            // 监听消息集合变化并添加新消息到消息列表
            this.chatService.Messages.CollectionChanged += (sender, args) =>
            {
                foreach (MessagePayload newMsg in args.NewItems)
                {
                    ChatRoleType role = ChatRoleType.Receiver;
                    if (newMsg.AuthorUsername == chatService.CurrentUser.UserName)
                        role = ChatRoleType.Sender;

                    switch (newMsg.Type)
                    {
                        case MessageType.Text:
                            Messages.Add(new TextMessage(newMsg) { Role = role });
                            break;
                        //case MessageType.Link:
                        //    Messages.Add(new LinkMessage(newMsg) { Role = role });
                        //    break;
                        //case MessageType.Image:
                        //    Messages.Add(new ImageMessage(newMsg) { Role = role });
                        //    break;
                    }
                }
            };

            // 监听用户登录与登出
            this.chatService.ParticipantLoggedIn.Subscribe(x => { Messages.Add(new UserConnectedMessage(x)); });
            this.chatService.ParticipantLoggedOut.Subscribe(x => { Messages.Add(new UserDisconnectedMessage(x)); });

            // 判断是否能发送消息
            canSendMessage = this.WhenAnyValue(x => x.MessageContent).Select(x => !string.IsNullOrEmpty(x));

            // 创建命令
            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessage, canSendMessage);
            AttachImageCommand = ReactiveCommand.CreateFromTask(AttachImage);
            DictateMessageCommand = ReactiveCommand.CreateFromTask(DictateMessage);
        }
        
        
        // 加载最近的聊天记录
        private async Task LoadRecentChats()
        {
            var chats = await _chatService.GetRecentChatsAsync(_currentUserId);
            RecentChats.Clear();
            foreach (var chat in chats)
            {
                RecentChats.Add(chat);
            }
        }
        
        // 发送消息
        public async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(MessageContent)) return;

            await _hubService.SendPrivateMessageAsync(_currentUserId, _currentChatId, MessageContent);
            MessageContent = string.Empty;
        }
        // 处理接收到的消息
        private void OnMessageReceived(MessageDto message)
        {
            // 如果接收到的消息是当前聊天用户的消息，添加到消息列表
            if (message.SenderId == _currentChatId || message.ReceiverId == _currentChatId)
            {
                Messages.Add(message);
            }
        }
        
        private async Disconnect()
        {
            await _hubService.DisconnectAsync();
        }


        //Fields
        private ChatService chatService;
        private string newMessageContent;
        private WindowNotificationManager windowNotificationManager;
        private IObservable<bool> canSendMessage;
    }
}
