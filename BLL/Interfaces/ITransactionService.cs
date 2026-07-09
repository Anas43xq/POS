using BLL.Models;
using Contracts.Transactions;
using DAL.Entities;

namespace BLL.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedResult<TransactionListItemDto>> GetTransactionsListAsync(
            GetTransactionsListRequest request,
            CancellationToken ct = default);

        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();

        Task<Transaction?> GetTransactionByIdAsync(int id);

        Task UpdateTransactionAsync(Transaction transaction);

        Task DeleteTransactionAsync(int id);

        Task<int> CreateTransactionAsync(CreateTransactionRequest request);

        /// <summary>
        /// Voids a completed transaction, flipping its status to <see cref="DAL.Entities.TransactionStatus.Voided"/>.
        /// Returns a failure Result if the transaction does not exist or is not in <see cref="DAL.Entities.TransactionStatus.Completed"/>.
        /// </summary>
        Task<Result<DAL.Entities.Transaction>> VoidTransactionAsync(int transactionId);
    }
}
