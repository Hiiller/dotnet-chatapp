using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;
using ReactiveUI;
using Shared.Models;

namespace ChatApp.Client.Views;

public partial class ChatList : ReactiveUserControl<ChatListModel>
{
    public ChatList()
    {
        InitializeComponent();
    }
    

    private void InitializeComponent()
    {
        this.WhenActivated(disposables => { /* Handle view activation etc. */ });   
        AvaloniaXamlLoader.Load(this);
    }

    
}