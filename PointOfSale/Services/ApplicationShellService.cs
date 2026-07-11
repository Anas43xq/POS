using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ApplicationShellService> _logger;

        public ApplicationShellService(
            IServiceProvider serviceProvider,
            ILogger<ApplicationShellService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void Start()
        {
            try
            {
                var loginAsWindow = _serviceProvider.GetRequiredService<LoginAsWindow>();
                if (loginAsWindow.DataContext is LoginAsViewModel vm)
                {
                    vm.ManagerLoginSucceeded += () => OpenMainWindow(loginAsWindow);
                    vm.CashierLoginSucceeded += () => OpenMainWindow(loginAsWindow);
                }
                loginAsWindow.Show();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to start application shell — a required service could not be resolved. Check that all ViewModels and Views are registered in App.xaml.cs DI.");
                MessageBox.Show(
                    $"Application failed to start:\n\n{ex.Message}\n\nSee event log for details.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        public void OpenMainWindow(Window loginAsWindow)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to open main window — a required service could not be resolved. Check that all ViewModels and Views are registered in App.xaml.cs DI.");
                MessageBox.Show(
                    $"Failed to open main window:\n\n{ex.Message}\n\nThe application will return to the login screen.",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
