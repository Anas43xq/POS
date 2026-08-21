using System.Windows;
using UI.ViewModels;

namespace UI.Views
{
    public partial class ReceiptWindow : Window
    {
        public ReceiptWindow()
        {
            InitializeComponent();
        }

        public ReceiptWindow(ReceiptViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Owner?.Activate();
            base.OnClosing(e);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
