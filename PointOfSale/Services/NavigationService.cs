using Microsoft.Extensions.DependencyInjection;
using System;

namespace UI.Services
{
    /// <summary>
    /// Manages navigation between different ViewModels in the application.
    /// This is the single source of truth for which ViewModel is currently displayed.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private object? _currentViewModel;

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set => _currentViewModel = value;
        }

        public event Action? CurrentViewModelChanged;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Navigates to a ViewModel of the specified type.
        /// Resolves it from the DI container and sets it as current.
        /// </summary>
        public void NavigateTo<TViewModel>() where TViewModel : class
        {
            var previous = _currentViewModel;

            var vm = _serviceProvider.GetRequiredService<TViewModel>();
            CurrentViewModel = vm;
            CurrentViewModelChanged?.Invoke();

            // Dispose the outgoing ViewModel after the new one is already
            // wired up and the UI has been notified, so any IDisposable
            // ViewModel (e.g. one subscribed to a singleton's event) can
            // unsubscribe and become collectible instead of being kept
            // alive by the singleton for the lifetime of the app.
            if (!ReferenceEquals(previous, vm) && previous is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
