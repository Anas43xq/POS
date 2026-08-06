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

        /// <param name="execute">The async work to run.</param>
        /// <param name="canExecute">Optional gate; the command is also
        /// disabled automatically while a previous invocation is still
        /// running.</param>
        /// <param name="onError">
        /// Optional centralized error handler (e.g.
        /// <c>ex => _notifications.ShowError(ex.Message)</c>). When
        /// provided, an exception thrown by <paramref name="execute"/> is
        /// caught here instead of propagating — callers no longer need to
        /// wrap every command body in its own try/catch/notify block.
        /// <para>
        /// When omitted (the default, and the behavior of every existing
        /// call site in the app today), an exception is NOT caught and
        /// bubbles out of this <c>async void</c> method to the app's
        /// global unhandled-exception handler, exactly as before this
        /// parameter was added.
        /// </para>
        /// </param>
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

        /// <param name="execute">The async work to run.</param>
        /// <param name="canExecute">Optional gate; the command is also
        /// disabled automatically while a previous invocation is still
        /// running.</param>
        /// <param name="onError">
        /// Optional centralized error handler — see
        /// <see cref="AsyncRelayCommand(Func{Task}, Func{bool}, Action{Exception})"/>
        /// for the full explanation. Omitted by default, preserving every
        /// existing call site's current behavior exactly.
        /// </param>
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
