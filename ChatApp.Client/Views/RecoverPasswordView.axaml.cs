using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;

namespace ChatApp.Client.Views;

public partial class RecoverPasswordView : ReactiveUserControl<RecoverPasswordViewModel>
{
    public RecoverPasswordView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
