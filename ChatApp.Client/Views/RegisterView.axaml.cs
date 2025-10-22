using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChatApp.Client.ViewModels;
using ReactiveUI;

namespace ChatApp.Client.Views
{
    public partial class RegisterView : ReactiveUserControl<RegisterViewModel>
    {
        private WindowNotificationManager? _notificationManager;

        public RegisterView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                if (ViewModel != null)
                {
                    ViewModel.AlertInteraction
                        .RegisterHandler(async interaction =>
                        {
                            await ShowNotificationAsync(interaction.Input.Title, interaction.Input.Message, interaction.Input.Type);
                            interaction.SetOutput(Unit.Default);
                        })
                        .DisposeWith(disposables);
                }
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private Task ShowNotificationAsync(string title, string message, NotificationType type)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
            {
                return Task.CompletedTask;
            }

            _notificationManager ??= new WindowNotificationManager(topLevel)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 2
            };

            _notificationManager.Show(new Avalonia.Controls.Notifications.Notification(title, message, type, System.TimeSpan.FromSeconds(3)));
            return Task.CompletedTask;
        }
    }
}
