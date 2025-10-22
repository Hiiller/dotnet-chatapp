using Avalonia.Controls.Notifications;
using ChatApp.Client.DTOs;
using ChatApp.Client.Services;
using ReactiveUI;
using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.Client.ViewModels
{
    // 管理用户注册流程的视图模型
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
                const string connectionFailed = "无法连接到服务器，请稍后再试。";
                ErrorMessage = connectionFailed;
                await AlertInteraction.Handle(("注册失败", connectionFailed, NotificationType.Error)).ToTask();
                return;
            }

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                const string emptyFieldMessage = "请输入用户名和密码。";
                ErrorMessage = emptyFieldMessage;
                await AlertInteraction.Handle(("提示", emptyFieldMessage, NotificationType.Warning)).ToTask();
                return;
            }

            if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            {
                const string mismatchMessage = "两次输入的密码不一致，请重新确认。";
                ErrorMessage = mismatchMessage;
                await AlertInteraction.Handle(("提示", mismatchMessage, NotificationType.Warning)).ToTask();
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
                    await AlertInteraction.Handle(("注册成功", "注册完成，正在为您登录...", NotificationType.Success));
                    Router.Navigate.Execute(new ChatListModel(result, Router));
                    return;
                }

                var message = result?.errorCode switch
                {
                    -1 => "该用户名已被注册，请尝试其他名称。",
                    -3 => "服务器出现问题，请稍后再试。",
                    _ => "注册失败，请检查输入信息。"
                };

                ErrorMessage = message;
                await AlertInteraction.Handle(("注册失败", message, NotificationType.Error)).ToTask();
            }
            catch (Exception ex)
            {
                const string exceptionMessage = "注册过程中发生错误，请稍后再试。";
                ErrorMessage = exceptionMessage;
                await AlertInteraction.Handle(("注册失败", exceptionMessage, NotificationType.Error)).ToTask();
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
    }
}
