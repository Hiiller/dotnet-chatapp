using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChatApp.Client.Services;
using ChatApp.Client.ViewModels;

namespace ChatApp.Client.Views
{
    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
            this.DataContext = new ChatViewModel(new HubService());
            // DataContext 不需要再次设置，因为已经通过 MainWindow 设置
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}