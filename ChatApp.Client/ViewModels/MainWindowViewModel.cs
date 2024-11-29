using System;
using ReactiveUI;
using ChatApp.Client.Services;

namespace ChatApp.Client.ViewModels;

//负责应用程序的导航逻辑。MainWindowViewModel：主视图模型，定义了导航状态 Router
public class MainWindowViewModel : ReactiveObject, IScreen
{
    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; }


    public MainWindowViewModel()
    {
        Console.WriteLine("MainWindowViewModel constructor started.");
        Router = new RoutingState();
        Console.WriteLine("Router initialized.");

        Console.WriteLine("Navigating to MainViewModel...");
        Router.Navigate.Execute(new MainViewModel(Router));
        Console.WriteLine("Navigation executed.");
    }

    private ChatService chatService;
}
