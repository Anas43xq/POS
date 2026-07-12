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
                var isEditingProp = vm.GetType().GetProperty("IsEditing");
                bool isEditing = isEditingProp?.GetValue(vm) is true;

                if (isEditing)
                {
                    var cancelProp = vm.GetType().GetProperty("CancelCommand");
                    if (cancelProp?.GetValue(vm) is ICommand cancelCmd && cancelCmd.CanExecute(null))
                    {
                        cancelCmd.Execute(null);
                        return;
                    }
                }
                else
                {
                    var closeProp = vm.GetType().GetProperty("CloseCommand");
                    if (closeProp?.GetValue(vm) is ICommand closeCmd && closeCmd.CanExecute(null))
                    {
                        closeCmd.Execute(null);
                        return;
                    }
                }
            }

            Close();
        }
    }
}