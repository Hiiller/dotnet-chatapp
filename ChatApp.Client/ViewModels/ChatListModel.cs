using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Avalonia.Controls;
using ChatApp.Client.Helpers;
using ChatApp.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;

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
    public ICommand RefreshCommend { get; private set; }
    
    public ObservableCollection<UserModel> RecentContacts { get; set; }

    public ChatListModel(LoginResponse loginResponse,RoutingState router) : base(router)
    {
        _loginResponse = loginResponse;
        ServerUrl = loginResponse.ServerUrl;
        RecentContacts = new ObservableCollection<UserModel>
        {
            new UserModel
            {
                Id = Guid.NewGuid(),
                Username = "Alice",
                ButtonCommand = new RelayCommand(OnButtonClicked)
            },
            new UserModel
            {
                Id = Guid.NewGuid(),
                Username = "Bob",
                ButtonCommand = new RelayCommand(OnButtonClicked)
            },
            new UserModel
            {
                Id = Guid.NewGuid(),
                Username = "Clear",
                ButtonCommand = new RelayCommand(OnButtonClicked)
            },
            new UserModel
            {
                Id = Guid.NewGuid(),
                Username = "Bobbyy",
                ButtonCommand = new RelayCommand(OnButtonClicked)
            },
            new UserModel
            {
                Id = Guid.NewGuid(),
                Username = "timmy",
                ButtonCommand = new RelayCommand(OnButtonClicked)
            }
            
        };
        RefreshCommend = ReactiveCommand.CreateFromTask(Refresh);
    }

    public string ServerUrl { get; set; }

    private async Task Refresh()
    {
        try
        {
            var httpClient = new HttpClient 
            {
                BaseAddress = new Uri(ServerUrl)
                
                    
            };
            var chatService = new ChatService(httpClient);
            RecentContactResponse response = await chatService.GetRecentContacts(_loginResponse.currentUserId);
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
    
}