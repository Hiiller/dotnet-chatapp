using System;
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

namespace ChatApp.Client;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {

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
        Locator.CurrentMutable.RegisterLazySingleton<IHubService>(() => new HubService("global"));
        Locator.CurrentMutable.RegisterLazySingleton<IAssetProvider>(() => new AssetProvider());

        //创建了 MainWindow 窗口，并将其 DataContext 设置为 IScreen（即根视图模型 MainWindowViewModel）
        var mainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
        mainWindow.Show();
        
        base.OnFrameworkInitializationCompleted();
    }
}
