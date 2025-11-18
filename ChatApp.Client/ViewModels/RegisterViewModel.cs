using Avalonia.Controls.Notifications;
using ChatApp.Client.DTOs;
using ChatApp.Client.Services;
using ChatApp.Client.Helpers;
using ReactiveUI;
using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
using Shared.Models;

namespace ChatApp.Client.ViewModels
{
    // 绠＄悊鐢ㄦ埛娉ㄥ唽娴佺▼鐨勮鍥炬ā鍨?
    public class RegisterViewModel : ViewModelBase
    {
        public RegisterViewModel(RoutingState router) : base(router)
        {
            ServerUrl = "http://localhost:5005";
            RegisterCommand = ReactiveCommand.CreateFromTask(RegisterAsync);
            BackCommand = ReactiveCommand.CreateFromObservable(() => Router.NavigateBack.Execute());
        }

        public string ServerUrl
        {
            get => serverUrl;
            set => this.RaiseAndSetIfChanged(ref serverUrl, value);
        }

        public string Username
        {
            get => username;
            set => this.RaiseAndSetIfChanged(ref username, value);
        }

        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }

        public string ConfirmPassword
        {
            get => confirmPassword;
            set => this.RaiseAndSetIfChanged(ref confirmPassword, value);
        }

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                this.RaiseAndSetIfChanged(ref errorMessage, value);
                this.RaisePropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public ICommand RegisterCommand { get; }

        public ICommand BackCommand { get; }

        public Interaction<(string Title, string Message, NotificationType Type), Unit> AlertInteraction { get; }
            = new Interaction<(string Title, string Message, NotificationType Type), Unit>();

        private async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;
            if (!await EnsureConnectedAsync())
            {
                const string connectionFailed = "Unable to reach the server. Please try again later.";
                ErrorMessage = connectionFailed;
                await AlertInteraction.Handle(("Error", connectionFailed, NotificationType.Error)).ToTask();
                return;
            }

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                const string emptyFieldMessage = "Please enter a username and password.";
                ErrorMessage = emptyFieldMessage;
                await AlertInteraction.Handle(("Warning", emptyFieldMessage, NotificationType.Warning)).ToTask();
                return;
            }

            if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            {
                const string mismatchMessage = "The two passwords do not match.";
                ErrorMessage = mismatchMessage;
                await AlertInteraction.Handle(("Warning", mismatchMessage, NotificationType.Warning)).ToTask();
                return;
            }

            try
            {
                var registerDto = new RegisterUserDto
                {
                    Username = Username,
                    Password = Password
                };

                var result = await chatService!.RegisterUser(registerDto);
                if (result != null && result.connectionStatus != false)
                {
                    ErrorMessage = string.Empty;
                    await AlertInteraction.Handle(("Success", "Registration complete. Signing you in...", NotificationType.Success)).ToTask();
                    await AssignDefaultAvatarAsync(result);
                    Router.Navigate.Execute(new ChatListModel(result, Router));
                    return;
                }

                var message = result?.errorCode switch
                {
                    -1 => "This username is already taken.",
                    -3 => "The server encountered a problem. Please retry soon.",
                    _ => "Registration failed. Please check your details."
                };

                ErrorMessage = message;
                await AlertInteraction.Handle(("Error", message, NotificationType.Error)).ToTask();
            }
            catch (Exception ex)
            {
                const string exceptionMessage = "An unexpected error occurred during registration. Please try again.";
                ErrorMessage = exceptionMessage;
                await AlertInteraction.Handle(("Error", exceptionMessage, NotificationType.Error)).ToTask();
                Console.WriteLine(ex);
            }
        }


        private async Task<bool> EnsureConnectedAsync()
        {
            if (chatService != null)
            {
                return true;
            }

            try
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(ServerUrl)
                };

                var response = await httpClient.GetAsync("/api/chat/ping");
                if (response.IsSuccessStatusCode)
                {
                    chatService = new ChatService(httpClient);
                    return true;
                }

                httpClient.Dispose();
                chatService = null;
                Console.WriteLine($"Failed to connect to the server. Status Code: {response.StatusCode}");
            }
            catch (Exception e)
            {
                chatService = null;
                Console.WriteLine($"Connection error: {e.Message}");
            }

            return false;
        }

        private ChatService? chatService;
        private string username = string.Empty;
        private string password = string.Empty;
        private string confirmPassword = string.Empty;
        private string serverUrl = string.Empty;

        private async Task AssignDefaultAvatarAsync(LoginResponse result)
        {
            if (chatService == null)
            {
                return;
            }

            var assetName = SystemImageProvider.GetRandomAvatar();
            var bytes = SystemImageProvider.LoadAssetBytes(assetName);
            if (bytes == null)
            {
                return;
            }

            var update = new UpdateProfileDto
            {
                Username = result.currentUsername,
                DisplayName = result.currentUsername,
                AvatarBase64 = Convert.ToBase64String(bytes)
            };

            try
            {
                await chatService.UpdateProfile(result.currentUserId, update);
                try { await AvatarCache.SaveAsync(result.currentUserId, bytes); } catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignDefaultAvatarAsync error: {ex.Message}");
            }
        }
    }
}




















