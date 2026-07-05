using BLL;
using BLL.Interfaces;
using BLL.Services;
using DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using UI.Services;
using UI.ViewModels;
using UI.Views;

namespace UI
{
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;

        private readonly ServiceProvider _serviceProvider;
        private readonly ILogger<App>? _logger;
        public App()
        {
            ServiceCollection services = new ServiceCollection();

            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();
            ServiceProvider = _serviceProvider;
            _logger = _serviceProvider.GetService<ILogger<App>>();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddLogging();

            // DAL
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDalServices(connectionString!);

            // BLL
            services.AddBLL();

            // UI Services
            services.AddTransient<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<IReceiptDisplayService, ReceiptDisplayService>();
            services.AddTransient<ExcelReportExporter>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<CashierDashboardViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<TransactionsViewModel>();
            services.AddTransient<ShiftManagementViewModel>();
            services.AddTransient<ReportViewModel>();
            services.AddTransient<ProductManagementViewModel>();
            services.AddTransient<CategoryManagementViewModel>();
            services.AddTransient<ReceiptManagementViewModel>();
            services.AddTransient<ManagerMainViewModel>();
            services.AddTransient<StartDayDialogViewModel>();
            services.AddTransient<EndDayDialogViewModel>();
            services.AddTransient<PaymentDialogViewModel>();
            services.AddTransient<RecentSalesDialogViewModel>();
            services.AddTransient<ReceiptPrinterService>();

            // Views / Windows
            services.AddTransient<LoginView>();
            services.AddTransient<MainWindow>();
            services.AddTransient<HomeView>();
            services.AddTransient<TransactionsView>();
            services.AddTransient<ShiftManagementView>();
            services.AddTransient<CashierDashboardView>();
            services.AddTransient<ManagerMainView>();
            services.AddTransient<CategoryManagementView>();
            services.AddTransient<ReceiptManagementView>();
            services.AddTransient<StartDayDialog>();
            services.AddTransient<EndDayDialog>();
            services.AddTransient<PaymentDialog>();
            services.AddTransient<RecentSalesDialog>();
            services.AddTransient<ReceiptWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;

            CultureInfo uaeCulture = new CultureInfo("en-AE");
            NumberFormatInfo aedFormat = (NumberFormatInfo)uaeCulture.NumberFormat.Clone();
            aedFormat.CurrencySymbol = "AED";
            aedFormat.CurrencyPositivePattern = 2; // AED 1.00
            aedFormat.CurrencyNegativePattern = 12; // -AED 1.00
            uaeCulture.NumberFormat = aedFormat;

            CultureInfo.DefaultThreadCurrentCulture = uaeCulture;
            CultureInfo.DefaultThreadCurrentUICulture = uaeCulture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage("en-AE")));

            var loginView = _serviceProvider.GetRequiredService<LoginView>();
            if (loginView.DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginSucceeded += () =>
                {
                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    if (mainWindow.DataContext is MainViewModel mainViewModel)
                    {
                        mainViewModel.LogoutRequested += () =>
                        {
                            var newLoginView = _serviceProvider.GetRequiredService<LoginView>();
                            newLoginView.Show();
                            mainWindow.Close();
                        };
                    }

                    mainWindow.Show();
                    loginView.Close();
                };
            }

            loginView.Show();
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogUnhandledException(e.Exception, "Dispatcher");
            ShowFatalError("An unexpected application error occurred.");
            e.Handled = true;
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                LogUnhandledException(exception, "AppDomain");
            }
            else
            {
                LogUnhandledException(new Exception("Unhandled non-exception object was raised."), "AppDomain");
            }

            ShowFatalError("An unexpected application error occurred.");
        }

        private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogUnhandledException(e.Exception, "TaskScheduler");
            e.SetObserved();
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            _logger?.LogError(exception, "Unhandled exception from {Source}", source);

            try
            {
                if (!EventLog.SourceExists("PointOfSale"))
                {
                    return;
                }

                EventLog.WriteEntry("PointOfSale", $"{source}: {exception}", EventLogEntryType.Error);
            }
            catch
            {
                // Event Log access is best-effort only.
            }
        }

        private static void ShowFatalError(string message)
        {
            MessageBox.Show(message, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
