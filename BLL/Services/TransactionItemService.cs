using DAL.Entities;
using DAL.Interfaces;
using BLL.Interfaces;

namespace BLL.Services
{
    public class TransactionItemService : ITransactionItemService
    {
        private readonly ITransactionItemRepository _transactionitemrepo;

        public TransactionItemService(ITransactionItemRepository TransactionItemRepo)
        {
            _transactionitemrepo = TransactionItemRepo;
        }

        public async Task<IEnumerable<TransactionItem>> GetAllTransactionItemsAsync() =>
            await _transactionitemrepo.GetAllAsync();

        public async Task<TransactionItem?> GetTransactionItemByIdAsync(int id) =>
            await _transactionitemrepo.GetByIdAsync(id);

        public async Task AddTransactionItemAsync(TransactionItem TransactionItem) =>
            await _transactionitemrepo.AddAsync(TransactionItem);

        public async Task UpdateTransactionItemAsync(TransactionItem TransactionItem) =>
            await _transactionitemrepo.UpdateAsync(TransactionItem);

        public async Task DeleteTransactionItemAsync(int id) =>
            await _transactionitemrepo.DeleteAsync(id);
    }
}