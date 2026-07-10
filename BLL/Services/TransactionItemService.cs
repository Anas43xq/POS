using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class TransactionItemService : ITransactionItemService
    {
        private readonly ITransactionItemRepository _transactionitemrepo;

        public TransactionItemService(ITransactionItemRepository TransactionItemRepo)
        {
            _transactionitemrepo = TransactionItemRepo;
        }

        public async Task<IEnumerable<TransactionItemDto>> GetAllTransactionItemsAsync()
        {
            var entities = await _transactionitemrepo.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<TransactionItemDto?> GetTransactionItemByIdAsync(int id)
        {
            var entity = await _transactionitemrepo.GetByIdAsync(id);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task AddTransactionItemAsync(TransactionItemDto item) =>
            await _transactionitemrepo.AddAsync(MapToEntity(item));

        public async Task UpdateTransactionItemAsync(TransactionItemDto item) =>
            await _transactionitemrepo.UpdateAsync(MapToEntity(item));

        public async Task DeleteTransactionItemAsync(int id) =>
            await _transactionitemrepo.DeleteAsync(id);

        private static TransactionItemDto MapToDto(TransactionItem e) => new()
        {
            TransactionItemId = e.TransactionItemId,
            TransactionId = e.TransactionId,
            VariantId = e.VariantId,
            ProductName = e.ProductName,
            UnitPrice = e.UnitPrice,
            Quantity = e.Quantity,
            TaxRate = e.TaxRate,
            LineSubtotal = e.LineSubtotal,
            LineTax = e.LineTax,
            LineTotal = e.LineTotal
        };

        private static TransactionItem MapToEntity(TransactionItemDto d) => new()
        {
            TransactionItemId = d.TransactionItemId,
            TransactionId = d.TransactionId,
            VariantId = d.VariantId,
            ProductName = d.ProductName,
            UnitPrice = d.UnitPrice,
            Quantity = d.Quantity,
            TaxRate = d.TaxRate,
            LineSubtotal = d.LineSubtotal,
            LineTax = d.LineTax,
            LineTotal = d.LineTotal
        };
    }
}