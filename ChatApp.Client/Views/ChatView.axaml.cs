using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;
using System;
using Avalonia.Input;
using Avalonia.Remote.Protocol.Input;
using Avalonia.Threading;
using Key = Avalonia.Input.Key;

namespace ChatApp.Client.Views
{
    public partial class ChatView : ReactiveUserControl<ChatViewModel>
    {
        private ScrollViewer? _messagesScrollViewer;
        private Border? _endAnchor;
        private ItemsControl? _itemsControl;
        public ChatView()
        {
            InitializeComponent();
            AttachAutoScroll();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _messagesScrollViewer = this.FindControl<ScrollViewer>("MessagesScrollViewer");
            _endAnchor = this.FindControl<Border>("EndAnchor");
            _itemsControl = this.FindControl<ItemsControl>("MessagesItemsControl");
        }

        private void AttachAutoScroll()
        {
            // When the DataContext or Messages collection changes, keep the scroll anchored to the latest message
            this.DataContextChanged += (_, __) =>
            {
                HookMessagesCollection();
            };
            HookMessagesCollection();
        }

        private void HookMessagesCollection()
        {
            if (ViewModel?.Messages == null) return;
            ViewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
            ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        }

        private void Messages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // Push the anchor into view on the UI thread so we always land at the newest message
                Dispatcher.UIThread.Post(() => _endAnchor?.BringIntoView());
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChatViewModel.Messages))
            {
                HookMessagesCollection();
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }
    }
}
