using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChatApp.Client.Views;

public partial class GroupDetailsView : UserControl
{
    public GroupDetailsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
