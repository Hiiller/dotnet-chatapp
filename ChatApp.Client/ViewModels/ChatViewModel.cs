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

namespace ChatApp.Client.ViewModels
{
    //聊天视图的视图模型，负责管理 UI 和业务逻辑的交互
    public class ChatViewModel : ViewModelBase
    {
        // ObservableCollection用来绑定消息列表
        public ObservableCollection<MessageBase> Messages { get; private set; }

        // 绑定到TextBox的NewMessageContent
        public string NewMessageContent
        {
            get => newMessageContent;
            set => this.RaiseAndSetIfChanged(ref newMessageContent, value);
        }

        // 命令
        public ICommand DictateMessageCommand { get; private set; }

        public ICommand AttachImageCommand { get; private set; }

        public ICommand SendMessageCommand { get; private set; }

        public ChatViewModel(ChatService chatService, RoutingState router) : base(router)
        {
            this.Messages = new ObservableCollection<MessageBase>();
            this.chatService = chatService;

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
            canSendMessage = this.WhenAnyValue(x => x.NewMessageContent).Select(x => !string.IsNullOrEmpty(x));

            // 创建命令
            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessage, canSendMessage);
            AttachImageCommand = ReactiveCommand.CreateFromTask(AttachImage);
            DictateMessageCommand = ReactiveCommand.CreateFromTask(DictateMessage);
        }

        // 发送消息
        async Task SendMessage()
        {
            await chatService.SendMessageAsync(new TextMessage(newMessageContent, chatService.CurrentUser.UserName).ToMessagePayload());
            NewMessageContent = string.Empty;
        }

        // 图片附加
        async Task AttachImage()
        {

        }

        // 语音输入
        async Task DictateMessage()
        {
        }


        //Fields
        private ChatService chatService;
        private string newMessageContent;
        private WindowNotificationManager windowNotificationManager;
        private IObservable<bool> canSendMessage;
    }
}


//namespace ChatApp.Client.ViewModels
//{
//    public class ChatViewModel : ViewModelBase
//    {
//        private readonly IHubService _hubService;

//        public ObservableCollection<string> Messages { get; } = new();
//        public string Username { get; set; } = string.Empty;
//        public string Chatroom { get; set; } = string.Empty;
//        public string Message { get; set; } = string.Empty;

//        public RelayCommand ConnectCommand { get; }
//        public RelayCommand SendMessageCommand { get; }

//        public ChatViewModel(IHubService hubService)
//        {
//            _hubService = hubService;

//            ConnectCommand = new RelayCommand(async () => await ConnectAsync());
//            SendMessageCommand = new RelayCommand(async () => await SendMessageAsync());

//            _hubService.MessageReceived += (user, message) =>
//            {
//                // 使用 Avalonia 的 UI 线程来更新界面
//                Dispatcher.UIThread.InvokeAsync(() =>
//                {
//                    Messages.Add($"{user}: {message}");
//                });
//            };
//        }

//        private async Task ConnectAsync()
//        {
//            await _hubService.ConnectAsync(Username, Chatroom);
//            Messages.Add("Connected to the chatroom.");
//        }

//        private async Task SendMessageAsync()
//        {
//            if (!string.IsNullOrWhiteSpace(Message))
//            {
//                await _hubService.SendMessageAsync(Chatroom, Username, Message);
//                Message = string.Empty;
//            }
//        }
//    }
//}