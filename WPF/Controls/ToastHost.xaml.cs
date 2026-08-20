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
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            DataContext = App.ServiceProvider.GetRequiredService<INotificationService>();
        }

        private void OnDismissClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ToastMessage toast } &&
                DataContext is INotificationService notificationService)
            {
                notificationService.Dismiss(toast);
            }
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: ToastMessage toast } button)
                return;

            try
            {
                Clipboard.SetText(toast.Message);
            }
            catch
            {
                // Clipboard access can transiently fail (locked by another
                // process); not worth surfacing a second toast for this.
                return;
            }

            // Brief visual feedback: swap the label to a checkmark, then
            // restore it after a short delay.
            string original = button.Content?.ToString() ?? "Copy";
            button.Content = "✓";

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.2) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                button.Content = original;
            };
            timer.Start();
        }
    }
}
