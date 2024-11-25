using System;
using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApp.Client.Helpers; // RelayCommand
using Avalonia.Threading; // Avalonia UI 线程

namespace ChatApp.Client.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private readonly IHubService _hubService;

        public ObservableCollection<string> Messages { get; } = new();

        private Guid _userId;
        public Guid UserId
        {
            get => _userId;
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Guid _receiverId;
        public Guid ReceiverId
        {
            get => _receiverId;
            set
            {
                if (_receiverId != value)
                {
                    _receiverId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _connectionStatus = "Not connected";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _messageContent;
        public string MessageContent
        {
            get => _messageContent;
            set
            {
                if (_messageContent != value)
                {
                    _messageContent = value;
                    RaisePropertyChanged();
                }
            }
        }

        public RelayCommand ConnectCommand { get; }
        public RelayCommand SendMessageCommand { get; }

        public ChatViewModel(IHubService hubService)
        {
            _hubService = hubService;

            // 初始化命令
            ConnectCommand = new RelayCommand(async () => await ConnectAsync(), 
                () => UserId != Guid.Empty);
            SendMessageCommand = new RelayCommand(async () => await SendPrivateMessageAsync(), 
                () => ReceiverId != Guid.Empty && !string.IsNullOrWhiteSpace(MessageContent));

            // 订阅消息接收事件
            _hubService.MessageReceived += (senderId, senderName, messageContent) =>
            {
                // 在 UI 线程上更新消息列表
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Messages.Add($"{senderName} ({senderId}): {messageContent}");
                });
            };
        }

        private async Task ConnectAsync()
        {
            try
            {
                ConnectionStatus = "Connecting...";
                await _hubService.ConnectAsync(UserId);
                ConnectionStatus = "Connected";
                Messages.Add("Successfully connected to the server.");
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Connection failed: {ex.Message}";
            }
        }

        private async Task SendPrivateMessageAsync()
        {
            try
            {
                await _hubService.SendPrivateMessageAsync(ReceiverId, MessageContent);
                Messages.Add($"You (to {ReceiverId}): {MessageContent}");
                MessageContent = string.Empty;
            }
            catch (Exception ex)
            {
                Messages.Add($"Failed to send message: {ex.Message}");
            }
        }
    }
}
