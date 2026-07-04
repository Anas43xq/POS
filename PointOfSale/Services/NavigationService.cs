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
            var vm = _serviceProvider.GetRequiredService<TViewModel>();
            CurrentViewModel = vm;
            CurrentViewModelChanged?.Invoke();
        }
    }
}
