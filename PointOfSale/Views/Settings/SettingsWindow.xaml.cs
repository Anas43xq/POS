using System.Windows;
using System.Windows.Input;
using UI.ViewModels;

namespace UI.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        HookCloseRequested(viewModel);
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is SettingsViewModel vm)
            HookCloseRequested(vm);
    }

    private void HookCloseRequested(SettingsViewModel vm)
    {
        vm.CloseRequested += () => Close();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            if (DataContext is SettingsViewModel vm)
            {
                vm.SaveCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void OverlayGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.Source == OverlayGrid)
            Close();
    }

    private void DialogCard_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}