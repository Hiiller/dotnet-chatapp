using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.Client.Services;


namespace ChatApp.Client.ViewModels
{
    // 管理用户登录/注册功能的视图模型
    public class WelcomeViewModel : ViewModelBase
    {
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

        public string Passcode
        {
            get => passcode;
            set => this.RaiseAndSetIfChanged(ref passcode, value);
        }

        public ICommand ConnectCommand { get; private set; }

        public ICommand RegisterCommand { get; private set; }

        public ICommand LoginCommand { get; private set; }

        public bool Connected
        {
            get => connected;
            set => this.RaiseAndSetIfChanged(ref connected, value);
        }

        public WelcomeViewModel(RoutingState router) : base(router)
        {
            ServerUrl = "Your Server URL";
            RegisterCommand = ReactiveCommand.CreateFromTask(Register);
            LoginCommand = ReactiveCommand.CreateFromTask(Login);
            ConnectCommand = ReactiveCommand.CreateFromTask(Connect);
        }


        private async Task Connect()
        {
            try
            {
                chatService = new ChatService(ServerUrl);
                chatService.ConnectionState.Subscribe(x =>
                {
                    if (x == HubConnectionState.Connected)
                        Connected = true;
                    else
                        Connected = false;
                });
                
                await chatService.ConnectAsync();
                Connected = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task Login()
        {
            try
            {
                var loginResult = await chatService.LoginAsync(Username, Passcode);
                if (loginResult != null)
                {
                    Router.Navigate.Execute(new ChatViewModel(chatService, Router));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task Register()
        {
            try
            {
                var loginResult = await chatService.RegisterAndLogIn(Username, Passcode);
                if (loginResult != null)
                {
                    Router.Navigate.Execute(new ChatViewModel(chatService, Router));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        //Fields
        private ChatService chatService;
        private string username;
        private string passcode;
        private string serverUrl;
        private bool connected;
    }
}
