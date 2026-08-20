using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using UI.Services;

namespace UI.Controls
{
    /// <summary>
    /// Self-contained toast overlay. Resolves <see cref="INotificationService"/>
    /// from the DI container and binds directly to its <c>Toasts</c>
    /// collection — it does not go through the hosting page's ViewModel,
    /// so it can be dropped into any window (see MainWindow.xaml) with no
    /// extra wiring.
    /// </summary>
    public partial class ToastHost : UserControl
    {
        public ToastHost()
        {
            InitializeComponent();

            // Binding the ItemsControl to {Binding Toasts} only works once
            // this control owns its own DataContext. If we defer that to
            // Loaded, the binding is first evaluated against the inherited
            // DataContext (MainWindow's MainViewModel, which has no Toasts)
            // and produces a BindingExpression path error (#40). App
            // builds ServiceProvider in its constructor, so it is guaranteed
            // to be available before any window — and therefore any ToastHost —
            // is created, making the constructor the earliest safe point.
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = App.ServiceProvider.GetRequiredService<INotificationService>();
            }
        }

        private void OnDismissClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ToastMessage toast } &&
                DataContext is INotificationService notificationService)
            {
                notificationService.Dismiss(toast);
                TxpTrace.WriteLine($"[TOAST] ToastHost.OnDismissClick — dismissed toast {toast.Id}");
            }

            // Always consume the click so it can never fall through to a
            // control underneath the toast overlay (e.g. the header Logout).
            e.Handled = true;
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: ToastMessage toast } button)
                return;

            try
            {
                Clipboard.SetText(toast.Message);
                TxpTrace.WriteLine($"[TOAST] ToastHost.OnCopyClick — copied message: {toast.Message}");
            }
            catch (Exception ex)
            {
                // Clipboard access can transiently fail (locked by another
                // process); not worth surfacing a second toast for this.
                TxpTrace.WriteLine($"[TOAST] ToastHost.OnCopyClick — clipboard error: {ex.Message}");
                e.Handled = true;
                return;
            }

            // Brief visual feedback: swap the label to a checkmark, then
            // restore it after a short delay.
            // Fetch the resolved resource value, not the binding expression type name.
            string original = (FindResource("Common.Copy") as string) ?? "Copy";
            button.Content = "✓";
            TxpTrace.WriteLine($"[TOAST] ToastHost.OnCopyClick — showing checkmark, will restore after 1.2s");

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.2) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                button.Content = original;
                TxpTrace.WriteLine($"[TOAST] ToastHost.OnCopyClick — restored button text to '{original}'");
            };
            timer.Start();

            // Consume the click so it never falls through to underlying controls.
            e.Handled = true;
        }
    }
}