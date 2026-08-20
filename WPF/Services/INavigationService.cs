using System;

namespace UI.Services
{
    public interface INavigationService
    {
        object? CurrentViewModel { get; }

        event Action? CurrentViewModelChanged;

        void NavigateTo<TViewModel>() where TViewModel : class;
    }
}
