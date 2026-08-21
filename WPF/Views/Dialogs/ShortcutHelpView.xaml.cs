using System.Windows;
using UI.ViewModels;

namespace UI.Views;

public partial class ShortcutHelpView : Window
{
    public ShortcutHelpView()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            if (DataContext is ShortcutHelpViewModel vm)
                vm.DialogClosed += () => Close();
        };
    }
}
