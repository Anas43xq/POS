using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

            // Design-time / no-container safety: skip resolution if the
            // app's ServiceProvider isn't available yet.
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = App.ServiceProvider.GetService(typeof(INotificationService));
            }
        }

        private void OnDismissClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ToastMessage toast } &&
                DataContext is INotificationService notificationService)
            {
                notificationService.Dismiss(toast);
            }
        }
    }
}
