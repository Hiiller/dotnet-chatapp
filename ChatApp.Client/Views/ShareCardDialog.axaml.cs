using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChatApp.Client.Views;

public partial class ShareCardDialog : Window
{
    public ShareCardDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

