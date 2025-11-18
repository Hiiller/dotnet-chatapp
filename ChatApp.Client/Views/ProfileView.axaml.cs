using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ChatApp.Client.ViewModels;
using ReactiveUI;

namespace ChatApp.Client.Views;

public partial class ProfileView : ReactiveUserControl<ProfileViewModel>
{
    private TextBlock? _displayNameTextBlock;
    private TextBlock? _statusMessageTextBlock;
    private TextBlock? _aboutTextBlock;
    private TextBlock? _identifierLabelTextBlock;

    public ProfileView()
    {
        InitializeComponent();
        
        // Find controls after InitializeComponent
        _displayNameTextBlock = this.FindControl<TextBlock>("DisplayNameTextBlock");
        _statusMessageTextBlock = this.FindControl<TextBlock>("StatusMessageTextBlock");
        _aboutTextBlock = this.FindControl<TextBlock>("AboutTextBlock");
        _identifierLabelTextBlock = this.FindControl<TextBlock>("IdentifierLabelTextBlock");

        this.WhenActivated(disposables =>
        {
            if (ViewModel == null)
            {
                return;
            }

            // Subscribe to property changes using ReactiveUI's WhenAnyValue
            // This works with ReactiveObject's RaiseAndSetIfChanged
            ViewModel.WhenAnyValue(x => x.DisplayName)
                .Subscribe(displayName =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (_displayNameTextBlock != null)
                        {
                            _displayNameTextBlock.Text = displayName ?? string.Empty;
                        }
                    });
                })
                .DisposeWith(disposables);

            ViewModel.WhenAnyValue(x => x.StatusMessage)
                .Subscribe(statusMessage =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (_statusMessageTextBlock != null)
                        {
                            _statusMessageTextBlock.Text = statusMessage ?? string.Empty;
                        }
                    });
                })
                .DisposeWith(disposables);

            ViewModel.WhenAnyValue(x => x.About)
                .Subscribe(about =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (_aboutTextBlock != null)
                        {
                            _aboutTextBlock.Text = about ?? string.Empty;
                        }
                    });
                })
                .DisposeWith(disposables);

            ViewModel.WhenAnyValue(x => x.IdentifierLabel)
                .Subscribe(identifierLabel =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (_identifierLabelTextBlock != null)
                        {
                            _identifierLabelTextBlock.Text = identifierLabel ?? string.Empty;
                        }
                    });
                })
                .DisposeWith(disposables);

            // Reload profile data every time view is activated (when navigating to this view)
            // This ensures we always get fresh data from database, just like ChatListModel does
            if (ViewModel.LoadProfileCommand != null)
            {
                ViewModel.LoadProfileCommand.Execute().Subscribe().DisposeWith(disposables);
            }

            // Edit Profile dialog
            ViewModel.EditProfileInteraction.RegisterHandler(async interaction =>
            {
                var owner = TopLevel.GetTopLevel(this) as Window;
                var dlg = new EditProfileDialog();
                // Use current values from ViewModel (which should be loaded from database)
                var vm = new EditProfileViewModel(ViewModel.UserId, ViewModel.DisplayName, ViewModel.About, ViewModel.ChatService!);
                dlg.DataContext = vm;
                vm.CloseRequested += _ => dlg.Close();
                if (owner != null)
                    await dlg.ShowDialog(owner);
                else
                    dlg.Show();

                if (ViewModel != null)
                {
                    ViewModel.AvatarPreview = vm.AvatarPreview;
                }

                if (ViewModel.LoadProfileCommand != null)
                {
                    await ViewModel.LoadProfileCommand.Execute().ToTask();
                }

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
