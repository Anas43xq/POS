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
                DataContext = viewModel
            };
            SetOwner(window);
            window.ShowDialog();
        }

        public bool? ShowDialogWithResult<TView>(object viewModel) where TView : Window, new()
        {
            var window = new TView
            {
                DataContext = viewModel
            };
            SetOwner(window);
            return window.ShowDialog();
        }

        /// <summary>
        /// Assigns the owner window if one is available and is not the dialog itself.
        /// Guards against the WPF "Cannot set Owner property to itself" exception that
        /// can occur when <see cref="Application.Current"/>'s MainWindow has already
        /// been closed, was never assigned, or otherwise resolves to the new window.
        /// </summary>
        private static void SetOwner(Window window)
        {
            var owner = Application.Current?.MainWindow;
            if (owner != null && !ReferenceEquals(owner, window))
            {
                window.Owner = owner;
            }
        }
    }
}
