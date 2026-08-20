using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SettingsViewModel old)
            old.PinResetRequested -= ClearPinBoxes;

        if (e.NewValue is SettingsViewModel vm)
            vm.PinResetRequested += ClearPinBoxes;
    }

    private void ClearPinBoxes()
    {
        PinBox.Clear();
        PinConfirmBox.Clear();
    }

    private void PinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.PinEntry = ((PasswordBox)sender).Password;
    }

    private void PinConfirmBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.PinConfirmEntry = ((PasswordBox)sender).Password;
    }
}