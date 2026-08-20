using System.Windows;
using System.Windows.Input;

namespace UI.Views
{
    /// <summary>
    /// Code-behind for AddEditCategoryDialog.
    /// Handles UI-only concerns:
    ///   1. Clicking the semi-transparent overlay closes the dialog (same as Cancel).
    ///   2. Auto-focus the Name field when the dialog opens.
    /// All data/validation logic lives in AddEditCategoryViewModel.
    /// </summary>
    public partial class AddEditCategoryDialog : Window
    {
        public AddEditCategoryDialog()
        {
            InitializeComponent();

            // Auto-focus the Name field when the dialog finishes rendering
            ContentRendered += (_, _) =>
            {
                NameBox.Focus();
                NameBox.SelectAll();
            };

            Loaded += (_, _) =>
            {
                if (Owner != null)
                {
                    Left = Owner.Left;
                    Top = Owner.Top;
                    Width = Owner.ActualWidth;
                    Height = Owner.ActualHeight;
                }
            };
        }

        // ── Overlay click → close (same as Cancel) ──────────────────────────
        private void OverlayGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Only act on direct clicks on the overlay, not bubbled events from the card
            if (e.Source == OverlayGrid)
                CloseDialog();
        }

        // ── Stop card clicks from bubbling up to the overlay ─────────────────
        private void DialogCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        // ── Close helper: invokes CancelCommand on the VM if available, ───────
        //    otherwise closes directly so the dialog always dismisses cleanly.
        private void CloseDialog()
        {
            if (DataContext is { } vm)
            {
                var cancelProp = vm.GetType().GetProperty("CancelCommand");
                if (cancelProp?.GetValue(vm) is ICommand cmd && cmd.CanExecute(null))
                {
                    cmd.Execute(null);
                    return;
                }
            }

            Close();
        }

        // ── Keyboard shortcuts: Enter = Save, Escape = Cancel ─────────────────
        private void AddEditCategoryDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseDialog();
            }
            else if (e.Key == Key.Enter)
            {
                if (DataContext is { } vm)
                {
                    var saveProp = vm.GetType().GetProperty("SaveCommand");
                    if (saveProp?.GetValue(vm) is ICommand cmd && cmd.CanExecute(null))
                    {
                        cmd.Execute(null);
                    }
                }
            }
        }
    }
}
