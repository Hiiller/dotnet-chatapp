using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ChatApp.Client.Services;
using Shared.MessageTypes;
using Shared.Models;

namespace ChatApp.Client.ViewModels;

public class SelectWindowViewModel : ViewModelBase
    {
        public ObservableCollection<User> Friends { get; } = new ObservableCollection<User>();
        public ObservableCollection<MessagePayload> ChatHistory { get; } = new ObservableCollection<MessagePayload>();

        private User _selectedFriend;
        public User SelectedFriend
        {
            get => _selectedFriend;
            set => this.RaiseAndSetIfChanged(ref _selectedFriend, value);
        }

        private string _messageInput;
        public string MessageInput
        {
            get => _messageInput;
            set => this.RaiseAndSetIfChanged(ref _messageInput, value);
        }

        public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
        private readonly ChatService _chatService;

        public SelectWindowViewModel(RoutingState router)
            : base(router)  // 调用基类构造函数
        {
            //_chatService = chatService;
            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessage);

            // 模拟获取好友列表
            Friends.Add(new User { UserName = "Alice" });
            Friends.Add(new User { UserName = "Bob" });
            Friends.Add(new User { UserName = "Charlie" });

            // 监听好友列表选择变化
            this.WhenAnyValue(x => x.SelectedFriend)
                .Where(friend => friend != null)
                .Subscribe(friend =>
                {
                    // 当选择好友时加载聊天历史并导航到聊天界面
                    LoadChatHistory(friend.UserName);
                    NavigateToChatView(friend.UserName);
                });
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageInput) || SelectedFriend == null)
                return;

            // 模拟发送消息
            var message = new MessagePayload(
                authorUsername: "CurrentUser", 
                recipientUsername: SelectedFriend.UserName, 
                message: MessageInput, 
                timestamp: DateTime.Now
            );

            await _chatService.SendMessageAsync(message);
            ChatHistory.Add(message);
            MessageInput = string.Empty;  // 清空输入框
        }

        private void LoadChatHistory(string userName)
        {
            // 模拟加载聊天历史
            ChatHistory.Clear();
            var dummyMessages = new[]
            {
                new MessagePayload ("Alice",userName,"Hello!",DateTime.Now.AddMinutes(-5)),
                new MessagePayload ( userName, "Alice",  "Hi! How are you?",  DateTime.Now.AddMinutes(-3) ),
                new MessagePayload ("Alice",  userName,  "I'm good, thanks!",  DateTime.Now.AddMinutes(-1) )
            };

            foreach (var msg in dummyMessages)
            {
                ChatHistory.Add(msg);
            }
        }

        private void NavigateToChatView(string friendUserName)
        {
            // 路由到聊天界面，传递所选好友的信息
            Router.Navigate.Execute(new ChatViewModel(_chatService, Router));
        }
    }