﻿using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Reactive.Disposables;
using ReactiveUI;
using Splat;

namespace ChatApp.Client.ViewModels;

public class ViewModelBase : ReactiveObject, IActivatableViewModel, IRoutableViewModel
{
    public ViewModelActivator Activator { get; init; }

    public RoutingState Router { get; }

    public string UrlPathSegment { get; }

    public IScreen HostScreen { get; }

    public ViewModelBase(RoutingState router)
    {
        Router = router;
        HostScreen = Locator.Current.GetService<IScreen>();
        UrlPathSegment = this.GetType().Name.Replace("ViewModel", "");

        Activator = new ViewModelActivator();
        this.WhenActivated((CompositeDisposable disposables) =>
        {
            /* handle activation */
            Disposable
                .Create(() => { Disappearing(); })
                .DisposeWith(disposables);
        });
    }

    public virtual void Disappearing()
    {
    }


}
