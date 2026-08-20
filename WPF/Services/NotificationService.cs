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
            TxpTrace.WriteLine($"[TOAST] NotificationService.Show — type={type}, message='{message}'");

            if (string.IsNullOrWhiteSpace(message))
            {
                TxpTrace.WriteLine("[TOAST] NotificationService.Show — ABORT: message is null/whitespace");
                return;
            }

            var toast = new ToastMessage(message, type);

            TxpTrace.WriteLine($"[TOAST] NotificationService.Show — dispatching to UI thread (CheckAccess={_dispatcher.CheckAccess()})");
            RunOnUiThread(() =>
            {
                TxpTrace.WriteLine($"[TOAST] NotificationService.Show — UI thread: adding toast, _toasts.Count before={_toasts.Count}");
                _toasts.Add(toast);
                TxpTrace.WriteLine($"[TOAST] NotificationService.Show — UI thread: _toasts.Count after={_toasts.Count}");

                // Error toasts stay until dismissed (✕ / Dismiss); all other
                // types auto-dismiss after DisplayDuration.
                if (type == ToastType.Error)
                {
                    TxpTrace.WriteLine("[TOAST] NotificationService.Show — Error toast: no auto-dismiss");
                    return;
                }

                var timer = new DispatcherTimer { Interval = DisplayDuration };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    _toasts.Remove(toast);
                    TxpTrace.WriteLine("[TOAST] NotificationService.Show — auto-dismissed toast");
                };
                timer.Start();
                TxpTrace.WriteLine($"[TOAST] NotificationService.Show — auto-dismiss timer started ({DisplayDuration.TotalSeconds}s)");
            });
        }

        private void RunOnUiThread(Action action)
        {
            if (_dispatcher.CheckAccess())
            {
                TxpTrace.WriteLine("[TOAST] NotificationService.RunOnUiThread — already on UI thread, running inline");
                action();
            }
            else
            {
                TxpTrace.WriteLine("[TOAST] NotificationService.RunOnUiThread — off UI thread, BeginInvoke-ing");
                _dispatcher.BeginInvoke(action);
            }
        }
    }
}
