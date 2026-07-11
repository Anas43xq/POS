using System.Windows;
using System.Windows.Input;

namespace UI.Views
{
    public partial class TranslationDialogView : Window
    {
        public TranslationDialogView()
        {
            InitializeComponent();
        }

        private void OverlayGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == OverlayGrid)
                CloseDialog();
        }

        private void DialogCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

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
    }
}