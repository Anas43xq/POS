using DAL.Entities.Data;
using DAL.Repositories;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDalServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PosDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        services.AddDbContextFactory<PosDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITaxRateRepository, TaxRateRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionItemRepository, TransactionItemRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionCommandRepository, TransactionCommandRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPurchaseReceiptRepository, PurchaseReceiptRepository>();
        services.AddScoped<IRecentSaleRepository, RecentSaleRepository>();
        services.AddScoped<IRecentTransactionRepository, RecentTransactionRepository>();
        services.AddScoped<ITopProductRepository, TopProductRepository>();
        services.AddScoped<IShiftSummaryRepository, ShiftSummaryRepository>();
        services.AddScoped<IShiftManagementRepository, ShiftManagementRepository>();
        services.AddScoped<IKpiRepository, KpiRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        return services;
    }
}
