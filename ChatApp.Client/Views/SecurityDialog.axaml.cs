using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChatApp.Client.Views;

public partial class SecurityDialog : Window
{
    public SecurityDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

