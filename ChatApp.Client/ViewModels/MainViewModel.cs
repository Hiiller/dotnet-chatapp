﻿using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.Client.ViewModels
{
    //负责应用程序的导航逻辑。提供了导航到登录界面的命令。
    public class MainViewModel : ViewModelBase
    {
        public ICommand GetStartedCommand { get; private set; }

        public MainViewModel(RoutingState router) : base(router)
        {
            this.GetStartedCommand = ReactiveCommand.Create(NavigateToLogin);
        }

        private void NavigateToLogin()
        {
            Router.Navigate.Execute(new WelcomeViewModel(Router));
        }

    }
}
