using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChatApp.Client.ViewModels;
using ReactiveUI;
using Shared.Models;

namespace ChatApp.Client.Views;

public partial class ChatList : Window
{
    public ChatList()
    {
        InitializeComponent();
        DataContext = new ChatListModel(new LoginResponse(),new RoutingState());
    }
    

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}