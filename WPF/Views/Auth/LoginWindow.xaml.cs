using System.Windows;
using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.PasswordResetRequested += OnPasswordResetRequested;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox &&
                DataContext is LoginWindowViewModel viewModel)
            {
                viewModel.Password = passwordBox.Password;
            }
        }

        private void OnPasswordResetRequested()
        {
            PasswordBox.Clear();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
