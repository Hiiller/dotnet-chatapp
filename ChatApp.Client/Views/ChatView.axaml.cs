using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;
using System;
using Avalonia.Input;
using Avalonia.Remote.Protocol.Input;
using Key = Avalonia.Input.Key;

namespace ChatApp.Client.Views
{
    public partial class ChatView : ReactiveUserControl<ChatViewModel>
    {
        public ChatView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}