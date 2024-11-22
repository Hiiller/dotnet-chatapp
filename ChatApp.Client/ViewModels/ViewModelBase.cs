using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModelBase : INotifyPropertyChanged
{
    // INotifyPropertyChanged 事件
    public event PropertyChangedEventHandler? PropertyChanged;

    // 触发属性更改事件
    protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}