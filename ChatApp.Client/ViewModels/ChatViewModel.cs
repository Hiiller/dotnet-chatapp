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
using ChatApp.Client.DTOs;
using System.Net.Http;
using Shared.Models;
using System.Linq;
using Avalonia.Controls;
using Splat;

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
        private string _oppositeUserName;
        private LoginResponse _loginResponse;
        
       
        
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
            // 复用 ChatListModel 的 HubService 实例
            _hubService = Locator.Current.GetService<IHubService>();
            _hubService.MessageReceived += OnMessageReceived;
            
            _chatService = new ChatService(new HttpClient { BaseAddress = new Uri("http://localhost:5005") });

            _currentChatId = contactor._oppo_id;
            _currentUserId = contactor.user_id;
            _oppositeUserName = contactor._oppo_name;   
            // 拉取历史消息
            LoadMessages();
            //PostMessages(postmessage);
            // 判断是否能发送消息
            canSendMessage = this.WhenAnyValue(x => x.MessageContent).Select(x => !string.IsNullOrEmpty(x));

            // 创建命令
             SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessageAsync);
            // AttachImageCommand = ReactiveCommand.CreateFromTask(AttachImage);
            // DictateMessageCommand = ReactiveCommand.CreateFromTask(DictateMessage);
            ReturnToChatListCommand = ReactiveCommand.CreateFromTask(ReturnToChatList);
        }
        
        
        ~ChatViewModel()
        {
            _hubService.MessageReceived -= OnMessageReceived;
        }

        
        // 加载最近的聊天记录
        private async void LoadMessages()
        {
            try
            {
                // 使用 GetPrivateMessages 从后端获取历史消息
                var messages = await _chatService.GetPrivateMessages(_currentChatId, _currentUserId);

                // 将历史消息合并到 MessageHistory 中
                foreach (var message in messages)
                {
                    // 设置每条消息的角色
                    SetMessageRole(message);

                    
                    Messages.Add(message);
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
        private void SetMessageRole(MessageDto message)
        {
            if (message.senderId == _currentUserId)
            {
                message.ChatRoleType = ChatRoleType.Sender;
            }
            else
            {
                message.ChatRoleType = ChatRoleType.Receiver;
            }
        }
        
        // 发送消息
        public async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(MessageContent)) return;
            
            // Create the message and add it immediately to the collection
            var message = new MessageDto
            {
                senderId = _currentUserId,
                receiverId = _currentChatId,
                content = MessageContent,
                timestamp = DateTime.UtcNow,
                ChatRoleType = ChatRoleType.Receiver  // 默认设置为Receiver
            };
    
            // 调用方法设置消息的角色
            SetMessageRole(message);
            
            // Add the message immediately to the collection for UI updates
            Messages.Add(message);
            await _chatService.PostreadMessageToDb(message);
            
            // Send the message via SignalR
            Console.WriteLine("Sending: " + MessageContent + " to: " + _currentChatId);
            await _hubService.SendPrivateMessageAsync(_currentUserId, _currentChatId, MessageContent);
            
            // Clear the input field after sending
            MessageContent = "";
            MessageContent = string.Empty;
        }
        
        // 处理接收到的消息
        private void OnMessageReceived(MessageDto message)
        {
            // 如果接收到的消息是当前聊天用户的消息，添加到消息列表
            
            if (message.senderId == _currentChatId && message.receiverId == _currentUserId)
            {
                // 根据 senderId 设置 ChatRoleType
                //Console.WriteLine($"received message: {message.content},senderId: {message.senderId},receiverId: {message.receiverId}");
                SetMessageRole(message);
                Messages.Add(message);
            }
            else
            {
                _hubService.SetMessagetoUnread(message);
                //todo : set message unread
            }
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
            }
            catch (Exception e)
            {
                // 如果出现异常，输出错误信息
                Console.WriteLine(e);
                throw;
            }
        }

        //Fields
        private ChatService chatService;
        private string newMessageContent;
        private WindowNotificationManager windowNotificationManager;
        private IObservable<bool> canSendMessage;
    }
}
