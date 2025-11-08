using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;
using ReactiveUI;

namespace ChatApp.Client.Views;

public partial class ProfileView : ReactiveUserControl<ProfileViewModel>
{
    public ProfileView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel == null) return;

            // Edit Profile dialog
            ViewModel.EditProfileInteraction.RegisterHandler(async interaction =>
            {
                var owner = TopLevel.GetTopLevel(this) as Window;
                var dlg = new EditProfileDialog();
                var vm = new EditProfileViewModel(ViewModel.UserId, ViewModel.DisplayName, null, ViewModel.ChatService!);
                dlg.DataContext = vm;
                vm.CloseRequested += _ => dlg.Close();
                if (owner != null)
                    await dlg.ShowDialog(owner);
                else
                    dlg.Show();
                interaction.SetOutput(true);
            }).DisposeWith(disposables);

            // Security dialog
            ViewModel.SecurityInteraction.RegisterHandler(async interaction =>
            {
                var owner = TopLevel.GetTopLevel(this) as Window;
                var dlg = new SecurityDialog();
                var vm = new SecurityViewModel(ViewModel.UserId, ViewModel.ChatService!);
                dlg.DataContext = vm;
                vm.CloseRequested += _ => dlg.Close();
                if (owner != null)
                    await dlg.ShowDialog(owner);
                else
                    dlg.Show();
                interaction.SetOutput(true);
            }).DisposeWith(disposables);

            // Share card dialog
            ViewModel.ShareCardInteraction.RegisterHandler(async interaction =>
            {
                var (userId, displayName, personalCode) = interaction.Input;
                var owner = TopLevel.GetTopLevel(this) as Window;
                var dlg = new ShareCardDialog();
                var vm = new ShareCardViewModel(userId, displayName, personalCode, ViewModel.ChatService!);
                dlg.DataContext = vm;
                vm.CloseRequested += () => dlg.Close();
                if (owner != null)
                    await dlg.ShowDialog(owner);
                else
                    dlg.Show();
                interaction.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
