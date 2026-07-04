
using Contracts.Transactions;
using DAL.Entities;

namespace DAL.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<PagedResult<TransactionListItemDto>> GetTransactionsListAsync(
            GetTransactionsListRequest request,
            CancellationToken ct = default);
    }
}
