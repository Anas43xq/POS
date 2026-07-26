using Contracts.Transactions;

namespace DAL.Interfaces
{
    public interface ITransactionCommandRepository
    {
        Task<int> CreateTransactionAsync(CreateTransactionRequest request);

        Task SaveTransactionItemModifiersAsync(int transactionId, IEnumerable<CreateTransactionItemRequest> items);
    }
}