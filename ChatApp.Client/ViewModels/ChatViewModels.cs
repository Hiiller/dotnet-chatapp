//ChatApp.Client/ViewModels/ChatViewModel.cs

using System;
using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApp.Client.Helpers;  //RelayCommand
using ChatApp.Client.Services;  //IHubService
using Avalonia.Threading;  // Avalonia UI 线程

namespace ChatApp.Client.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private readonly IHubService _hubService;

        public ObservableCollection<string> Messages { get; } = new();
        private string _username;
        private string _connectionStatus = "Not connected";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    RaisePropertyChanged();  // 通知属性已改变，UI 将更新
                }
            }
        }
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                RaiseCanExecuteChanged();  // Notify command that CanExecute status might have changed
            }
        }

        private string _chatroom;
        public string Chatroom
        {
            get => _chatroom;
            set
            {
                _chatroom = value;
                RaiseCanExecuteChanged();  // Notify command that CanExecute status might have changed
            }
        }

        private void RaiseCanExecuteChanged()
        {
            // Raise CanExecuteChanged event to notify that the command's state has changed
            ConnectCommand.RaiseCanExecuteChanged();
        }
        public string Message { get; set; } = string.Empty;

        public RelayCommand ConnectCommand { get; }
        public RelayCommand SendMessageCommand { get; }

        public ChatViewModel(IHubService hubService)
        {
            _hubService = hubService;
            TestConnectionAsync();
            ConnectCommand = new RelayCommand(async () => await ConnectAsync(), 
                () => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Chatroom));
            SendMessageCommand = new RelayCommand(async () => await SendMessageAsync());

            _hubService.MessageReceived += (user, message) =>
            {
                // 使用 Avalonia UI 线程更新 UI
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Messages.Add($"{user}: {message}");
                });
            };
        }

        private async Task ConnectAsync()
        {
            await _hubService.ConnectAsync(Username, Chatroom);
            Messages.Add("Connected to the chatroom.");
        }

        private async Task SendMessageAsync()
        {
            if (!string.IsNullOrWhiteSpace(Message))
            {
                await _hubService.SendMessageAsync(Chatroom, Username, Message);
                Message = string.Empty;
            }
        }
        
        private async Task TestConnectionAsync()
        {
            try
            {
                // 使用默认用户名和聊天室连接
                await _hubService.ConnectAsync("TestUser", "TestRoom");

                // 连接成功，更新状态
                ConnectionStatus = "Connected to the server."; // 会触发 RaisePropertyChanged
            }
            catch (Exception ex)
            {
                // 连接失败，显示错误信息
                ConnectionStatus = $"Connection failed: {ex.Message}"; // 会触发 RaisePropertyChanged
            }
        }



    }
}