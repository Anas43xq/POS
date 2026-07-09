using System.Windows;
using UI.ViewModels;

namespace UI.Views
{
    public partial class LoginAsWindow : Window
    {
        public LoginAsWindow(LoginAsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
