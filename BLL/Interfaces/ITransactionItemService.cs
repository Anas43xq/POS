using DAL.Entities;

namespace BLL.Interfaces
{
    public interface ITransactionItemService
    {
        Task<IEnumerable<TransactionItem>> GetAllTransactionItemsAsync();

        Task<TransactionItem?> GetTransactionItemByIdAsync(int id);

        Task AddTransactionItemAsync(TransactionItem TransactionItem);

        Task UpdateTransactionItemAsync(TransactionItem TransactionItem);

        Task DeleteTransactionItemAsync(int id);
    }
}