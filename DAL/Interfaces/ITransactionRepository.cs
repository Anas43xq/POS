
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
        /// Finds transactions whose receipt number contains the given text
        /// (case-insensitive), most recent first.
        /// </summary>
        Task<IEnumerable<TransactionListItemDto>> SearchByReceiptNumberAsync(
            string receiptNumber,
            CancellationToken ct = default);

        /// <summary>
        /// Loads a transaction with its line items (and each item's modifiers)
        /// and payments eagerly included. Returns null if no transaction with
        /// this id exists.
        /// </summary>
        Task<Transaction?> GetTransactionDetailAsync(int transactionId, CancellationToken ct = default);

        /// <summary>
        /// Voids a transaction atomically. Returns true if exactly one row
        /// was updated, false otherwise.
        /// </summary>
        Task<bool> VoidTransactionAsync(
            int transactionId,
            string? voidReason,
            CancellationToken ct = default);
    }
}
