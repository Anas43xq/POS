using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ITransactionItemService
    {
        Task<IEnumerable<TransactionItemDto>> GetAllTransactionItemsAsync();

        Task<TransactionItemDto?> GetTransactionItemByIdAsync(int id);

        Task AddTransactionItemAsync(TransactionItemDto TransactionItem);

        Task UpdateTransactionItemAsync(TransactionItemDto TransactionItem);

        Task DeleteTransactionItemAsync(int id);
    }
}