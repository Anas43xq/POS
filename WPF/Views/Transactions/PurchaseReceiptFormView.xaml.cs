using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using UI.ViewModels;

namespace UI.Views
{
    public partial class PurchaseReceiptFormView : UserControl
    {
        public PurchaseReceiptFormView()
        {
            InitializeComponent();
            PreviewKeyDown += PurchaseReceiptFormView_PreviewKeyDown;
        }

        /// <summary>Handles Enter as Save unless focus is already in a multiline editor or button.</summary>
        private void PurchaseReceiptFormView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (Keyboard.FocusedElement is ButtonBase)
                return;

            if (Keyboard.FocusedElement is TextBoxBase textBox && textBox.AcceptsReturn)
                return;

            if (DataContext is ReceiptManagementViewModel viewModel && viewModel.SaveReceiptCommand.CanExecute(null))
            {
                viewModel.SaveReceiptCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
