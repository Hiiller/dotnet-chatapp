using System;
using System.IO;
using System.Reactive;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using ChatApp.Client.ViewModels;
using ChatApp.Client.Views;
using ChatApp.Client.Services;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Splat;
using ChatApp.Client.Helpers;
using static ChatApp.Client.Helpers.DebugLogger;
using System.Reactive;

namespace ChatApp.Client;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Log("App", "Application starting - DebugLogger is working!");
        Log("App", $"Log file location: {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChatApp", "debug.log")}");

        // Add global exception handling
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Log("App", $"UNHANDLED EXCEPTION: {e.ExceptionObject}");
            if (e.ExceptionObject is Exception ex)
            {
                Log("App", $"Exception Message: {ex.Message}");
                Log("App", $"Exception StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Log("App", $"InnerException: {ex.InnerException.Message}");
                }
            }
        };
        
        RxApp.DefaultExceptionHandler = System.Reactive.Observer.Create<Exception>(ex =>
        {
            Log("App", $"ReactiveUI EXCEPTION: {ex.Message}");
            Log("App", $"StackTrace: {ex.StackTrace}");
        });

        // Create the AutoSuspendHelper
        var suspension = new AutoSuspendHelper(ApplicationLifetime);
        //设置了 MainWindowViewModel 作为应用程序的初始状态
        RxApp.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
        //设置了如何处理应用状态的序列化和反序列化
        RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
        suspension.OnFrameworkInitializationCompleted();

        //MainWindowViewModel 作为根视图模型加载
        Locator.CurrentMutable.RegisterConstant<IScreen>(RxApp.SuspensionHost.GetAppState<MainWindowViewModel>());
        
        Locator.CurrentMutable.Register<IViewFor<MainViewModel>>(() => new MainView());
        Locator.CurrentMutable.Register<IViewFor<ChatViewModel>>(() => new ChatView());
        Locator.CurrentMutable.Register<IViewFor<WelcomeViewModel>>(() => new WelcomeView());
        Locator.CurrentMutable.Register<IViewFor<ChatListModel>>(() => new ChatList());
        // Register missing views for routing
        Locator.CurrentMutable.Register<IViewFor<RegisterViewModel>>(() => new RegisterView());
        Locator.CurrentMutable.Register<IViewFor<ProfileViewModel>>(() => new ProfileView());
        Locator.CurrentMutable.Register<IViewFor<SearchFriendsViewModel>>(() => new SearchFriendsView());
        Locator.CurrentMutable.RegisterLazySingleton<IHubService>(() => new HubService("global"));
        Locator.CurrentMutable.RegisterLazySingleton<IAssetProvider>(() => new AssetProvider());

        //创建了 MainWindow 窗口，并将其 DataContext 设置为 IScreen（即根视图模型 MainWindowViewModel）
        var mainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
        // 确保 ApplicationLifetime 的 MainWindow 被正确设置，供文件选择对话框等功能使用
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainWindow;
        }
        mainWindow.Show();

        base.OnFrameworkInitializationCompleted();
    }
}
