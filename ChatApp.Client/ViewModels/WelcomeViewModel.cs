using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.Client.DTOs;
using ChatApp.Client.Services;
using System.Net.Http;
using Avalonia.Threading;

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
        
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ICommand ConnectCommand { get; }

        public ICommand RegisterCommand { get; private set; }

        public ICommand LoginCommand { get; private set; }

        public bool Connected
        {
            get => connected;
            set
            {
                this.RaiseAndSetIfChanged(ref connected, value);
                Console.WriteLine($"Connected set to: {value}");  // Debug log to checkMessages
            }
        }

        public WelcomeViewModel(RoutingState router) : base(router)
        {
            ServerUrl = "http://localhost:5005";
            RegisterCommand = ReactiveCommand.CreateFromTask(Register);
            LoginCommand = ReactiveCommand.CreateFromTask(Login);
            ConnectCommand = ReactiveCommand.CreateFromTask(Connect);

        }


        private async Task Connect()
        {
            try
            {
                Console.WriteLine("Attempting to connect...");
            
                // 初始化 HttpClient 和 ChatService
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(ServerUrl)
                };
                chatService = new ChatService(httpClient);
                var response = await httpClient.GetAsync("/api/chat/ping");
                if (response.IsSuccessStatusCode)
                {
                    Connected = true; // 更新状态
                    Console.WriteLine("Connected to the http server successfully.");
                    Console.WriteLine($"Http Connected state updated: {Connected}");
                }
                else
                {
                    Console.WriteLine($"Failed to connect to the server. Status Code: {response.StatusCode}");
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine($"Connection error: {e.Message}");
            }
        }


        private async Task Login()
        {
            await Connect();
            try
            {
                await Connect();
                var loginResult = await chatService.LoginUser(_loginUserDto);
                if (loginResult != null)
                {
                    
                    if (loginResult.connectionStatus != false)
                    {
                        Router.Navigate.Execute(new ChatListModel(loginResult, Router));
                        ErrorMessage = string.Empty;
                    }
                    else
                    {
                        Console.WriteLine(loginResult?.errorCode);
                        ErrorMessage = loginResult?.errorCode switch
                        {
                            -1 => "Registration failed: Username already exists.",
                            -2 => "Login failed: Invalid username or password.",
                            -3 => "Server error: Please try again later.",
                            _ => "An unknown error occurred."
                        };
                    }
                }
            }
            catch (Exception e)
            {
                ErrorMessage = "An error occurred while attempting to log in.";
                Console.WriteLine(e);
                throw;
            }

        }

        private async Task Register()
        {
            await Connect();
            try
            {
                await Connect();
                var loginResult = await chatService.RegisterUser(_registerUserDto);
                if (loginResult != null)
                {
                    Console.WriteLine(loginResult.currentUserId);
                    Console.WriteLine(loginResult.currentUsername);
                    if (loginResult.connectionStatus != false)
                    {
                        Router.Navigate.Execute(new ChatListModel(loginResult, Router));
                    }
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
