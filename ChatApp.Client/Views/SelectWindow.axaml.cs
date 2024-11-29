using Avalonia;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;

namespace ChatApp.Client.Views;

public partial class SelectWindow : ReactiveWindow<SelectWindowViewModel>
{
    public SelectWindow()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        this.WhenActivated(disposables => { /* Handle view activation etc. */ });
        AvaloniaXamlLoader.Load(this);
    }
}