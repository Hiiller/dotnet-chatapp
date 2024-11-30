using System;
using System.Windows.Input;

namespace ChatApp.Client.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        private readonly Action<object> _execute_;
        private readonly Func<object, bool> _canExecute_;
        public RelayCommand(Action execute) : this(execute, null) { }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute_ = execute;
            _canExecute_ = canExecute;
        }
        
        // 自定义事件触发机制，手动更新命令的可执行状态
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object? parameter)
        {
            if (_execute == null)
            {
                throw new InvalidOperationException("Execute delegate is not set.");
            }
            _execute();
        }

        // 可以在需要时手动触发 CanExecuteChanged 事件
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}