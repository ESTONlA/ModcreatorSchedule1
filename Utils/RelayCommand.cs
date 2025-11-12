using System.Windows.Input;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// A command implementation for MVVM pattern
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            var result = _canExecute?.Invoke() ?? true;
            return result;
        }

        public void Execute(object? parameter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[RelayCommand] Execute called for action: {_execute.Method.Name}");
                _execute();
                System.Diagnostics.Debug.WriteLine($"[RelayCommand] Execute completed for action: {_execute.Method.Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RelayCommand] Exception in Execute for {_execute.Method.Name}: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[RelayCommand] Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }

    /// <summary>
    /// A parameterized command implementation for MVVM pattern
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            try
            {
                _execute((T?)parameter);
            }
            catch (Exception ex)
            {
                // Log exception - in WPF apps, unhandled exceptions in commands are often silently swallowed
                System.Diagnostics.Debug.WriteLine($"RelayCommand<T> exception: {ex.Message}\n{ex.StackTrace}");
                throw; // Re-throw to allow proper error handling in the command handler
            }
        }
    }
}