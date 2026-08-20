using BLL.DTOs;
using BLL.Models;
using Contracts.Transactions;

namespace BLL.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedResult<TransactionListItemDto>> GetTransactionsListAsync(
            GetTransactionsListRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Finds transactions whose receipt number contains the given text
        /// (case-insensitive). Used by the mobile receipt-search screen.
        /// </summary>
        Task<IEnumerable<TransactionListItemDto>> SearchByReceiptNumberAsync(
            string receiptNumber,
            CancellationToken ct = default);

        Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync();

        Task<TransactionDto?> GetTransactionByIdAsync(int id);

        /// <summary>
        /// Returns the full detail projection (header, payment, line items with
        /// modifiers) for a transaction, or null if it doesn't exist.
        /// </summary>
        Task<TransactionDetailDto?> GetTransactionDetailAsync(int id, CancellationToken ct = default);

        Task UpdateTransactionAsync(TransactionDto transaction);

        Task DeleteTransactionAsync(int id);

        Task<Result<int>> CreateTransactionAsync(CreateTransactionRequest request);

        /// <summary>
        /// Voids a completed transaction.
        /// Returns a failure Result if the transaction does not exist, is not completed,
        /// A void reason may be omitted.
        /// </summary>
        Task<Result<TransactionDto>> VoidTransactionAsync(int transactionId, string? voidReason);
    }
}