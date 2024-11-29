using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.Client.DTOs;
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

        private RegisterUserDto _registerUserDto => new RegisterUserDto
        {
            Username = Username,
            Password = Passcode
        };
        
        private LoginUserDto _loginUserDto => new LoginUserDto
        {
            Username = Username,
            Password = Passcode
        };

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
                var loginResult = await chatService.LoginUser(_loginUserDto);
                if (loginResult != null)
                {
                    Router.Navigate.Execute(new ChatListModel(_currentUserId, Router));
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
                var loginResult = await chatService.RegisterUser(_registerUserDto);
                if (loginResult != null)
                {
                    Router.Navigate.Execute(new ChatListModel(_currentUserId, Router));
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
        private Guid _currentUserId;
    }
}
