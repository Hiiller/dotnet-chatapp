using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using Avalonia.Controls;
using ChatApp.Client.Helpers;
using ChatApp.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using ChatApp.Client.DTOs;
namespace ChatApp.Client.ViewModels;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.Client.Services;
using Shared.Models;

public class ChatListModel : ViewModelBase
{
    private LoginResponse _loginResponse;
    private readonly IHubService _hubService;
    private ChatService _chatService;
    public ObservableCollection<UserModel> RecentContacts { get; set; }
    private Dictionary<Guid, ObservableCollection<MessageDto>> _messageHistories = new Dictionary<Guid, ObservableCollection<MessageDto>>();

// 为每个联系人创建一个 Message History
    private ObservableCollection<MessageDto> GetOrCreateMessageHistory(Guid contactId)
    {
        if (!_messageHistories.ContainsKey(contactId))
        {
            // 如果不存在，创建新的消息历史
            _messageHistories[contactId] = new ObservableCollection<MessageDto>();
        }
        return _messageHistories[contactId];
    }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; }
    private ObservableCollection<MessageDto> _readMessages;
    public ObservableCollection<MessageDto> ReadMessages
    {
        get => _readMessages;
        set => this.SetProperty(ref _readMessages, value);
    }
    private AddRequestDto _addRequestDto => new AddRequestDto()
    {
        userId = _loginResponse.currentUserId,
        friendName = NewContactName
    };
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    
    
    private string _newContactName;
    public string NewContactName
    {
        get => _newContactName;
        set => this.RaiseAndSetIfChanged(ref _newContactName, value);
    }
    public ChatListModel(LoginResponse loginResponse,RoutingState router) : base(router)
    {
        _loginResponse = loginResponse;
        var httpClient = new HttpClient 
        {
            BaseAddress = new Uri("http://localhost:5005")
                    
        };
        _chatService = new ChatService(httpClient);
        _hubService = new HubService();
        _hubService.ConnectAsync(_loginResponse.currentUserId).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                _hubService.MessageReceived += OnMessageReceived;
            }
        });
        RecentContacts = new ObservableCollection<UserModel>();
        // 初始化命令
        RefreshCommand = ReactiveCommand.CreateFromTask(Refresh);
        AddCommand = ReactiveCommand.Create(AddContact);
        
        
    }
    
    private async Task Refresh()
    {
        try
        {
            RecentContacts.Clear();
            List<Friend> friendlist = await _chatService.GetFriend(_loginResponse.currentUserId);
            Console.WriteLine("try getting friends....");
            foreach (var friend in friendlist)
            {
                Console.WriteLine("get friend:" + friend.friendName + "," + friend.friendId);

                // 为每个新好友初始化消息历史
                if (!_messageHistories.ContainsKey(friend.friendId))
                {
                    _messageHistories[friend.friendId] = new ObservableCollection<MessageDto>();
                }
                
                RecentContacts.Add(new UserModel { Id = friend.friendId, Username = 
                    friend.friendName,ButtonCommand = new RelayCommand(OnButtonClicked)});
            }
            var recentMessages = await _chatService.GetRecentMessages(_loginResponse.currentUserId);
            foreach (var message in recentMessages)
            {
                if (message.receiverId == _loginResponse.currentUserId)
                {
                    var sender = RecentContacts.FirstOrDefault(u => u.Id == message.senderId);
                    if (sender != null)
                    {
                        sender.BackgroundColor = "Yellow";  // 标记为黄色
                    }
                }
            }
        } 
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private void OnMessageReceived(MessageDto message)
    {
        // 如果消息的接收者是当前用户
        if (message.receiverId == _loginResponse.currentUserId)
        {
            // 获取对应发送者的聊天记录
            var history = GetOrCreateMessageHistory(message.senderId);

            // 将新消息添加到该历史中
            history.Add(message);

            // 可以进一步进行其他处理，例如高亮显示发送者等
            var user = RecentContacts.FirstOrDefault(u => u.Id == message.senderId);
            if (user != null)
            {
                user.BackgroundColor = "Yellow";  // 将发送者按钮背景色改为黄色
            }
        }
    }
    private async void OnButtonClicked(object obj)
    {
        if (obj is UserModel user)
        {
            // 获取对应用户的消息历史
            var messageHistory = GetOrCreateMessageHistory(user.Id);

            // 创建一个聊天联系人对象并传递给 ChatViewModel
            InContact contactor = new InContact
            {
                user_id = _loginResponse.currentUserId,
                _oppo_id = user.Id,
                _oppo_name = user.Username
            };

            // 传递消息历史
            Router.Navigate.Execute(new ChatViewModel(_loginResponse, contactor,  Router, messageHistory));
            _messageHistories.Remove(user.Id);
            user.BackgroundColor = "#0078D7";  // 将发送者按钮背景色改为蓝色
            
        }

    }

    

    private async void AddContact()
    {
        //await Refresh();
        if (!string.IsNullOrEmpty(NewContactName))
        {
            Console.WriteLine("try adding friend...." + _addRequestDto.friendName);
            Friend friend = await _chatService.AddFriend(_addRequestDto);
            
            if (friend.friendName != null)
            {
                Console.WriteLine("add a friend: " + friend.friendName);
                NewContactName = string.Empty;
            
                // 如果添加成功，添加至联系人列表
                RecentContacts.Add(new UserModel { Id = friend.friendId,
                    Username = friend.friendName,ButtonCommand = new RelayCommand(OnButtonClicked)});
            }
            else
            {
                Console.WriteLine("friend object is null");
            }
            // 清空输入框
           

        }
       
    }
    
    
}