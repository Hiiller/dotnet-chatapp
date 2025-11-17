using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.DTOs;
using ChatApp.Client.ViewModels;
using ReactiveUI;

namespace ChatApp.Client.Views;

public partial class SearchFriendsView : ReactiveUserControl<SearchFriendsViewModel>
{
    public SearchFriendsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OnSendFriendRequestClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is SearchUserResultDto user && ViewModel != null)
        {
            _ = ViewModel.SendFriendRequestCommand.Execute(user);
        }
    }
}

