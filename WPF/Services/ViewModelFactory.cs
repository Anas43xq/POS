using System;
using Microsoft.Extensions.DependencyInjection;

namespace UI.Services
{
    /// <summary>
    /// Thin wrapper over <see cref="ActivatorUtilities.CreateInstance"/>.
    /// Registered as a singleton — it holds no state of its own beyond the
    /// root <see cref="IServiceProvider"/>, and every dialog/child
    /// ViewModel it creates is a fresh instance regardless of the
    /// factory's own lifetime.
    /// </summary>
    public sealed class ViewModelFactory : IViewModelFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Create<T>(params object[] parameters) where T : class
        {
            return ActivatorUtilities.CreateInstance<T>(_serviceProvider, parameters);
        }
    }
}
