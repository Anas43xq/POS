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

        Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync();

        Task<TransactionDto?> GetTransactionByIdAsync(int id);

        Task UpdateTransactionAsync(TransactionDto transaction);

        Task DeleteTransactionAsync(int id);

        Task<int> CreateTransactionAsync(CreateTransactionRequest request);

        /// <summary>
        /// Voids a completed transaction.
        /// Returns a failure Result if the transaction does not exist or is not completed.
        /// </summary>
        Task<Result<TransactionDto>> VoidTransactionAsync(int transactionId);
    }
}