using BLL;
using BLL.Interfaces;
using BLL.Services;
using DAL;
using DAL.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        public App()
        {
            ServiceCollection services = new ServiceCollection();

            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();
            ServiceProvider = _serviceProvider;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // DAL
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDalServices(connectionString!);

            // BLL
            services.AddBLL();

            // UI Services
            services.AddTransient<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISessionService, SessionService>();
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
            services.AddTransient<UI.Views.ShiftManagementView>();
            services.AddTransient<CashierDashboardView>();
            services.AddTransient<ManagerMainView>();
            services.AddTransient<CategoryManagementView>();
            services.AddTransient<StartDayDialog>();
            services.AddTransient<EndDayDialog>();
            services.AddTransient<PaymentDialog>();
            services.AddTransient<RecentSalesDialog>();
            services.AddTransient<ReceiptWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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

            // Temporary startup flow for manager testing.
            // Keep the original login flow commented out for later restoration.

            //var loginView = _serviceProvider.GetRequiredService<LoginView>();

            //if (loginView.DataContext is LoginViewModel loginViewModel)
            //{
            //    loginViewModel.LoginSucceeded += () =>
            //    {
            //        try
            //        {
            //            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            //            if (mainWindow.DataContext is MainViewModel mainViewModel)
            //            {
            //                mainViewModel.LogoutRequested += () =>
            //                {
            //                    var newLoginView = _serviceProvider.GetRequiredService<LoginView>();
            //                    newLoginView.Show();
            //                    mainWindow.Close();
            //                };
            //            }
            //            mainWindow.Show();
            //            loginView.Close();
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.WriteLine($"Error opening main window: {ex.Message}\n{ex.StackTrace}");
            //        }
            //    };
            //}

            //loginView.Show();

            var sessionService = _serviceProvider.GetRequiredService<ISessionService>();
            sessionService.CurrentUser = new User
            {
                //    UserId = 3,
                //    FullName = "System Manager",
                //    Username = "manager",
                //    PasswordHash = "$2a$12$ZTDNXVRX/BZtJPmElXQ8buwjOmNwCWmzxVnBvOZ98FVL3.AJDVVsi",
                //    RoleId = 2,
                //    IsActive = true,
                //    CreatedAt = DateTime.Parse("2026-06-23 14:12:36"),
                //    UpdatedAt = DateTime.Parse("2026-06-23 14:12:36"),
                //    Role = new Role
                //    {
                //        RoleId = 2,
                //        RoleName = "Manager",
                //        CreatedAt = DateTime.UtcNow
                //    }
                //};

                UserId = 2,
                FullName = "System Cashier",
                Username = "cashier",
                PasswordHash = "$2a$12$Zwju2F5K5taFZu3qp3BxleeFyLW5t6qWN1dbEloAo1v/RjjsgPFwC",
                RoleId = 3,
                IsActive = true,
                CreatedAt = DateTime.Parse("2026-06-23 14:12:36"),
                UpdatedAt = DateTime.Parse("2026-06-23 14:12:36"),
                Role = new Role
                {
                    RoleId = 3,
                    RoleName = "Cashier",
                    CreatedAt = DateTime.UtcNow
                }
            };

            sessionService.CurrentShift = new Shift
            {
                ShiftId = 4,
                UserId = 2,
                OpenedAt = DateTime.Parse("2026-06-27 16:35:36"),
                ClosedAt = null,
                OpeningCash = 1000.00m,
                ClosingCash = null,
                ExpectedCash = null,
                CashDifference = null,
                Status = ShiftStatus.Open
            };

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

        }
    }
}
