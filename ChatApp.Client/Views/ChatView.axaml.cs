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
            // DataContext ����Ҫ�ٴ����ã���Ϊ���Ѿ�ͨ�� MainWindow ����
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}