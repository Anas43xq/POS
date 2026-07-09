using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using UI.ViewModels;
using UI.Views;

namespace UI.Services
{
    /// <summary>
    /// Default <see cref="IApplicationShellService"/> implementation.
    /// Lives in the application layer alongside the other navigation
    /// services; kept free of any UI controls so it can be unit-tested
    /// if needed (a <c>Func<Window></c> factory seam would be
    /// the next step if unit tests are introduced).
    /// </summary>
    public sealed class ApplicationShellService : IApplicationShellService
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationShellService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Start()
        {
            var loginAsWindow = _serviceProvider.GetRequiredService<LoginAsWindow>();
            if (loginAsWindow.DataContext is LoginAsViewModel vm)
            {
                vm.ManagerLoginSucceeded += () => OpenMainWindow(loginAsWindow);
                vm.CashierLoginSucceeded += () => OpenMainWindow(loginAsWindow);
            }
            loginAsWindow.Show();
        }

        public void OpenMainWindow(Window loginAsWindow)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            if (mainWindow.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.LogoutRequested += () =>
                {
                    // Logout: spin up a fresh Login-As window.
                    var newLoginAs = _serviceProvider.GetRequiredService<LoginAsWindow>();
                    if (newLoginAs.DataContext is LoginAsViewModel vm)
                    {
                        vm.ManagerLoginSucceeded += () => OpenMainWindow(newLoginAs);
                        vm.CashierLoginSucceeded += () => OpenMainWindow(newLoginAs);
                    }
                    newLoginAs.Show();
                    mainWindow.Close();
                };
            }

            mainWindow.Show();
            loginAsWindow.Close();
        }
    }
}
