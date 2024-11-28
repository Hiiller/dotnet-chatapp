using Avalonia.Controls;
using ChatApp.Client.Services;
using ChatApp.Client.DTOs;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ChatApp.Client.ViewModels
{
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

        public ObservableCollection<MessageDto> Messages
        {
            get => _messages;
            set => this.SetProperty(ref _messages, value);
        }

        public string MessageContent
        {
            get => _messageContent;
            set => this.SetProperty(ref _messageContent, value);
        }

        public ChatViewModel(IChatService chatService, IHubService hubService)
        {
            _chatService = chatService;
            _hubService = hubService;
            _recentChats = new ObservableCollection<PrivateChatDto>();
            _messages = new ObservableCollection<MessageDto>();
        }

        // 登录并加载最近聊天记录
        public async Task LoginAsync(string username, string password)
        {
            var loginSuccess = await _chatService.LoginUserAsync(username, password);
            if (loginSuccess)
            {
                // 假设登录后返回一个用户 ID
                _currentUserId = Guid.NewGuid(); // 获取当前用户 ID
                await LoadRecentChats();
                await _hubService.ConnectAsync(_currentUserId);
                _hubService.MessageReceived += OnMessageReceived;
            }
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

        // 加载聊天记录
        public async Task LoadMessages(Guid userId)
        {
            _currentChatId = userId;
            var messages = await _chatService.GetPrivateMessagesAsync(_currentUserId, userId);
            Messages.Clear();
            foreach (var message in messages)
            {
                Messages.Add(message);
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
    }
}
