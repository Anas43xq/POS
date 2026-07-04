using System;
using System.Windows;

namespace UI.Services
{
    /// <summary>
    /// Interface for managing dialog display in MVVM pattern.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a modal dialog with the given view model.
        /// </summary>
        void ShowDialog<TView>(object viewModel) where TView : Window, new();

        /// <summary>
        /// Shows a modal dialog and waits for result.
        /// </summary>
        bool? ShowDialogWithResult<TView>(object viewModel) where TView : Window, new();
    }

    /// <summary>
    /// Implementation of IDialogService for WPF.
    /// </summary>
    public class DialogService : IDialogService
    {
        public void ShowDialog<TView>(object viewModel) where TView : Window, new()
        {
            var window = new TView
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        public bool? ShowDialogWithResult<TView>(object viewModel) where TView : Window, new()
        {
            var window = new TView
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };
            return window.ShowDialog();
        }
    }
}
