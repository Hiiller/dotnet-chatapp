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
    // 管理用户登录/注册功能的视图模型
    public class WelcomeViewModel : ViewModelBase
    {
        public string ServerUrl
        {
            get => serverUrl;
            set
            {
                this.RaiseAndSetIfChanged(ref serverUrl, value);
                chatService = null;
                Connected = false;
            }
        }

        public string Username
        {
            get => username;
            set => this.RaiseAndSetIfChanged(ref username, value);
        }

        public string Passcode
        {
            get => passcode;
            set => this.RaiseAndSetIfChanged(ref passcode, value);
        }

        private LoginUserDto LoginDto => new LoginUserDto
        {
            Username = Username,
            Password = Passcode
        };

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set => this.RaiseAndSetIfChanged(ref errorMessage, value);
        }

        public ICommand ConnectCommand { get; }

        public ICommand RegisterCommand { get; }

        public ICommand LoginCommand { get; }

        public bool Connected
        {
            get => connected;
            set => this.RaiseAndSetIfChanged(ref connected, value);
        }

        public Interaction<(string Title, string Message, NotificationType Type), Unit> AlertInteraction { get; }
            = new Interaction<(string Title, string Message, NotificationType Type), Unit>();

        public WelcomeViewModel(RoutingState router) : base(router)
        {
            ServerUrl = "http://localhost:5005";
            RegisterCommand = ReactiveCommand.Create(NavigateToRegister);
            LoginCommand = ReactiveCommand.CreateFromTask(Login);
            ConnectCommand = ReactiveCommand.CreateFromTask(Connect);
        }

        private void NavigateToRegister()
        {
            Router.Navigate.Execute(new RegisterViewModel(Router));
        }

        private async Task<bool> Connect()
        {
            if (chatService != null && Connected)
            {
                return true;
            }

            try
            {
                Console.WriteLine("Attempting to connect...");

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(ServerUrl)
                };

                var response = await httpClient.GetAsync("/api/chat/ping");
                if (response.IsSuccessStatusCode)
                {
                    chatService = new ChatService(httpClient);
                    Connected = true;
                    Console.WriteLine("Connected to the http server successfully.");
                    Console.WriteLine($"Http Connected state updated: {Connected}");
                    return true;
                }

                Connected = false;
                chatService = null;
                httpClient.Dispose();
                Console.WriteLine($"Failed to connect to the server. Status Code: {response.StatusCode}");
                return false;
            }
            catch (Exception e)
            {
                Connected = false;
                chatService = null;
                Console.WriteLine($"Connection error: {e.Message}");
                return false;
            }
        }

        private async Task Login()
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Passcode))
            {
                const string emptyInputMessage = "请输入账号和密码。";
                ErrorMessage = emptyInputMessage;
                await AlertInteraction.Handle(("登录提醒", emptyInputMessage, NotificationType.Warning));
                return;
            }

            var connected = await Connect();
            if (!connected || chatService == null)
            {
                const string connectionFailed = "无法连接到服务器，请检查网络或服务器地址。";
                ErrorMessage = connectionFailed;
                await AlertInteraction.Handle(("登录失败", connectionFailed, NotificationType.Error));
                return;
            }

            try
            {
                var loginResult = await chatService.LoginUser(LoginDto);
                if (loginResult != null)
                {
                    if (loginResult.connectionStatus != false)
                    {
                        Router.Navigate.Execute(new ChatListModel(loginResult, Router));
                        ErrorMessage = string.Empty;
                        return;
                    }

                    Console.WriteLine(loginResult?.errorCode);
                                        var message = loginResult?.errorCode switch
                    {
                        -1 => "注册失败：用户名已存在。",
                        -2 => "账号未注册",
                        -3 => "服务器错误：请稍后再试。",
                        _ => "登录失败：请检查账号和密码。"
                    };

                    ErrorMessage = message;
                    var type = loginResult?.errorCode switch
                    {
                        -2 => NotificationType.Warning,
                        -1 => NotificationType.Warning,
                        -3 => NotificationType.Error,
                        _ => NotificationType.Error
                    };
                    await AlertInteraction.Handle(("登录失败", message, type));
                }
                else
                {
                    const string unknownMessage = "登录失败：服务器返回了空响应。";
                    ErrorMessage = unknownMessage;
                    await AlertInteraction.Handle(("登录失败", unknownMessage, NotificationType.Error));
                }
            }
            catch (Exception e)
            {
                const string exceptionMessage = "登录过程中发生错误，请稍后重试。";
                ErrorMessage = exceptionMessage;
                await AlertInteraction.Handle(("登录失败", exceptionMessage, NotificationType.Error));
                Console.WriteLine(e);
            }
        }

        //Fields
        private ChatService? chatService;
        private string username = string.Empty;
        private string passcode = string.Empty;
        private string serverUrl = string.Empty;
        private bool connected;
    }
}

