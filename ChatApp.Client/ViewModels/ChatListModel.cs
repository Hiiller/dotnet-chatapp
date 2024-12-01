using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    private ChatService _chatService;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; }
    private AddRequestDto _addRequestDto => new AddRequestDto()
    {
        userId = _loginResponse.currentUserId,
        friendName = NewContactName
    };
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ObservableCollection<UserModel> RecentContacts { get; set; }
    
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
        RecentContacts = new ObservableCollection<UserModel>();
        RefreshCommand = ReactiveCommand.CreateFromTask(Refresh);
        AddCommand = ReactiveCommand.Create(AddContact);
        
    }
    
    private async Task Refresh()
    {
        try
        {
            RecentContactResponse response = await _chatService.GetRecentContacts(_loginResponse.currentUserId);
            Display(response);
            List<Friend> friendlist = await _chatService.GetFriend(_loginResponse.currentUserId);
            Console.WriteLine("try getting friends....");
            foreach (var friend in friendlist)
            {
                Console.WriteLine("get friend:"+friend.friendName+","+friend.friendId);
                RecentContacts.Add(new UserModel { Id = friend.friendId, Username = friend.friendName,ButtonCommand = new RelayCommand(OnButtonClicked)});
            }
        } 
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    
    private void Display(RecentContactResponse response)
    {
        RecentContacts.Clear();
        Console.WriteLine("try getting recentcontact....");
        foreach (var contact in response.Contacts)
        {
            Console.WriteLine("get friend:"+contact.Key+"," +contact.Value);
            RecentContacts.Add(new UserModel { Id = contact.Key, Username = contact.Value,ButtonCommand = new RelayCommand(OnButtonClicked)});
        }
        
    }
    
    private async void OnButtonClicked(object obj)
    {
        
        
        if (obj is UserModel user)
        {
            InContact  contactor = new InContact();
            contactor._oppo_id = user.Id;
            contactor._oppo_name = user.Username;
            Router.Navigate.Execute(new ChatViewModel(contactor, Router));

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