using System.ComponentModel.DataAnnotations;
using System.Net.Http;
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
using Shared.MessageTypes;
using Shared.Models;

public class ChatListModel : ViewModelBase
{
    public ICommand RefreshCommend { get; private set; }
    
    public ChatListModel(LoginResponse loginResponse,RoutingState router) : base(router)
    {
        ServerUrl = loginResponse.ServerUrl;
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
            RecentContactResponse response = await GetRecentContacts();
            Display(response);

        } 
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private async Task<RecentContactResponse> GetRecentContacts()
    {
        // TODO: var response = await chatService.GetRecentContacts();
        return new RecentContactResponse();
    }
    
    private void Display(RecentContactResponse response)
    {
        // TODO : display the response
    }
    
    
}