using System.Windows;
using UI.ViewModels;

namespace UI.Views
{
    public partial class ReceiptWindow : Window
    {
        public ReceiptWindow(ReceiptViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
