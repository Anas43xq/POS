
using Contracts.Transactions;
using DAL.Entities;

namespace DAL.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<PagedResult<TransactionListItemDto>> GetTransactionsListAsync(
            GetTransactionsListRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Voids a transaction atomically. Issues a single
        /// <c>UPDATE Transactions SET Status = 2 WHERE TransactionId = @id AND Status = 1</c>
        /// so the status flip is race-safe (the status guard is in the WHERE clause).
        /// Returns true if exactly one row was updated, false otherwise.
        /// </summary>
        Task<bool> VoidTransactionAsync(int transactionId, CancellationToken ct = default);
    }
}
