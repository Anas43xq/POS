using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DAL.Entities.Data
{
    public class PosDbContext: DbContext
    {
        public DbSet<Role> Roles { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<TaxRate> TaxRates { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Size> Sizes { get; set; }

        public DbSet<ProductVariant> ProductVariants { get; set; }

        public DbSet<ProductTranslation> ProductTranslations { get; set; }

        public DbSet<CategoryTranslation> CategoryTranslations { get; set; }

        public DbSet<SizeTranslation> SizeTranslations { get; set; }

        public DbSet<Shift> Shifts { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<TransactionItem> TransactionItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<PurchaseReceiptType> PurchaseReceiptTypes { get; set; }

        public DbSet<PurchaseReceipt> PurchaseReceipts { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<ModifierGroup> ModifierGroups { get; set; }
        public DbSet<ModifierOption> ModifierOptions { get; set; }
        public DbSet<ModifierGroupTranslation> ModifierGroupTranslations { get; set; }
        public DbSet<ModifierOptionTranslation> ModifierOptionTranslations { get; set; }
        public DbSet<CategoryModifierGroup> CategoryModifierGroups { get; set; }
        public DbSet<ProductModifierGroup> ProductModifierGroups { get; set; }
        public DbSet<TransactionItemModifier> TransactionItemModifiers { get; set; }

        public DbSet<RecentTransactionView> vw_RecentTransactions { get; set; }
        public DbSet<ShiftSummaryView> VwShiftSummaries { get; set; }
        public DbSet<ShiftManagementView> VwShiftManagement { get; set; }

        public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) 
        {
           
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }


    }
}
