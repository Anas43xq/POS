using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.Views
{
    /// <summary>
    /// Action bar used by both VAT and Non-VAT purchase receipt
    /// tabs.  Exposes every command as a DependencyProperty so
    /// the consumer can wire the VM directly to the toolbar.
    ///
    /// Inherits the host page's DataContext; the AddCommand is
    /// always enabled, while Edit / View / Delete are gated on
    /// <see cref="HasSelection"/>.
    /// </summary>
    public partial class PurchaseReceiptToolbarControl : UserControl
    {
        // ── Commands ──
        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata(null));

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata(null));

        public static readonly DependencyProperty ViewCommandProperty =
            DependencyProperty.Register(nameof(ViewCommand), typeof(ICommand), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata(null));

        public static readonly DependencyProperty ExportCommandProperty =
            DependencyProperty.Register(nameof(ExportCommand), typeof(ICommand), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata(null));

        // ── HasSelection: gates Edit / View / Delete ──
        public static readonly DependencyProperty HasSelectionProperty =
            DependencyProperty.Register(
                nameof(HasSelection),
                typeof(bool),
                typeof(PurchaseReceiptToolbarControl),
                new FrameworkPropertyMetadata(false));

        // ── Labels (so the same toolbar is reused across VAT / Non-VAT) ──
        public static readonly DependencyProperty AddLabelProperty =
            DependencyProperty.Register(nameof(AddLabel), typeof(string), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata("Add"));

        public static readonly DependencyProperty EditLabelProperty =
            DependencyProperty.Register(nameof(EditLabel), typeof(string), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata("Edit"));

        public static readonly DependencyProperty ViewLabelProperty =
            DependencyProperty.Register(nameof(ViewLabel), typeof(string), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata("View"));

        public static readonly DependencyProperty DeleteLabelProperty =
            DependencyProperty.Register(nameof(DeleteLabel), typeof(string), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata("Delete"));

        public static readonly DependencyProperty ExportLabelProperty =
            DependencyProperty.Register(nameof(ExportLabel), typeof(string), typeof(PurchaseReceiptToolbarControl), new PropertyMetadata("Export"));

        public PurchaseReceiptToolbarControl()
        {
            InitializeComponent();
        }

        public ICommand? AddCommand
        {
            get => (ICommand?)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        public ICommand? EditCommand
        {
            get => (ICommand?)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public ICommand? ViewCommand
        {
            get => (ICommand?)GetValue(ViewCommandProperty);
            set => SetValue(ViewCommandProperty, value);
        }

        public ICommand? DeleteCommand
        {
            get => (ICommand?)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public ICommand? ExportCommand
        {
            get => (ICommand?)GetValue(ExportCommandProperty);
            set => SetValue(ExportCommandProperty, value);
        }

        public bool HasSelection
        {
            get => (bool)GetValue(HasSelectionProperty);
            set => SetValue(HasSelectionProperty, value);
        }

        public string AddLabel
        {
            get => (string)GetValue(AddLabelProperty);
            set => SetValue(AddLabelProperty, value);
        }

        public string EditLabel
        {
            get => (string)GetValue(EditLabelProperty);
            set => SetValue(EditLabelProperty, value);
        }

        public string ViewLabel
        {
            get => (string)GetValue(ViewLabelProperty);
            set => SetValue(ViewLabelProperty, value);
        }

        public string DeleteLabel
        {
            get => (string)GetValue(DeleteLabelProperty);
            set => SetValue(DeleteLabelProperty, value);
        }

        public string ExportLabel
        {
            get => (string)GetValue(ExportLabelProperty);
            set => SetValue(ExportLabelProperty, value);
        }
    }
}
