using System.Windows;
using System.Windows.Input;
using UI.ViewModels;

namespace UI.Views;

public partial class EditShortcutDialogWindow : Window
{
    private EditShortcutDialogViewModel? _viewModel;

    public EditShortcutDialogWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is EditShortcutDialogViewModel vm)
        {
            _viewModel = vm;
            vm.RequestClose += OnRequestClose;
        }
    }

    private void OnRequestClose(bool result)
    {
        DialogResult = result;
        Close();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        _viewModel?.HandleKeyDown(e.Key, Keyboard.Modifiers);
        e.Handled = true;
    }

    private void OverlayGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is System.Windows.Shapes.Path)
            return;
        DragMove();
    }

    private void DialogCard_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}
