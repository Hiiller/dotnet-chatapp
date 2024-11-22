using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChatApp.Client.ViewModels;

namespace ChatApp.Client.Views
{

    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
            // DataContext 不需要再次设置，因为它已经通过 MainWindow 传递
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}