using Avalonia.Controls;
using ChatApp.Client.ViewModels;
using ChatApp.Client.DTOs;
using System;

namespace ChatApp.Client.Views
{
    public partial class ChatView : Window
    {
        private readonly ChatViewModel _viewModel;

        public ChatView(ChatViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }

        // 选择某个聊天
        private async void OnChatSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedChat = (PrivateChatDto)e.AddedItems[0];
                await _viewModel.LoadMessages(selectedChat.UserId);
            }
        }
    }
}