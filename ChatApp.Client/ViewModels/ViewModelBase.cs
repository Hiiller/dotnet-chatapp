using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Reactive.Disposables;
using ReactiveUI;
using Splat;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChatApp.Client.ViewModels
{
    public class ViewModelBase : ReactiveObject, IActivatableViewModel, IRoutableViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
        
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}