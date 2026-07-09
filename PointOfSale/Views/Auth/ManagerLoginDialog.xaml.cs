using System.Windows;
using System.Windows.Controls;
using DAL.Entities;
using UI.ViewModels;

namespace UI.Views
{
    public partial class ManagerLoginDialog : Window
    {
        public ManagerLoginDialog()
        {
            InitializeComponent();

            // The dialog is always shown via IDialogService.
            // ShowDialogWithResult<TView>, which constructs the
            // window with the parameterless ctor and then sets
            // DataContext to the VM. We wire the lifecycle here
            // (rather than in a ctor) because the VM is not
            // available until after construction.
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(
            object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ManagerLoginViewModel oldVm)
            {
                oldVm.LoginSucceeded -= OnLoginSucceeded;
            }

            if (e.NewValue is ManagerLoginViewModel newVm)
            {
                newVm.LoginSucceeded += OnLoginSucceeded;
            }
        }

        private void OnLoginSucceeded(User user)
        {
            // Closing with DialogResult=true lets IDialogService.
            // ShowDialogWithResult return true to its caller.
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Forwards the <see cref="PasswordBox.Password"/> value into the
        /// ViewModel. This is the one accepted code-behind exception for
        /// <c>PasswordBox</c>: WPF does not expose <c>Password</c> as a
        /// dependency property, so binding directly is impossible. We
        /// keep the handler minimal — it never persists the password.
        /// </summary>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox &&
                DataContext is ManagerLoginViewModel vm)
            {
                vm.Password = passwordBox.Password;
            }
        }
    }
}
