using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;

namespace ChatApp.Client.Views;

public partial class PrivacySettingsView : ReactiveUserControl<PrivacySettingsViewModel>
{
    public PrivacySettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
