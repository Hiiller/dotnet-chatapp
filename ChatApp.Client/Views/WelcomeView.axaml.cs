using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ChatApp.Client.ViewModels;

namespace ChatApp.Client.Views
{
    public partial class WelcomeView : ReactiveUserControl<WelcomeViewModel>
    {
        public WelcomeView()
        {
            InitializeComponent();
            this.DataContext = new WelcomeViewModel(new RoutingState());
        }
        
    }
}