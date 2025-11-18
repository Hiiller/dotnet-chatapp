using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;

namespace ChatApp.Client.Views;

public partial class NotificationSettingsView : ReactiveUserControl<NotificationSettingsViewModel>
{
    public NotificationSettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
