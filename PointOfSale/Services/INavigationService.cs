using System;

namespace UI.Services
{
    /// <summary>
    /// Defines the contract for navigation between ViewModels.
    /// Fully decouples navigation logic from ViewModels.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// The current ViewModel being displayed.
        /// </summary>
        object? CurrentViewModel { get; }

        /// <summary>
        /// Fired when CurrentViewModel changes.
        /// </summary>
        event Action? CurrentViewModelChanged;

        /// <summary>
        /// Navigates to a ViewModel of type TViewModel.
        /// </summary>
        void NavigateTo<TViewModel>() where TViewModel : class;
    }
}
