using System;
using System.Collections.Generic;
using System.Text;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using POS.Contracts.Printing;

namespace BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBLL(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ITransactionItemService, TransactionItemService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IShiftManagementService, ShiftManagementService>();
        services.AddScoped<ITaxRateService, TaxRateService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITopProductService, TopProductService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPurchaseReceiptService, PurchaseReceiptService>();
        services.AddScoped<IRecentSaleService, RecentSaleService>();
        services.AddScoped<IRecentTransactionService, RecentTransactionService>();
        services.AddScoped<IShiftSummaryService, ShiftSummaryService>();
        services.AddScoped<IKpiService, KpiService>();
        services.AddSingleton<ICurrencyService, CurrencyService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddScoped<ICategoryTranslationService, CategoryTranslationService>();
        services.AddScoped<IProductTranslationService, ProductTranslationService>();
        services.AddScoped<ISizeTranslationService, SizeTranslationService>();
        services.AddScoped<ISizeService, SizeService>();
        services.AddScoped<IModifierService, ModifierService>();
        services.AddScoped<ICartModifierService, CartModifierService>();

        services.AddSingleton<ISettingsService, UI.Services.SettingsService>();
        services.AddSingleton<IPrintingService, PrintingService>();

        return services;
    }
}

