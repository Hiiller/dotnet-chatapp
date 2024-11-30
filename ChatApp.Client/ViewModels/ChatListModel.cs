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
        RecentContacts = new ObservableCollection<UserModel>();
        RefreshCommand = ReactiveCommand.CreateFromTask(Refresh);
        AddCommand = ReactiveCommand.Create(AddContact);
        
    }
    
    private async Task Refresh()
    {
        try
        {
            var httpClient = new HttpClient 
            {
                BaseAddress = new Uri("http://localhost:5005")
                    
            };
            _chatService = new ChatService(httpClient);
            RecentContactResponse response = await _chatService.GetRecentContacts(_loginResponse.currentUserId);
            Display(response);

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
        foreach (var contact in response.Contacts)
        {
            RecentContacts.Add(new UserModel { Id = contact.Key, Username = contact.Value,ButtonCommand = new RelayCommand(OnButtonClicked)});
        }
    }
    
    private async void OnButtonClicked(object parameter)
    {
        if (parameter is Guid id)
        {
            
        }

    }
    
    private async void AddContact()
    {
        if (!string.IsNullOrEmpty(NewContactName))
        {
            Friend friend = await _chatService.AddFriend(_addRequestDto);

            // 清空输入框
            NewContactName = string.Empty;
            
            // 如果添加成功，添加至联系人列表
            RecentContacts.Add(new UserModel { Id = friend.FriendId ,Username = friend.FriendName,ButtonCommand = new RelayCommand(OnButtonClicked)});

        }
       
    }
    
}