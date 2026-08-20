using Contracts.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
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
                ShowLoginWindow();
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

        private void ShowLoginWindow()
        {
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            if (loginWindow.DataContext is LoginWindowViewModel viewModel)
            {
                viewModel.LoginSucceeded += () => OpenMainWindow(loginWindow);
            }

            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
        }

        public void OpenMainWindow(Window loginAsWindow)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var session = _serviceProvider.GetRequiredService<BLL.Interfaces.ISessionService>();
                var isManager = string.Equals(session.CurrentUser?.RoleName, "Manager", StringComparison.OrdinalIgnoreCase);

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                TxpTrace.WriteLine(
                    $"[TXP] - Resolved MainWindow in {stopwatch.ElapsedMilliseconds} ms for role {(isManager ? "Manager" : "Cashier")}");
                if (mainWindow.DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.LogoutRequested += () =>
                    {
                        // Detach this MainViewModel from the singleton
                        // NavigationService BEFORE the next MainViewModel
                        // is constructed; otherwise the old subscription
                        // stays alive, resubscribes to the next manager
                        // VM's LogoutRequested, and the confirmation
                        // MessageBox ends up being shown N+1 times.
                        mainViewModel.UnloadFromNavigation();

                        ShowLoginWindow();
                        mainWindow.Close();
                    };
                }

                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                TxpTrace.WriteLine(
                    $"[TXP] - Displayed MainWindow in {stopwatch.ElapsedMilliseconds} ms for role {(isManager ? "Manager" : "Cashier")}");
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
