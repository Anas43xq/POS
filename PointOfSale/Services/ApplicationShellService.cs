using Contracts.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using UI.ViewModels;
using UI.Views;

namespace UI.Services
{
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
                _logger.LogCritical(ex, "Failed to start application shell");
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
                // Switch keyboard shortcut profile based on user role
                var session = _serviceProvider.GetRequiredService<BLL.Interfaces.ISessionService>();
                var isManager = string.Equals(session.CurrentUser?.RoleName, "Manager", StringComparison.OrdinalIgnoreCase);

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                if (mainWindow.DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.LogoutRequested += () =>
                    {
                        // Reset to Cashier profile on logout

                        var newLoginAs = _serviceProvider.GetRequiredService<LoginAsWindow>();
                        if (newLoginAs.DataContext is LoginAsViewModel loginVm)
                        {
                            loginVm.ManagerLoginSucceeded += () => OpenMainWindow(newLoginAs);
                            loginVm.CashierLoginSucceeded += () => OpenMainWindow(newLoginAs);
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
                _logger.LogCritical(ex, "Failed to open main window");
                MessageBox.Show(
                    $"Failed to open main window:\n\n{ex.Message}\n\nThe application will return to the login screen.",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
