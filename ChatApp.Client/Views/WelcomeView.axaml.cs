using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;

namespace ChatApp.Client.Views
{
    public partial class WelcomeView : ReactiveUserControl<WelcomeViewModel>
    {
        public WelcomeView()
        {
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { /* Handle view activation etc. */ });
            AvaloniaXamlLoader.Load(this);
        }
    }
}