using System.Windows;
using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.IsKeyboardShortcutsVisible = false;
        }
    }
}
