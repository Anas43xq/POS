using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace UI.Services
{
    /// <summary>
    /// DI-singleton implementation of <see cref="INotificationService"/>.
    /// Maintains an observable collection of currently-visible toasts and
    /// auto-dismisses each one after <see cref="DisplayDuration"/>.
    /// <para>
    /// Registered as a singleton so any ViewModel (transient or otherwise)
    /// can push notifications into a single, always-hosted visual
    /// (<c>ToastHost</c>, hosted once in <c>MainWindow</c>).
    /// </para>
    /// </summary>
    public sealed class NotificationService : INotificationService
    {
        private static readonly TimeSpan DisplayDuration = TimeSpan.FromSeconds(4);

        private readonly ObservableCollection<ToastMessage> _toasts = new();
        private readonly Dispatcher _dispatcher;

        public NotificationService()
        {
            // Captured at construction time (app startup, on the UI
            // thread) so later calls from any thread can marshal back
            // correctly.
            _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            Toasts = new ReadOnlyObservableCollection<ToastMessage>(_toasts);
        }

        public ReadOnlyObservableCollection<ToastMessage> Toasts { get; }

        public void ShowSuccess(string message) => Show(message, ToastType.Success);

        public void ShowError(string message) => Show(message, ToastType.Error);

        public void ShowWarning(string message) => Show(message, ToastType.Warning);

        public void ShowInfo(string message) => Show(message, ToastType.Info);

        public void Dismiss(ToastMessage toast)
        {
            RunOnUiThread(() => _toasts.Remove(toast));
        }

        private void Show(string message, ToastType type)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var toast = new ToastMessage(message, type);

            RunOnUiThread(() =>
            {
                _toasts.Add(toast);

                var timer = new DispatcherTimer { Interval = DisplayDuration };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    _toasts.Remove(toast);
                };
                timer.Start();
            });
        }

        private void RunOnUiThread(Action action)
        {
            if (_dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _dispatcher.BeginInvoke(action);
            }
        }
    }
}
