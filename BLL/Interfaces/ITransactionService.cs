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
    }
}
