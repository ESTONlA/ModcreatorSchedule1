using System;
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
            // Log CanExecute for debugging (can be filtered in debugger if needed)
            System.Diagnostics.Debug.WriteLine($"[RelayCommand] CanExecute called for method: {_execute?.Method?.Name ?? "unknown"}, result: {result}");
            return result;
        }

        public void Execute(object? parameter)
        {
            System.Diagnostics.Debug.WriteLine($"[RelayCommand] Execute called for method: {_execute?.Method?.Name ?? "unknown"}");
            try
            {
                _execute();
            }
            catch (Exception ex)
            {
                // Log exception - in WPF apps, unhandled exceptions in commands are often silently swallowed
                System.Diagnostics.Debug.WriteLine($"[RelayCommand] Exception in Execute: {ex.Message}\n{ex.StackTrace}");
                throw; // Re-throw to allow proper error handling in the command handler
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