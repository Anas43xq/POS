using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace UI.Services
{
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
            TxpTrace.WriteLine($"[TOAST] Show - {type}: '{message}'");

            if (string.IsNullOrWhiteSpace(message))
                return;

            var toast = new ToastMessage(message, type);

            RunOnUiThread(() =>
            {
                _toasts.Add(toast);

                // Error toasts persist until dismissed (✕ / Dismiss); all
                // other types auto-dismiss after DisplayDuration.
                if (type == ToastType.Error)
                {
                    TxpTrace.WriteLine("[TOAST] Error toast persists until dismissed");
                    return;
                }

                var timer = new DispatcherTimer { Interval = DisplayDuration };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    _toasts.Remove(toast);
                    TxpTrace.WriteLine("[TOAST] auto-dismissed");
                };
                timer.Start();
            });
        }

        private void RunOnUiThread(Action action)
        {
            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.BeginInvoke(action);
        }
    }
}
