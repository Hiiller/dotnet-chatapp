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
        Router = new RoutingState();
        Router.Navigate.Execute(new MainViewModel(Router));
    }

    private ChatService chatService;
}
