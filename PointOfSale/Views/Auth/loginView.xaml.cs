using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.ViewModels;

namespace UI.Views
{
    public partial class LoginView : Window
    {
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox &&
                DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = passwordBox.Password;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EyeToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isPasswordVisible = PasswordTextBox.Visibility == Visibility.Visible;

            if (isPasswordVisible)
            {
                // Switch back to PasswordBox
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Focus();
            }
            else
            {
                // Switch to TextBox (plain text)
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.Focus();
            }
        }
    }
}
