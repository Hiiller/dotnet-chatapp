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
    
        // Constructor for command without parameters
        public RelayCommand(Action execute) : this(execute, null) { }

        // Constructor for command with parameters
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Constructor for command with parameterized actions
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute_ = execute;
            _canExecute_ = canExecute;
        }
        
        // Custom event to notify if the command can execute
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

        // Manually trigger CanExecuteChanged event, e.g., when a relevant property changes
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

}