using System.Windows;
using UI.ViewModels;

namespace UI.Views
{
    public partial class ManagerPinOverlayView : Window
    {
        public ManagerPinOverlayView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ManagerPinOverlayViewModel old)
            {
                old.CloseRequested -= Close;
                old.PinClearRequested -= OnPinClearRequested;
            }

            if (e.NewValue is ManagerPinOverlayViewModel vm)
            {
                vm.CloseRequested += Close;
                vm.PinClearRequested += OnPinClearRequested;
            }
        }

        private void OnPinClearRequested()
        {
            // Dot indicators reset automatically via PinLength binding.
            // No separate visual state to clear in the numpad-based layout.
            // If a physical-keyboard shortcut is added later that focuses
            // a text input, clear it here.
        }
    }
}
