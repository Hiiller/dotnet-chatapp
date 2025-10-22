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
    // �����û���¼/ע�Ṧ�ܵ���ͼģ��
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
                const string emptyInputMessage = "�������˺ź����롣";
                ErrorMessage = emptyInputMessage;
                await AlertInteraction.Handle(("��¼����", emptyInputMessage, NotificationType.Warning));
                return;
            }

            var connected = await Connect();
            if (!connected || chatService == null)
            {
                const string connectionFailed = "�޷����ӵ�������������������������ַ��";
                ErrorMessage = connectionFailed;
                await AlertInteraction.Handle(("��¼ʧ��", connectionFailed, NotificationType.Error));
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
                        -1 => "ע��ʧ�ܣ��û����Ѵ��ڡ�",
                        -2 => "�˺�δע��",
                        -3 => "�������������Ժ����ԡ�",
                        _ => "��¼ʧ�ܣ������˺ź����롣"
                    };

                    ErrorMessage = message;
                    var type = loginResult?.errorCode switch
                    {
                        -2 => NotificationType.Warning,
                        -1 => NotificationType.Warning,
                        -3 => NotificationType.Error,
                        _ => NotificationType.Error
                    };
                    await AlertInteraction.Handle(("��¼ʧ��", message, type));
                }
                else
                {
                    const string unknownMessage = "��¼ʧ�ܣ������������˿���Ӧ��";
                    ErrorMessage = unknownMessage;
                    await AlertInteraction.Handle(("��¼ʧ��", unknownMessage, NotificationType.Error));
                }
            }
            catch (Exception e)
            {
                const string exceptionMessage = "��¼�����з����������Ժ����ԡ�";
                ErrorMessage = exceptionMessage;
                await AlertInteraction.Handle(("��¼ʧ��", exceptionMessage, NotificationType.Error));
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

