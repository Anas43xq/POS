using System.Threading.Tasks;
using System.Windows.Input;

namespace UI.Commands
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private readonly Action<Exception>? _onError;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null, Action<Exception>? onError = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            _onError = onError;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute());
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();

                if (_onError == null)
                {
                    await _execute();
                }
                else
                {
                    try
                    {
                        await _execute();
                    }
                    catch (Exception ex)
                    {
                        _onError(ex);
                    }
                }
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Predicate<T?>? _canExecute;
        private readonly Action<Exception>? _onError;
        private bool _isExecuting;

        public AsyncRelayCommand(
            Func<T?, Task> execute,
            Predicate<T?>? canExecute = null,
            Action<Exception>? onError = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            _onError = onError;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting &&
                   (_canExecute == null || _canExecute((T?)parameter));
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();

                if (_onError == null)
                {
                    await _execute((T?)parameter);
                }
                else
                {
                    try
                    {
                        await _execute((T?)parameter);
                    }
                    catch (Exception ex)
                    {
                        _onError(ex);
                    }
                }
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
