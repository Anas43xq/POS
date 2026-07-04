using Contracts.Sales;
using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class RecentTransactionRepository : IRecentTransactionRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public RecentTransactionRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int take = 10)
        {
            await using var context = _contextFactory.CreateDbContext();

            return await context.vw_RecentTransactions
                .AsNoTracking()
                .OrderByDescending(x => x.TransactionDate)
                .Take(take)
                .Select(x => new RecentTransactionDto
                {
                    TransactionId = x.TransactionId,
                    ReceiptNumber = x.ReceiptNumber,
                    GrandTotal = x.GrandTotal,
                    PaymentMethod = x.PaymentMethod,
                    TransactionDate = x.TransactionDate,
                    CashierName = x.CashierName ?? "",
                    Status = (TransactionStatus)x.Status == TransactionStatus.Pending ? "Pending" : (TransactionStatus)x.Status == TransactionStatus.Completed ? "Completed" : "Voided"
                })
                .ToListAsync();
        }
    }
}
