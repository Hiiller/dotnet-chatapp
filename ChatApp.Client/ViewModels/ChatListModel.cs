using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using Avalonia.Controls;
using ChatApp.Client.Helpers;
using ChatApp.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using ChatApp.Client.DTOs;
using Splat;

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

// 为每个联系人创建一个 Message History
   
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
        // 获取单例 HubService 实例
        _hubService = Locator.Current.GetService<IHubService>();
        _hubService.ConnectAsync(_loginResponse.currentUserId).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                _hubService.MessageReceived += OnMessageReceived;
            }
            else
            {
                Console.WriteLine("Error connecting to HubService in ChatListModel.");
            }
        });
        RecentContacts = new ObservableCollection<UserModel>();
        // 初始化命令
        RefreshCommand = ReactiveCommand.CreateFromTask(Refresh);
        AddCommand = ReactiveCommand.Create(AddContact);
        Refresh();

    }
    
    private async Task Refresh()
    {
        try
        {
            RecentContacts.Clear();
            List<Friend> friendlist = await _chatService.GetFriend(_loginResponse.currentUserId);
            Console.WriteLine("try getting friends....");
            if ( friendlist.Count == 0)
            {
                Console.WriteLine("No friends found.");
                return; // 如果没有好友，直接返回
            }
            
            foreach (var friend in friendlist)
            {
                Console.WriteLine("get friend:" + friend.friendName + "," + friend.friendId);
                
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
                        sender.BackgroundColor = "#FF3B2F";  // 若用户发送了未读信息，将按钮的颜色更改
                    }
                    else
                    {
                        sender.BackgroundColor = "#0078D7";  // 若用户没有发送未读信息，将按钮的颜色更改
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
            // 可以进一步进行其他处理，例如高亮显示发送者等
            var user = RecentContacts.FirstOrDefault(u => u.Id == message.senderId);
            if (user != null)
            {
                user.BackgroundColor = "#FF3B2F";  // 将发送者按钮背景色改为红色
            }
            //todo : set message as unread
            Console.WriteLine($"List Received messsage: {message.content},id:{message.id}");
            _hubService.SetMessageToUnread(message);
        } 
    }
    private async void OnButtonClicked(object obj)
    {
        if (obj is UserModel user)
        {
            // 获取对应用户的消息历史

            // 创建一个聊天联系人对象并传递给 ChatViewModel
            InContact contactor = new InContact
            {
                user_id = _loginResponse.currentUserId,
                _oppo_id = user.Id,
                _oppo_name = user.Username
            };

            // 传递消息历史
            // Router.Navigate.Execute(new ChatViewModel(_loginResponse, contactor,  Router, messageHistory));
            Router.Navigate.Execute(new ChatViewModel(_loginResponse, contactor, Router));
            user.BackgroundColor = "#0078D7";  // 将发送者按钮背景色改为蓝色
            Cleanup();
            
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
    
    public void Cleanup()
    {
        _hubService.MessageReceived -= OnMessageReceived;
        Console.WriteLine("ChatListModel cleaned up.");
    }
    
    ~ChatListModel()
    {
        Cleanup(); // 确保销毁时清理资源
    }
    
}