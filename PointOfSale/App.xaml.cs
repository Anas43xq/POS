using BLL;
using BLL.Interfaces;
using BLL.Services;
using DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using POS.Contracts.Printing;
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
    /// <summary>
    /// Composition root. Builds the DI container, configures the
    /// runtime culture / unhandled-exception hooks, and hands
    /// control of the application shell to
    /// <see cref="IApplicationShellService"/>. All login and
    /// navigation orchestration lives in that service, not here.
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;

        private readonly ServiceProvider _serviceProvider;
        private readonly ILogger<App>? _logger;
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PointOfSale",
            "ErrorLog.txt");
            
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
            services.AddSingleton<IReceiptPrinter, ReceiptPrinter>();
            services.AddTransient<ExcelReportExporter>();
            services.AddSingleton<IRegistryService, RegistryService>();
            services.AddSingleton<IApplicationShellService, ApplicationShellService>();
            services.AddSingleton<IKeyboardShortcutService, KeyboardShortcutService>();
            services.AddSingleton<ShortcutManager>();

            services.AddSingleton<BLL.Interfaces.ILocalizationService, UI.Services.LocalizationService>();

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
            services.AddTransient<SizeManagementViewModel>();
            services.AddTransient<ReceiptManagementViewModel>();
            services.AddTransient<ManagerMainViewModel>();
            services.AddTransient<StartDayDialogViewModel>();
            services.AddTransient<EndDayDialogViewModel>();
            services.AddTransient<PaymentDialogViewModel>();
            services.AddTransient<RecentSalesDialogViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<LoginAsViewModel>();
            services.AddTransient<ManagerLoginViewModel>();
            services.AddTransient<KeyboardShortcutsViewModel>();

            // Views / Windows
            services.AddTransient<LoginView>();
            services.AddTransient<LoginAsWindow>();
            services.AddTransient<ManagerLoginDialog>();
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
            services.AddTransient<SettingsView>();
            services.AddTransient<SettingsWindow>();
            services.AddTransient<EditShortcutDialogWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Cross-cutting: unhandled-exception hooks.
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;

            // Cross-cutting: culture (AED formatting, en-AE UI language).
            ConfigureCulture();

            // Cross-cutting: localization must initialize BEFORE any
            // view is shown, so the first window's resources resolve.
            _serviceProvider.GetRequiredService<BLL.Interfaces.ILocalizationService>()
                .Initialize();

            // Initialize shortcut gesture converter for dynamic binding.
            UI.Converters.ShortcutGestureConverter.Initialize(
                _serviceProvider.GetRequiredService<IKeyboardShortcutService>());

            // Load any user-customized shortcut bindings.
            var shortcutService = _serviceProvider.GetRequiredService<IKeyboardShortcutService>();
            _ = shortcutService.LoadCustomBindingsAsync(Contracts.Enum.ShortcutProfileType.Cashier);
            _ = shortcutService.LoadCustomBindingsAsync(Contracts.Enum.ShortcutProfileType.Manager);

            // Hand off to the shell service. Everything else — showing
            // Login-As, swapping to MainWindow, handling logout —
            // lives in IApplicationShellService, not here.
            _serviceProvider.GetRequiredService<IApplicationShellService>().Start();
        }

        private static void ConfigureCulture()
        {
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
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogUnhandledException(e.Exception, "Dispatcher");
            ShowFatalError($"Application Error\n\n{e.Exception.GetType().Name}: {e.Exception.Message}", e.Exception);
            e.Handled = true;
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                LogUnhandledException(exception, "AppDomain");
                ShowFatalError($"Application Error\n\n{exception.GetType().Name}: {exception.Message}", exception);
            }
            else
            {
                LogUnhandledException(new Exception("Unhandled non-exception object was raised."), "AppDomain");
                ShowFatalError("An unexpected application error occurred.", null);
            }
        }

        private void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogUnhandledException(e.Exception, "TaskScheduler");
            e.SetObserved();
            ShowFatalError($"Background Task Error\n\n{e.Exception.GetType().Name}: {e.Exception.Message}", e.Exception);
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            var errorDetails = new System.Text.StringBuilder();
            errorDetails.AppendLine($"=== Unhandled Exception [{source}] ===");
            errorDetails.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            errorDetails.AppendLine($"Exception Type: {exception.GetType().FullName}");
            errorDetails.AppendLine($"Message: {exception.Message}");
            errorDetails.AppendLine($"Source: {exception.Source}");
            errorDetails.AppendLine($"Target Site: {exception.TargetSite?.Name ?? "N/A"}");
            errorDetails.AppendLine($"HResult: 0x{exception.HResult:X8}");
            
            if (exception.StackTrace != null)
            {
                errorDetails.AppendLine($"StackTrace:\n{exception.StackTrace}");
            }
            
            if (exception.InnerException != null)
            {
                errorDetails.AppendLine($"\n--- Inner Exception ---");
                errorDetails.AppendLine($"Type: {exception.InnerException.GetType().FullName}");
                errorDetails.AppendLine($"Message: {exception.InnerException.Message}");
                if (exception.InnerException.StackTrace != null)
                {
                    errorDetails.AppendLine($"StackTrace:\n{exception.InnerException.StackTrace}");
                }
            }

            _logger?.LogError(exception, "Unhandled exception from {Source}\n{Details}", source, errorDetails.ToString());

            // Write to file-based log (always works)
            try
            {
                var logDirectory = Path.GetDirectoryName(LogFilePath);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                if (!string.IsNullOrEmpty(LogFilePath))
                {
                    File.AppendAllText(LogFilePath, errorDetails.ToString());
                    File.AppendAllText(LogFilePath, "\n" + new string('=', 80) + "\n\n");
                }
            }
            catch
            {
                // File logging is best-effort only
            }

            try
            {
                if (!EventLog.SourceExists("PointOfSale"))
                {
                    try
                    {
                        EventLog.CreateEventSource("PointOfSale", "Application");
                    }
                    catch
                    {
                        // Cannot create event source - may need admin rights
                    }
                }

                if (EventLog.SourceExists("PointOfSale"))
                {
                    EventLog.WriteEntry("PointOfSale", errorDetails.ToString(), EventLogEntryType.Error);
                }
            }
            catch
            {
                // Event Log access is best-effort only.
            }
        }

        private static void ShowFatalError(string message, Exception? exception)
        {
#if DEBUG
            // In DEBUG mode, show full exception details with stack trace
            if (exception != null)
            {
                var debugMessage = new System.Text.StringBuilder();
                debugMessage.AppendLine(message);
                debugMessage.AppendLine();
                debugMessage.AppendLine("== Debug Information ==");
                debugMessage.AppendLine();
                
                var currentEx = exception;
                int level = 1;
                while (currentEx != null)
                {
                    debugMessage.AppendLine($"[Level {level}] {currentEx.GetType().FullName}");
                    debugMessage.AppendLine($"Message: {currentEx.Message}");
                    debugMessage.AppendLine($"Source: {currentEx.Source}");
                    debugMessage.AppendLine($"Target Site: {currentEx.TargetSite?.Name ?? "N/A"}");
                    
                    if (currentEx.StackTrace != null)
                    {
                        debugMessage.AppendLine();
                        debugMessage.AppendLine("StackTrace:");
                        debugMessage.AppendLine(currentEx.StackTrace);
                    }
                    
                    if (currentEx.InnerException != null)
                    {
                        debugMessage.AppendLine();
                        debugMessage.AppendLine("--- Inner Exception ---");
                    }
                    
                    currentEx = currentEx.InnerException;
                    level++;
                }
                
                MessageBox.Show(debugMessage.ToString(), "Application Error (Debug Mode)", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(message, "Application Error (Debug Mode)", MessageBoxButton.OK, MessageBoxImage.Error);
            }
#else
            // In Release mode, show simplified message
            MessageBox.Show(message, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
        }
    }
} 
