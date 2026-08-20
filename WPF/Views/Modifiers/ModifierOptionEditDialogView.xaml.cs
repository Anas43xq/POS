using System.Windows;
using System.Windows.Input;
using UI.ViewModels;

namespace UI.Views.Modifiers;

public partial class ModifierOptionEditDialogView : Window
{
    public ModifierOptionEditDialogView()
    {
        InitializeComponent();

        ContentRendered += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };

        Loaded += (_, _) =>
        {
            if (DataContext is ModifierGroupManagementViewModel vm)
                vm.RequestClose = Close;

            if (Owner != null)
            {
                Left = Owner.Left;
                Top = Owner.Top;
                Width = Owner.ActualWidth;
                Height = Owner.ActualHeight;
            }
        };

        Closed += (_, _) =>
        {
            if (DataContext is ModifierGroupManagementViewModel vm)
                vm.RequestClose = null;
        };
    }

    private void OverlayGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (ReferenceEquals(e.Source, OverlayGrid))
            CloseDialog();
    }

    private void DialogCard_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void ModifierOptionEditDialogView_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            CloseDialog();
    }

    private void CloseDialog()
    {
        if (DataContext is ModifierGroupManagementViewModel vm && vm.CancelOptionEditCommand.CanExecute(null))
        {
            vm.CancelOptionEditCommand.Execute(null);
            return;
        }

        Close();
    }
}
