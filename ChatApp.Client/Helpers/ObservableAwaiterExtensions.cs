using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ChatApp.Client.Helpers
{
    public static class ObservableAwaiterExtensions
    {
        public static TaskAwaiter<T> GetAwaiter<T>(this IObservable<T> source)
            => source.ToTask().GetAwaiter();
    }
}

