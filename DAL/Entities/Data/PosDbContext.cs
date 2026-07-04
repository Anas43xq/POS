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

        public DbSet<Shift> Shifts { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<TransactionItem> TransactionItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

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
