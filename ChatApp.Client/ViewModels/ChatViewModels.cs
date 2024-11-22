using ChatApp.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApp.Client.Helpers;  // ���� RelayCommand
using ChatApp.Client.Services;  // ���� IHubService
using Avalonia.Threading;  // ���� Avalonia �� UI �̴߳���

namespace ChatApp.Client.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private readonly IHubService _hubService;

        public ObservableCollection<string> Messages { get; } = new();
        public string Username { get; set; } = string.Empty;
        public string Chatroom { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public RelayCommand ConnectCommand { get; }
        public RelayCommand SendMessageCommand { get; }

        public ChatViewModel(IHubService hubService)
        {
            _hubService = hubService;

            ConnectCommand = new RelayCommand(async () => await ConnectAsync());
            SendMessageCommand = new RelayCommand(async () => await SendMessageAsync());

            _hubService.MessageReceived += (user, message) =>
            {
                // ʹ�� Avalonia �� UI �߳������½���
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
    }
}